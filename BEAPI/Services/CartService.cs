using AutoMapper;
using BEAPI.Dtos.Cart;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<CartItem> _cartItemRepo;
        private readonly IRepository<User> _userRepo;
        private readonly  IRepository<ProductVariant> _productVariantRepo;
        private readonly IMapper _mapper;

        public CartService(IRepository<Cart> cartRepo, IMapper mapper, IRepository<CartItem> cartItemRepo, IRepository<User> userRepo, IRepository<ProductVariant> productVariantRepo)
        {
            _cartRepo = cartRepo;
            _mapper = mapper;
            _cartItemRepo = cartItemRepo;
            _userRepo = userRepo;
            _productVariantRepo = productVariantRepo;
        }

        public async Task ChangeStatus(CartStatus cartStatus, string id)
        {
            var cartId = GuidHelper.ParseOrThrow(id, "cartId");

            var cart = await _cartRepo.Get().FirstOrDefaultAsync(x => x.Id == cartId) ?? throw new Exception("Cart not found");

            cart.Status = cartStatus;
            _cartRepo.Update(cart);
            await _cartRepo.SaveChangesAsync();
        }

        public async Task<CartDto?> GetCartByIdAsync(string id)
        {
            var cartId = GuidHelper.ParseOrThrow(id, "cartId");

            var cart = await _cartRepo.Get()
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Elder)
                .FirstOrDefaultAsync(x => x.Id == cartId);

            if (cart == null) return null;

            var items = await _cartItemRepo.Get()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductImages)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductVariantValues)
                        .ThenInclude(pvv => pvv.Value)
                .ToListAsync();

            var dto = new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer?.FullName ?? string.Empty,
                Status = cart.Status.ToString(),
                ElderId = cart.ElderId,
                ElderName = cart.Elder?.FullName,
                Items = items.Select(i =>
                {
                    var pv = i.ProductVariant;

                    var styles = pv?.ProductVariantValues != null
                        ? string.Join(", ",
                            pv.ProductVariantValues
                              .Select(pvv =>
                              {
                                  var label =
                                      (pvv.Value?.Label ??
                                       (pvv.GetType().GetProperty("ValueLabel")?.GetValue(pvv) as string) ??
                                       (pvv.GetType().GetProperty("ValueCode")?.GetValue(pvv) as string) ??
                                       string.Empty).Trim();

                                  return label;
                              })
                              .Where(s => !string.IsNullOrWhiteSpace(s)))
                        : string.Empty;

                    var imageUrl = pv?.ProductImages?
                        .Select(img => img.URL)
                        .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;

                    return new CartItemDto
                    {
                        ProductVariantId = i.ProductVariantId,
                        ProductName = pv?.Product?.Name ?? string.Empty,
                        Quantity = i.Quantity,
                        ProductPrice = i.ProductPrice,
                        Discount = (int)(pv?.Discount),
                        Styles = styles,
                        ImageUrl = imageUrl
                    };
                }).ToList()
            };

            return dto;
        }

        public async Task<List<CartDto>> GetCartsByCustomerIdAsync(string cusId, CartStatus cartStatus)
        {
            var customerId = GuidHelper.ParseOrThrow(cusId, nameof(cusId));

            var carts = await _cartRepo.Get()
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Elder)
                .Where(x => x.CustomerId == customerId && x.Status == cartStatus)
                .ToListAsync();

            if (!carts.Any()) return new List<CartDto>();

            // lấy tất cả cartId để query items 1 lần
            var cartIds = carts.Select(c => c.Id).ToList();

            var items = await _cartItemRepo.Get()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(ci => cartIds.Contains(ci.CartId))
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductImages)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductVariantValues)
                        .ThenInclude(pvv => pvv.Value)
                .ToListAsync();

            return carts.Select(cart => new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer?.FullName ?? string.Empty,
                Status = cart.Status.ToString(),
                ElderId = cart.ElderId,
                ElderName = cart.Elder?.FullName,
                Items = items
                    .Where(i => i.CartId == cart.Id)
                    .Select(i =>
                    {
                        var pv = i.ProductVariant;

                        var styles = pv?.ProductVariantValues != null
                            ? string.Join(", ",
                                pv.ProductVariantValues
                                  .Select(pvv =>
                                      (pvv.Value?.Label ??
                                       pvv.Value?.Description ??
                                       pvv.Value?.Code ??
                                       string.Empty).Trim())
                                  .Where(s => !string.IsNullOrWhiteSpace(s)))
                            : string.Empty;

                        var imageUrl = pv?.ProductImages?
                            .Select(img => img.URL)
                            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;

                        return new CartItemDto
                        {
                            ProductVariantId = i.ProductVariantId,
                            ProductName = pv?.Product?.Name ?? string.Empty,
                            Quantity = i.Quantity,
                            ProductPrice = i.ProductPrice,
                            Discount = (int)(pv?.Discount),
                            Styles = styles,
                            ImageUrl = imageUrl
                        };
                    }).ToList()
            }).ToList();
        }
        public async Task<List<CartDto>> GetCartsByElderIdAsync(string elderId, CartStatus cartStatus)
        {
            var elderGuid = GuidHelper.ParseOrThrow(elderId, nameof(elderId));

            var carts = await _cartRepo.Get()
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Elder)
                .Where(x => x.ElderId == elderGuid && x.Status == cartStatus)
                .ToListAsync();

            if (!carts.Any()) return new List<CartDto>();

            var cartIds = carts.Select(c => c.Id).ToList();

            var items = await _cartItemRepo.Get()
                .AsNoTracking()
                .AsSplitQuery()
                .Where(ci => cartIds.Contains(ci.CartId))
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductImages)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.ProductVariantValues)
                        .ThenInclude(pvv => pvv.Value)
                .ToListAsync();

            return carts.Select(cart => new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer?.FullName ?? string.Empty,
                Status = cart.Status.ToString(),
                ElderId = cart.ElderId,
                ElderName = cart.Elder?.FullName,
                Items = items
                    .Where(i => i.CartId == cart.Id)
                    .Select(i =>
                    {
                        var pv = i.ProductVariant;

                        var styles = pv?.ProductVariantValues != null
                            ? string.Join(", ",
                                pv.ProductVariantValues
                                  .Select(pvv =>
                                      (pvv.Value?.Label ??
                                       pvv.Value?.Description ??
                                       pvv.Value?.Code ??
                                       string.Empty).Trim())
                                  .Where(s => !string.IsNullOrWhiteSpace(s)))
                            : string.Empty;

                        var imageUrl = pv?.ProductImages?
                            .Select(img => img.URL)
                            .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;

                        return new CartItemDto
                        {
                            ProductVariantId = i.ProductVariantId,
                            ProductName = pv?.Product?.Name ?? string.Empty,
                            Quantity = i.Quantity,
                            ProductPrice = i.ProductPrice,
                            Discount = (int)(pv?.Discount),
                            Styles = styles,
                            ImageUrl = imageUrl
                        };
                    }).ToList()
            }).ToList();
        }


        public async Task ReplaceCartAsync(CartReplaceAllDto dto)
        {
            var customerId = GuidHelper.ParseOrThrow(dto.CustomerId, nameof(dto.CustomerId));
            var customer = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == customerId) ?? throw new Exception("Customer not found");
            var productVariantIds = dto.Items.Select(i => GuidHelper.ParseOrThrow(i.ProductVariantId)).ToList();

            var invalidProductVariants = await ValidationHelper.ValidateIdsExistAsync(_productVariantRepo, productVariantIds);
            if (invalidProductVariants.Any())
                throw new Exception($"Invalid Products: {string.Join(", ", invalidProductVariants)}");

            var priceDict = await _productVariantRepo.Get()
                    .Where(p => productVariantIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Price })
                    .ToDictionaryAsync(p => p.Id, p => p.Price);
            var cart = await _cartRepo.Get()
                 .Include(c => c.Items)
                 .FirstOrDefaultAsync(c =>
                     (customer.GuardianId != null
                         ? c.ElderId == customerId
                         : c.CustomerId == customerId)
                     && c.Status == CartStatus.Created);


            if (cart == null)
            {
                cart = new Cart { Status = CartStatus.Created };
                if (customer.GuardianId == null)
                {
                    cart.CustomerId = customerId;
                }else
                {
                    cart.ElderId = customerId;
                    cart.CustomerId = (Guid)customer.GuardianId;
                }
                await _cartRepo.AddAsync(cart);
            }
            else
            {
                cart.Items.Clear();
            }

            var newItems = _mapper.Map<List<CartItem>>(dto.Items);

            foreach (var item in newItems)
            {
                item.ProductPrice = priceDict[item.ProductVariantId];
                item.CartId = cart.Id;
            }

            await _cartItemRepo.AddRangeAsync(newItems);
            await _cartRepo.SaveChangesAsync();
        }

        public async Task<List<CartDto>> GetAllElderCarts(string userId)
        {
            var customerId = GuidHelper.ParseOrThrow(userId, "userId");

            var carts = await _cartRepo.Get()
                .Include(x => x.Customer)
                .Include(x => x.Elder)
                .Include(x => x.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Where(x => x.CustomerId == customerId && x.Elder != null)
                .ToListAsync();

            var result = carts.Select(cart => new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer.FullName,
                Status = cart.Status.ToString(),
                ElderId = cart.ElderId,
                ElderName = cart.Elder?.FullName ?? null,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name ?? "",
                    Quantity = i.Quantity,
                    ProductPrice = i.ProductPrice
                }).ToList()
            }).ToList();

            return result;
        }
    }
}

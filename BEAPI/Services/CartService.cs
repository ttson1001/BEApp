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
                .Include(x => x.Customer)
                .Include(x => x.Elder)
                .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == cartId);

            if (cart == null)
                return null;
            var listCart = await _cartItemRepo.Get()
                .Include(x => x.ProductVariant)
                .ThenInclude(x => x.Product)
                    .Where(x => x.CartId == cart.Id).ToArrayAsync();

            return new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer.FullName,
                Status = cart.Status.ToString(),
                ElderId = cart.Id,
                ElderName = cart?.Elder?.FullName ?? null,
                Items = listCart.Select(i => new CartItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name ?? "",
                    Quantity = i.Quantity,
                    ProductPrice = i.ProductPrice
                }).ToList()
            };
        }

        public async Task<CartDto?> GetCartByCustomerIdAsync(string cusId, CartStatus cartStatus)
        {
            var customerId = GuidHelper.ParseOrThrow(cusId, "cusId");
            
            var cart = await _cartRepo.Get().Include(x => x.Customer).Include(x => x.Elder)
                .Include(x => x.Items).FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Status == cartStatus);

            if (cart == null)
                return null;
            var listCart = await _cartItemRepo.Get()
               .Include(x => x.ProductVariant)
               .ThenInclude(x => x.Product)
                   .Where(x => x.CartId == cart.Id).ToArrayAsync();
            return new CartDto
            {
                CartId = cart.Id,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer.FullName,
                Status = cart.Status.ToString(),
                ElderId = cart.Id,
                ElderName = cart?.Elder?.FullName ?? null,
                Items = listCart.Select(i => new CartItemDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name ?? "",
                    Quantity = i.Quantity,
                    ProductPrice = i.ProductPrice
                }).ToList()
            };
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
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == CartStatus.Created);

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
                .Where(x => x.CustomerId == customerId && x.Status == CartStatus.Created && x.Elder != null)
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

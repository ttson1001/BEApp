using AutoMapper;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Order;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<ProductVariant> _productVariantRepo;
        private readonly IRepository<PaymentHistory> _paymentHistoryRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<Address> _addressRepo;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order> orderRepo,
                            IRepository<ProductVariant> productVariantRepo,
                            IRepository<User> userRepo,
                            IRepository<Cart> cartRepo,
                            IRepository<Address> addressRepo,
                            IRepository<PaymentHistory> paymentHistoryRepo,
                            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productVariantRepo = productVariantRepo;
            _userRepo = userRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
            _paymentHistoryRepo = paymentHistoryRepo;
            _addressRepo = addressRepo;
        }
        public async Task CreateOrderAsync(OrderCreateDto dto, bool isPaid)
        {
            var cartId = GuidHelper.ParseOrThrow(dto.CartId, nameof(dto.CartId));

            var cart = await _cartRepo.Get()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ProductVariant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(u => u.Id == cartId && u.Status == CartStatus.Pending)
                ?? throw new Exception("Cart not found or not in pending status");

            var price = cart.Items.Sum(x => x.ProductPrice);

            var orderDetails = cart.Items.Select(x => new OrderDetail
            {
                ProductVariant = x.ProductVariant,
                Price = x.ProductPrice,
                Quantity = x.Quantity,
                ProductName = x.ProductVariant.Product?.Name ?? "",
            }).ToList();

            var address = _addressRepo.Get().First();

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                ElderId = cart.ElderId,
                Note = dto.Note,
                OrderStatus = isPaid ? OrderStatus.Paid : OrderStatus.Fail,
                TotalPrice = price,
                OrderDetails = orderDetails,
                DistrictID = address.DistrictID,
                DistrictName = address.DistrictName,
                WardCode = address.WardCode,
                WardName = address.WardName,
                ProvinceID = address.ProvinceID,
                ProvinceName = address.ProvinceName
            };

            cart.Status = isPaid ? CartStatus.Approve : CartStatus.Pending;

            await _orderRepo.AddAsync(order);

            if (isPaid)
            {
                DeductStockFromCart(cart);

                var payment = new PaymentHistory
                {
                    Amount = price,
                    OrderId = order.Id,
                    UserId = cart.CustomerId,
                    PaymentMenthod = "VNPay",
                    paymentStatus = PaymentStatus.Success
                };
                await _paymentHistoryRepo.AddAsync(payment);

                var user = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == cart.CustomerId);
                if (user != null)
                {
                    var pointEarned = (int)(price / 1000);
                    user.RewardPoint += pointEarned;
                }
            }

            await _orderRepo.SaveChangesAsync();
        }

        //public async Task CreateOrderAsync(OrderCreateDto dto, bool isPaid)
        //{
        //    var cartId = GuidHelper.ParseOrThrow(dto.CartId, nameof(dto.CartId));

        //    var cart = await _cartRepo.Get()
        //        .Include(x => x.Items)
        //            .ThenInclude(x => x.ProductVariant)
        //                .ThenInclude(x => x.Product)
        //        .FirstOrDefaultAsync(u => u.Id == cartId && u.Status == CartStatus.Pending)
        //        ?? throw new Exception("Cart not found or not in pending status");

        //    UserPromotion? userPromo = null;
        //    Promotion? promo = null;
        //    if (dto.PromotionId.HasValue)
        //    {
        //        userPromo = await _userPromoRepo.Get()
        //            .Include(up => up.Promotion)
        //            .FirstOrDefaultAsync(up =>
        //                up.UserId == cart.CustomerId &&
        //                up.PromotionId == dto.PromotionId.Value &&
        //                !up.IsUsed);

        //        promo = userPromo?.Promotion ?? throw new Exception("Promotion invalid or already used");

        //        var now = DateTimeOffset.UtcNow;
        //        if (!promo.IsActive || (promo.StartAt.HasValue && now < promo.StartAt.Value) || (promo.EndAt.HasValue && now > promo.EndAt.Value))
        //            throw new Exception("Promotion not available");

        //        if (promo.DiscountPercent < 0 || promo.DiscountPercent > 100)
        //            throw new Exception("Invalid discount percent");
        //    }

        //    decimal multiplier = 1m;
        //    if (promo != null) multiplier = 1m - (promo.DiscountPercent / 100m);

        //    var orderDetails = cart.Items.Select(x =>
        //    {
        //        bool applicable = promo?.ApplicableProductId == null
        //                          || promo.ApplicableProductId == x.ProductVariant.ProductId;

        //        var price = x.ProductPrice;
        //        if (promo != null && applicable)
        //            price = Math.Round(price * multiplier, 2);

        //        return new OrderDetail
        //        {
        //            ProductVariant = x.ProductVariant,
        //            Price = price,
        //            Quantity = x.Quantity,
        //            ProductName = x.ProductVariant.Product?.Name ?? "",
        //        };
        //    }).ToList();

        //    var address = _addressRepo.Get().First(); // TODO: chọn đúng địa chỉ của user

        //    var total = orderDetails.Sum(d => d.Price * d.Quantity);

        //    var order = new Order
        //    {
        //        CustomerId = cart.CustomerId,
        //        ElderId = cart.ElderId,
        //        Note = dto.Note,
        //        OrderStatus = isPaid ? OrderStatus.Paid : OrderStatus.Fail,
        //        TotalPrice = total,
        //        OrderDetails = orderDetails,
        //        DistrictID = address.DistrictID,
        //        DistrictName = address.DistrictName,
        //        WardCode = address.WardCode,
        //        WardName = address.WardName,
        //        ProvinceID = address.ProvinceID,
        //        ProvinceName = address.ProvinceName
        //    };

        //    await _orderRepo.AddAsync(order);

        //    cart.Status = isPaid ? CartStatus.Approve : CartStatus.Pending;

        //    if (isPaid)
        //    {
        //        DeductStockFromCart(cart);

        //        await _paymentHistoryRepo.AddAsync(new PaymentHistory
        //        {
        //            Amount = total,
        //            OrderId = order.Id,
        //            UserId = cart.CustomerId,
        //            PaymentMenthod = "VNPay",
        //            paymentStatus = PaymentStatus.Success
        //        });

        //        var user = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == cart.CustomerId);
        //        if (user != null)
        //        {
        //            user.RewardPoint += (int)(total / 1000); // 1 điểm cho mỗi 1000đ
        //            _userRepo.Update(user);
        //        }

        //        if (userPromo != null)
        //        {
        //            userPromo.IsUsed = true;
        //            userPromo.UsedAt = DateTimeOffset.UtcNow;
        //            _userPromoRepo.Update(userPromo);
        //        }
        //    }

        //    // Nếu tất cả repository share cùng DbContext: 1 lần save là đủ
        //    await _orderRepo.SaveChangesAsync();
        //}


        private void DeductStockFromCart(Cart cart)
        {
            foreach (var item in cart.Items)
            {
                var variant = item.ProductVariant;

                if (variant.Stock < item.Quantity)
                {
                    var productName = variant.Product?.Name ?? "Unknown";
                    throw new Exception($"Not enough stock for product: {productName}");
                }

                variant.Stock -= item.Quantity;

                if (variant.Stock == 0)
                {
                    variant.IsDeleted = true;
                }
            }
        }


        public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string userId)
        {
            var customerId = GuidHelper.ParseOrThrow(userId, "CustomerId");

            var orders = await _orderRepo.Get()
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<PagedResult<OrderDto>> FilterOrdersAsync(OrderFilterDto request)
        {
            var query = _orderRepo.Get()
                .Include(o => o.OrderDetails)
                .Include(o => o.Elder)
                .AsQueryable();

            if (request.OrderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == request.OrderStatus.Value);
            }

            switch (request.SortBy?.ToLower())
            {
                case "totalprice":
                    query = request.IsDescending
                        ? query.OrderByDescending(o => o.TotalPrice)
                        : query.OrderBy(o => o.TotalPrice);
                    break;
                case "creationdate":
                    query = request.IsDescending
                        ? query.OrderByDescending(o => o.CreationDate)
                        : query.OrderBy(o => o.CreationDate);
                    break;
                default:
                    query = query.OrderByDescending(o => o.CreationDate);
                    break;
            }

            var totalItems = await query.CountAsync();
            var skip = (request.Page - 1) * request.PageSize;

            var orders = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync();

            var result = new PagedResult<OrderDto>
            {
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize,
                Items = _mapper.Map<List<OrderDto>>(orders)
            };

            return result;
        }

    }
}

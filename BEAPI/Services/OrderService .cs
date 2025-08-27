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
        private readonly IRepository<UserPromotion> _userPromotionRepo;
        private readonly IRepository<PaymentHistory> _paymentHistoryRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Cart> _cartRepo;
        private readonly IRepository<Address> _addressRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order> orderRepo,
                            IRepository<User> userRepo,
                            IRepository<Cart> cartRepo,
                            IRepository<Address> addressRepo,
                            IRepository<PaymentHistory> paymentHistoryRepo,
                            IRepository<UserPromotion> userPromotionRepo,
                            IRepository<Wallet> walletRepo,
                            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
            _paymentHistoryRepo = paymentHistoryRepo;
            _addressRepo = addressRepo;
            _userPromotionRepo = userPromotionRepo;
            _walletRepo = walletRepo;
        }

        public async Task CreateOrderAsync(OrderCreateDto dto, bool isPaid)
        {
            await PlaceOrderInternal(dto, payByWallet: false, isPaidParam: isPaid);
        }

        public async Task CreateOrderByWalletAsync(OrderCreateDto dto)
        {
            await PlaceOrderInternal(dto, payByWallet: true, isPaidParam: true);
        }

        private async Task PlaceOrderInternal(OrderCreateDto dto, bool payByWallet, bool isPaidParam)
        {
            var cart = await GetPendingCartAsync(dto.CartId);
            var userPromotion = await ValidateAndGetPromotionAsync(dto.UserPromotionId, cart.CustomerId);

            var orderDetails = BuildOrderDetails(cart);
            var address = await GetDefaultAddressAsync();

            var subTotal = orderDetails.Sum(d => d.Price * d.Quantity);
            var itemDiscountAmount = CalculateItemDiscount(orderDetails);
            var promoDiscountAmount = CalculatePromotionDiscount(userPromotion?.Promotion, subTotal - itemDiscountAmount);

            var total = subTotal - itemDiscountAmount - promoDiscountAmount;
            if (total < 0) total = 0;

            string paymentMethod;
            var isPaid = isPaidParam;

            if (payByWallet)
            {
                var deducted = await _walletRepo.Get()
                    .Where(w => w.UserId == cart.CustomerId && w.Amount >= total)
                    .ExecuteUpdateAsync(s => s.SetProperty(w => w.Amount, w => w.Amount - total));
                if (deducted == 0)
                {
                    throw new Exception("Insufficient wallet balance");
                }
                paymentMethod = "WALLET";
                isPaid = true;
            }
            else
            {
                paymentMethod = "VNPay";
            }

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                ElderId = cart.ElderId,
                Note = dto.Note,
                OrderStatus = isPaid ? OrderStatus.Paid : OrderStatus.Fail,
                TotalPrice = total,
                OrderDetails = orderDetails,
                DistrictID = address.DistrictID,
                DistrictName = address.DistrictName,
                WardCode = address.WardCode,
                WardName = address.WardName,
                PhoneNumber = address.PhoneNumber,
                StreetAddress = address.StreetAddress,
                ProvinceID = address.ProvinceID,
                ProvinceName = address.ProvinceName,
                Discount = userPromotion?.Promotion?.DiscountPercent ?? 0
            };

            await _orderRepo.AddAsync(order);
            cart.Status = isPaid ? CartStatus.Approve : CartStatus.Pending;

            if (isPaid)
            {
                DeductStockFromCart(cart);

                var payment = new PaymentHistory
                {
                    Amount = total,
                    OrderId = order.Id,
                    UserId = cart.CustomerId,
                    PaymentMenthod = paymentMethod,
                    PaymentStatus = PaymentStatus.Paid,
                };
                await _paymentHistoryRepo.AddAsync(payment);

                var user = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == cart.CustomerId);
                if (user != null)
                {
                    var pointEarned = (int)(total / 1000);
                    user.RewardPoint += pointEarned;
                }

                if (userPromotion != null)
                {
                    userPromotion.IsUsed = true;
                    _userPromotionRepo.Update(userPromotion);
                }

                var admin = await _userRepo.Get()
                    .Include(u => u.Role)
                    .FirstAsync(u => (u.Role != null && u.Role.Name == "Admin"));
                var adminWallet = await _walletRepo.Get()
                    .Where(w => w.UserId == admin.Id)
                    .FirstAsync();
                adminWallet.Amount = adminWallet.Amount + total;
            }

            await _orderRepo.SaveChangesAsync();
        }

        private async Task<Cart> GetPendingCartAsync(string cartId)
        {
            var id = GuidHelper.ParseOrThrow(cartId, nameof(cartId));
            return await _cartRepo.Get()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ProductVariant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(u => u.Id == id && u.Status == CartStatus.Pending)
                ?? throw new Exception("Cart not found or not in pending status");
        }

        private async Task<UserPromotion?> ValidateAndGetPromotionAsync(string? userPromotionId, Guid customerId)
        {
            if (string.IsNullOrWhiteSpace(userPromotionId) || !Guid.TryParse(userPromotionId, out var upId))
                return null;

            var userPromotion = await _userPromotionRepo.Get()
                .Include(up => up.Promotion)
                .FirstOrDefaultAsync(up => up.Id == upId)
                ?? throw new Exception("UserPromotion not found");

            if (userPromotion.UserId != customerId)
                throw new Exception("Promotion does not belong to this user");
            if (userPromotion.IsUsed)
                throw new Exception("Promotion was already used");

            var now = DateTimeOffset.UtcNow;
            var promo = userPromotion.Promotion;
            if (!promo.IsActive) throw new Exception("Promotion is inactive");
            if (promo.StartAt.HasValue && promo.StartAt.Value > now) throw new Exception("Promotion not started yet");
            if (promo.EndAt.HasValue && promo.EndAt.Value < now) throw new Exception("Promotion expired");

            return userPromotion;
        }

        private List<OrderDetail> BuildOrderDetails(Cart cart)
        {
            return cart.Items.Select(x => new OrderDetail
            {
                ProductVariant = x.ProductVariant,
                Price = x.ProductPrice,
                Quantity = x.Quantity,
                Discount = x.Discount > 0 ? x.Discount : x.ProductVariant.Discount,
                ProductName = x.ProductVariant.Product?.Name ?? string.Empty
            }).ToList();
        }

        private async Task<Address> GetDefaultAddressAsync()
        {
            return await _addressRepo.Get().FirstAsync();
        }

        private decimal CalculateItemDiscount(List<OrderDetail> orderDetails)
        {
            return orderDetails.Sum(d => (decimal)d.Discount / 100m * d.Price * d.Quantity);
        }

        private decimal CalculatePromotionDiscount(Promotion? promo, decimal baseAmount)
        {
            if (promo == null) return 0m;
            if (baseAmount <= 0) return 0m;
            return (decimal)promo.DiscountPercent / 100m * baseAmount;
        }

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

        public async Task<List<OrderDto>> GetOrdersByElderIdAsync(string userId)
        {
            var elderId = GuidHelper.ParseOrThrow(userId, "ElderId");

            var orders = await _orderRepo.Get()
                .Where(o => o.ElderId == elderId)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(string orderId)
        {
            var id = GuidHelper.ParseOrThrow(orderId, nameof(orderId));

            var order = await _orderRepo.Get()
                  .Include(o => o.OrderDetails)
                      .ThenInclude(od => od.ProductVariant)
                          .ThenInclude(pv => pv.Product)
                  .Include(o => o.OrderDetails)
                      .ThenInclude(od => od.ProductVariant)
                          .ThenInclude(pv => pv.ProductImages)
                  .Include(o => o.OrderDetails)
                      .ThenInclude(od => od.ProductVariant)
                          .ThenInclude(pv => pv.ProductVariantValues)
                              .ThenInclude(pvv => pvv.Value)
                  .Include(o => o.Elder)
                  .Include(o => o.Customer)
                  .FirstOrDefaultAsync(o => o.Id == id);


            if (order == null) return null;

            var orderDto = new OrderDto
            {
                Id = order.Id,
                Note = order.Note ?? string.Empty,
                TotalPrice = order.TotalPrice,
                Discount = order.Discount,
                OrderStatus = order.OrderStatus.ToString(),
                PhoneNumber = order.PhoneNumber ?? string.Empty,
                StreetAddress = order.StreetAddress ?? string.Empty,
                WardName = order.WardName ?? string.Empty,
                DistrictName = order.DistrictName ?? string.Empty,
                ProvinceName = order.ProvinceName ?? string.Empty,
                ShippingFee = order.ShippingFee,
                ShippingCode = order.ShippingCode,
                ExpectedDeliveryTime = order.ExpectedDeliveryTime,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                CreationDate = order.CreationDate,
                ElderName = order.Elder?.FullName ?? string.Empty,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    ProductName = od.ProductVariant?.Product?.Name ?? string.Empty,
                    Price = od.Price,
                    Quantity = od.Quantity,
                    Discount = od.Discount,
                    Style = string.Join(", ",
                        od.ProductVariant?.ProductVariantValues
                            .Select(pvv => pvv.Value?.Label)
                            .Where(s => !string.IsNullOrWhiteSpace(s)) ?? new List<string>()
                    ),
                    Images = od.ProductVariant?.ProductImages?
                        .Select(img => img.URL)
                        .ToList() ?? new List<string>()
                }).ToList()
            };

            return orderDto;
        }

        public async Task<PagedResult<OrderDto>> FilterOrdersAsync(OrderFilterDto request)
        {
            var query = _orderRepo.Get()
                .Include(o => o.OrderDetails)
                .Include(o => o.Elder)
                .Include(o => o.Customer)
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

        public async Task<OrderStatisticDto> UserStatistic(Guid UserId)
        {
            var orders = await _orderRepo.Get()
                .Where(x => x.CustomerId == UserId)
                .ToListAsync();

            var result = new OrderStatisticDto
            {
                TotalCount = orders.Count,
                TotalOrderToPending = orders.Count(x =>
                                    x.OrderStatus == OrderStatus.Paid
                                    || x.OrderStatus == OrderStatus.PendingChecked
                                    || x.OrderStatus == OrderStatus.PendingConfirm
                                    || x.OrderStatus == OrderStatus.PendingPickup
                                    || x.OrderStatus == OrderStatus.PendingDelivery
                                    || x.OrderStatus == OrderStatus.Shipping),
                TotalOrderComplete = orders.Count(x => x.OrderStatus == OrderStatus.Delivered)
            };
            return result;
        }

        public async Task<List<ElderBudgetStatisticDto>> ElderBudgetStatistic(Guid customerId, DateTime fromDate, DateTime toDate)
        {
            var query = await _orderRepo.Get()
                 .Where(o => o.CustomerId == customerId
                          && o.CreationDate >= fromDate
                          && o.CreationDate <= toDate)
                 .GroupBy(o => new { o.ElderId, o.Elder.FullName, o.Elder.Spendlimit })
                 .Select(g => new ElderBudgetStatisticDto
                 {
                     ElderId = g.Key.ElderId,
                     ElderName = g.Key.FullName,
                     TotalSpent = g.Sum(x => x.TotalPrice),
                     LimitSpent = g.Key.Spendlimit,
                     OrderCount = g.Count()
                 })
                 .ToListAsync();

            return query;
        }

        public async Task<CancelOrderResponseDto> CancelOrderAsync(CancelOrderDto dto)
        {
            if (!Guid.TryParse(dto.OrderId, out var orderId))
            {
                throw new Exception("Invalid order ID format");
            }

            var order = await _orderRepo.Get()
                .Include(o => o.OrderDetails)
                .Include(o => o.Customer)
                .Include(o => o.Elder)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Order not found");
            }

            var cancellableStatuses = new[] 
            { 
                OrderStatus.Created, 
                OrderStatus.Paid
            };

            if (!cancellableStatuses.Contains(order.OrderStatus))
            {
                throw new Exception($"Order cannot be cancelled in current status: {order.OrderStatus}. Only orders in 'Created' or 'Paid' status can be cancelled.");
            }

            var previousStatus = order.OrderStatus;
            var isRefunded = false;

            order.OrderStatus = OrderStatus.Canceled;
            order.Note = string.IsNullOrEmpty(order.Note) 
                ? $"Cancelled: {dto.CancelReason}" 
                : $"{order.Note} | Cancelled: {dto.CancelReason}";

            if (previousStatus == OrderStatus.Paid)
            {
                var wallet = await _walletRepo.Get()
                    .FirstOrDefaultAsync(w => w.UserId == order.CustomerId);

                if (wallet != null)
                {
                    wallet.Amount += order.TotalPrice;
                     _walletRepo.Update(wallet);
                    isRefunded = true;
                }
            }

            _orderRepo.Update(order);

            var payment = new PaymentHistory
            {
                UserId = order.CustomerId,
                Amount = order.TotalPrice,
                PaymentMenthod = "WALLET",
                PaymentStatus = PaymentStatus.Refund
            };

            await _paymentHistoryRepo.AddAsync(payment);
            await _orderRepo.SaveChangesAsync();
            return new CancelOrderResponseDto
            {
                OrderId = dto.OrderId,
                CancelReason = dto.CancelReason,
                PreviousStatus = previousStatus,
                CurrentStatus = OrderStatus.Canceled,
                RefundAmount = isRefunded ? order.TotalPrice : 0,
                IsRefunded = isRefunded,
                CancelledAt = DateTime.UtcNow
            };
        }
    }
}

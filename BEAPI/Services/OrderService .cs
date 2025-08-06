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
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Cart> _cartRepo;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order> orderRepo,
                            IRepository<ProductVariant> productVariantRepo,
                            IRepository<User> userRepo,
                            IRepository<Cart> cartRepo,
                            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productVariantRepo = productVariantRepo;
            _userRepo = userRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
        }

        public async Task CreateOrderAsync(OrderCreateDto dto, bool isPaid)
        {
            var cartId = GuidHelper.ParseOrThrow(dto.CartId, nameof(dto.CartId));

            var cart = await _cartRepo.Get()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ProductVariant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(u => u.Id == cartId && u.Status == CartStatus.Pending) ?? throw new Exception("Cart not found or not in pending status");
            var price = cart.Items.Sum(x => x.ProductPrice);

            var orderDetails = cart.Items.Select(x => new OrderDetail
            {
                ProductVariant = x.ProductVariant,
                Price = x.ProductPrice,
                Quantity = x.Quantity,
                ProductName = x.ProductVariant.Product.Name ?? "",
            }).ToList();

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                ElderId = cart.ElderId,
                Note = dto.Note,
                OrderStatus = isPaid ? OrderStatus.Paid : OrderStatus.Fail,
                TotalPrice = price,
                OrderDetails = orderDetails,
            };

            cart.Status = isPaid ? CartStatus.Approve : CartStatus.Pending;

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();
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

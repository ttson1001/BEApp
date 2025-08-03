using AutoMapper;
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

        public OrderService(IRepository<Order> orderRepo,
                            IRepository<ProductVariant> productVariantRepo,
                            IRepository<User> userRepo,
                            IRepository<Cart> cartRepo)
        {
            _orderRepo = orderRepo;
            _productVariantRepo = productVariantRepo;
            _userRepo = userRepo;
            _cartRepo = cartRepo;
        }

        public async Task CreateElderOrderAsync(OrderCreateDto dto)
        {
            var CartId = GuidHelper.ParseOrThrow(dto.CartId, nameof(dto.CartId));
            if (!await _cartRepo.Get().AnyAsync(u => u.Id == CartId))
                throw new Exception("Cart not found");
            var cart = _cartRepo.Get().Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                .ThenInclude(x => x.Product).First(u => u.Id == CartId && u.Status == CartStatus.Pending);

            var price = cart.Items.Sum(x => x.ProductPrice);
            
            var elder = await _userRepo.Get().Include(x => x.Guardian).FirstAsync(x => x.Id == cart.CustomerId);
           
            var order = new Order
            {
                Customer = elder.Guardian,
                ElderId = elder.Id,
                Note = dto.Note,
                OrderStatus = OrderStatus.Created,
                TotalPrice = price,
            };
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            orderDetails.AddRange(cart.Items.Select(x => new OrderDetail
            {
                Order = order,
                ProductVariant = x.ProductVariant,
                Price = x.ProductPrice,
                Quantity = x.Quantity,
                ProductName = x.ProductVariant.Product.Name ?? "",
            }));
            cart.Status = CartStatus.Approve;
            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();
        }
    }
}

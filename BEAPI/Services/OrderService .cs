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

        public async Task CreateOrderAsync(OrderCreateDto dto, bool isPaid)
        {
            var cartId = GuidHelper.ParseOrThrow(dto.CartId, nameof(dto.CartId));

            var cart = await _cartRepo.Get()
                .Include(x => x.Items)
                    .ThenInclude(x => x.ProductVariant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(u => u.Id == cartId && u.Status == CartStatus.Pending) ?? throw new Exception("Cart not found or not in pending status");
            var price = cart.Items.Sum(x => x.ProductPrice);

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                ElderId = cart.ElderId,
                Note = dto.Note,
                OrderStatus = isPaid ? OrderStatus.Paid : OrderStatus.Fail,
                TotalPrice = price,
            };

            var orderDetails = cart.Items.Select(x => new OrderDetail
            {
                Order = order,
                ProductVariant = x.ProductVariant,
                Price = x.ProductPrice,
                Quantity = x.Quantity,
                ProductName = x.ProductVariant.Product.Name ?? "",
            }).ToList();

            cart.Status = CartStatus.Approve;

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();
        }
    }
}

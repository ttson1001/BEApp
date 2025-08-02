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
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order> orderRepo,
                            IRepository<Product> productRepo,
                            IRepository<User> userRepo,
                            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task CreateAsync(OrderCreateDto dto)
        {
            var customerId = GuidHelper.ParseOrThrow(dto.CustomerId, nameof(dto.CustomerId));

            if (!await _userRepo.Get().AnyAsync(u => u.Id == customerId))
                throw new Exception("Customer not found");

            if (dto.OrderDetails == null || !dto.OrderDetails.Any())
                throw new Exception("Order must have at least one OrderDetail");

            var productIds = dto.OrderDetails
                .Select(d => GuidHelper.ParseOrThrow(d.ProductId, nameof(d.ProductId)))
                .ToList();

            var elderIds = dto.OrderDetails
                .Select(d => GuidHelper.ParseOrThrow(d.ElderId, nameof(d.ElderId)))
                .ToList();

            var invalidProducts = await ValidationHelper.ValidateIdsExistAsync(_productRepo, productIds);
            if (invalidProducts.Any())
                throw new Exception($"Invalid ProductIds: {string.Join(", ", invalidProducts)}");

            var invalidElders = await ValidationHelper.ValidateIdsExistAsync(_userRepo, elderIds);
            if (invalidElders.Any())
                throw new Exception($"Invalid ElderIds: {string.Join(", ", invalidElders)}");

            var order = _mapper.Map<Order>(dto);
            order.OrderStatus = OrderStatus.Paid;

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();
        }

    }
}

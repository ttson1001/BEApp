using AutoMapper;
using BEAPI.Database;
using BEAPI.Dtos.Cart;
using BEAPI.Entities;
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
        private readonly  IRepository<Product> _productRepo;
        private readonly IMapper _mapper;

        public CartService(IRepository<Cart> cartRepo, IMapper mapper, IRepository<CartItem> cartItemRepo, IRepository<User> userRepo, IRepository<Product> productRepo)
        {
            _cartRepo = cartRepo;
            _mapper = mapper;
            _cartItemRepo = cartItemRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
        }

        public async Task ReplaceCartAsync(CartUpdateDto dto)
        {
            var customerId = GuidHelper.ParseOrThrow(dto.CustomerId, nameof(dto.CustomerId));

            if (!await _userRepo.Get().AnyAsync(u => u.Id == customerId))
                throw new Exception("Customer not found");

            var productIds = dto.Items.Select(i => GuidHelper.ParseOrThrow(i.ProductId)).ToList();
            var elderIds = dto.Items.Select(i => GuidHelper.ParseOrThrow(i.ElderId)).ToList();

            var invalidProducts = await ValidationHelper.ValidateIdsExistAsync(_productRepo, productIds);
            if (invalidProducts.Any())
                throw new Exception($"Invalid Products: {string.Join(", ", invalidProducts)}");

            var invalidElders = await ValidationHelper.ValidateIdsExistAsync(_userRepo, elderIds);
            if (invalidElders.Any())
                throw new Exception($"Invalid Elders: {string.Join(", ", invalidElders)}");

            var productPrices = await _productRepo.Get()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, Price = p.ProductVariants.FirstOrDefault()!.Price })
                .ToListAsync();

            var cart = await _cartRepo.Get()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId };
                await _cartRepo.AddAsync(cart);
            }
            else
            {
                cart.Items.Clear();
            }

            var newItems = _mapper.Map<List<CartItem>>(dto.Items);
            var priceDict = productPrices.ToDictionary(p => p.Id, p => p.Price);

            foreach (var item in newItems)
            {
                item.ProductPrice = priceDict[item.ProductId];
                cart.Items.Add(item);
            }

            await _cartRepo.SaveChangesAsync();
        }
    }
}

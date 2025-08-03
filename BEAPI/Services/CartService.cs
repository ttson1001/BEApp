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

        public async Task ReplaceCartAsync(CartReplaceAllDto dto)
        {
            var customerId = GuidHelper.ParseOrThrow(dto.CustomerId, nameof(dto.CustomerId));

            if (!await _userRepo.Get().AnyAsync(u => u.Id == customerId))
                throw new Exception("Customer not found");

            var productVariantIds = dto.Items.Select(i => GuidHelper.ParseOrThrow(i.ProductVariantId)).ToList();
            var elderIds = dto.Items.Select(i => GuidHelper.ParseOrThrow(i.ElderId)).ToList();

            var invalidProductVariants = await ValidationHelper.ValidateIdsExistAsync(_productVariantRepo, productVariantIds);
            if (invalidProductVariants.Any())
                throw new Exception($"Invalid Products: {string.Join(", ", invalidProductVariants)}");

            var invalidElders = await ValidationHelper.ValidateIdsExistAsync(_userRepo, elderIds);
            if (invalidElders.Any())
                throw new Exception($"Invalid Elders: {string.Join(", ", invalidElders)}");

            var priceDict = await _productVariantRepo.Get()
                    .Where(p => productVariantIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Price })
                    .ToDictionaryAsync(p => p.Id, p => p.Price);

            var cart = await _cartRepo.Get()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.Status == CartStatus.Created);

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId, Status = CartStatus.Created };
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

            await _cartRepo.SaveChangesAsync();
        }
    }
}

using AutoMapper;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Promotion;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IRepository<Promotion> _promoRepo;
        private readonly IRepository<UserPromotion> _userPromoRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public PromotionService(
            IRepository<Promotion> promoRepo,
            IRepository<UserPromotion> userPromoRepo,
            IRepository<User> userRepo,
            IMapper mapper)
        {
            _promoRepo = promoRepo;
            _userPromoRepo = userPromoRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<List<PromotionDto>> GetAllAsync()
        {
            var list = await _promoRepo.Get()
                .OrderByDescending(x => x.CreationDate)
                .ToListAsync();

            return _mapper.Map<List<PromotionDto>>(list);
        }

        public async Task<PromotionDto> GetByIdAsync(Guid id)
        {
            var entity = await _promoRepo.Get().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception("Promotion not found");

            return _mapper.Map<PromotionDto>(entity);
        }

        public async Task<PagedResult<PromotionDto>> SearchAsync(PromotionSearchDto dto)
        {
            var q = _promoRepo.Get().AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                var kw = dto.Keyword.Trim();
                q = q.Where(p => p.Title.Contains(kw) || p.Description.Contains(kw));
            }

            if (dto.IsActive.HasValue)
                q = q.Where(p => p.IsActive == dto.IsActive.Value);

            q = dto.SortBy.ToLower() switch
            {
                "title" => dto.SortAscending ? q.OrderBy(p => p.Title) : q.OrderByDescending(p => p.Title),
                "requiredpoints" => dto.SortAscending ? q.OrderBy(p => p.RequiredPoints) : q.OrderByDescending(p => p.RequiredPoints),
                "discountpercent" => dto.SortAscending ? q.OrderBy(p => p.DiscountPercent) : q.OrderByDescending(p => p.DiscountPercent),
                _ => dto.SortAscending ? q.OrderBy(p => p.CreationDate) : q.OrderByDescending(p => p.CreationDate),
            };

            var total = await q.CountAsync();
            var items = await q.Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();

            return new PagedResult<PromotionDto>
            {
                TotalItems = total,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Items = _mapper.Map<List<PromotionDto>>(items)
            };
        }

        public async Task CreateAsync(PromotionCreateDto dto)
        {
            if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
                throw new Exception("DiscountPercent must be between 0 and 100");

            var entity = _mapper.Map<Promotion>(dto);
            entity.IsActive = true;
            await _promoRepo.AddAsync(entity);
            await _promoRepo.SaveChangesAsync();
        }

        public async Task UpdateAsync(PromotionUpdateDto dto)
        {
            if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
                throw new Exception("DiscountPercent must be between 0 and 100");

            var entity = await _promoRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new Exception("Promotion not found");

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.DiscountPercent = dto.DiscountPercent;
            entity.RequiredPoints = dto.RequiredPoints;
            entity.StartAt = dto.StartAt;
            entity.EndAt = dto.EndAt;
            entity.IsActive = dto.IsActive;

            _promoRepo.Update(entity);
            await _promoRepo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _promoRepo.Get()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception("Promotion not found");

            entity.IsActive = false; 
            _promoRepo.Update(entity);

            await _promoRepo.SaveChangesAsync();
        }

        public async Task RedeemAsync(RedeemPromotionDto dto)
        {
            var user = await _userRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.UserId)
                ?? throw new Exception("User not found");

            var promo = await _promoRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.PromotionId && x.IsActive)
                ?? throw new Exception("Promotion not found or inactive");

            var now = DateTimeOffset.UtcNow;
            if ((promo.StartAt.HasValue && now < promo.StartAt.Value) ||
                (promo.EndAt.HasValue && now > promo.EndAt.Value))
                throw new Exception("Promotion not available");

            if (user.RewardPoint < promo.RequiredPoints)
                throw new Exception("Not enough points");

            user.RewardPoint -= promo.RequiredPoints;

            await _userPromoRepo.AddAsync(new UserPromotion
            {
                UserId = user.Id,
                PromotionId = promo.Id
            });

            await _userRepo.SaveChangesAsync();
        }

        public async Task<List<PromotionDto>> GetMyPromotionsAsync(Guid userId, bool onlyAvailable)
        {
            var now = DateTimeOffset.UtcNow;

            var query = _userPromoRepo.Get()
                 .Where(up => up.UserId == userId && !up.IsUsed)
                 .Join(
                     _promoRepo.Get().Where(p => p.IsActive),
                     up => up.PromotionId,
                     p => p.Id,
                     (up, p) => p
                 );
            if (onlyAvailable)
            {
                query = query.Where(p =>
                    (!p.StartAt.HasValue || p.StartAt <= now) &&
                    (!p.EndAt.HasValue || p.EndAt >= now));
            }

            var list = await query.OrderByDescending(p => p.CreationDate).ToListAsync();
            return _mapper.Map<List<PromotionDto>>(list);
        }
    }
}

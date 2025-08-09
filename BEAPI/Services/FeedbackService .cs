using AutoMapper;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Feedback;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IRepository<Feedback> _feedbackRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public FeedbackService(
            IRepository<Feedback> feedbackRepo,
            IRepository<User> userRepo,
            IMapper mapper)
        {
            _feedbackRepo = feedbackRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<FeedbackDto> GetByIdAsync(Guid id)
        {
            var entity = await _feedbackRepo.Get().FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception("Feedback not found");
            return _mapper.Map<FeedbackDto>(entity);
        }

        public async Task<List<FeedbackDto>> GetAllAsync()
        {
            var list = await _feedbackRepo.Get().OrderByDescending(x => x.CreationDate).ToListAsync();
            return _mapper.Map<List<FeedbackDto>>(list);
        }

        public async Task<List<FeedbackDto>> GetByUserIdAsync(Guid userId)
        {
            var list = await _feedbackRepo.Get().Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreationDate).ToListAsync();
            return _mapper.Map<List<FeedbackDto>>(list);
        }

        public async Task<List<FeedbackDto>> GetByAdminIdAsync(Guid adminId)
        {
            var list = await _feedbackRepo.Get().Where(x => x.AdminId == adminId)
                .OrderByDescending(x => x.CreationDate).ToListAsync();
            return _mapper.Map<List<FeedbackDto>>(list);
        }

        public async Task<PagedResult<FeedbackDto>> SearchAsync(FeedbackSearchDto dto)
        {
            var query = _feedbackRepo.Get().AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                var kw = dto.Keyword.Trim();
                query = query.Where(x => x.Title.Contains(kw) || x.Description.Contains(kw));
            }
            if (dto.Status.HasValue)
                query = query.Where(x => x.Status == dto.Status.Value);
            if (dto.UserId.HasValue)
                query = query.Where(x => x.UserId == dto.UserId.Value);

            query = dto.SortBy.ToLower() switch
            {
                "title" => dto.SortAscending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
                "status" => dto.SortAscending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                "userid" => dto.SortAscending ? query.OrderBy(x => x.UserId) : query.OrderByDescending(x => x.UserId),
                _ => dto.SortAscending ? query.OrderBy(x => x.CreationDate) : query.OrderByDescending(x => x.CreationDate),
            };

            var total = await query.CountAsync();
            var items = await query.Skip((dto.Page - 1) * dto.PageSize).Take(dto.PageSize).ToListAsync();

            return new PagedResult<FeedbackDto>
            {
                TotalItems = total,
                Page = dto.Page,
                PageSize = dto.PageSize,
                Items = _mapper.Map<List<FeedbackDto>>(items)
            };
        }

        public async Task CreateAsync(FeedbackCreateDto dto)
        {
            var entity = _mapper.Map<Feedback>(dto);
            entity.Status = FeedbackStatus.Pending;


            if (entity.AdminId == null)
            {
                var adminId = await _userRepo.Get()
                    .Where(u => u.Role != null && u.Role.Name == "Admin")
                    .Select(u => new
                    {
                        u.Id,
                        Pending = _feedbackRepo.Get().Count(f => f.AdminId == u.Id && f.Status == FeedbackStatus.Pending)
                    })
                    .OrderBy(x => x.Pending)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (adminId != Guid.Empty)
                    entity.AdminId = adminId;
            }

            await _feedbackRepo.AddAsync(entity);
            await _feedbackRepo.SaveChangesAsync();
        }

        public async Task UpdateAsync(FeedbackUpdateDto dto)
        {
            var entity = await _feedbackRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new Exception("Feedback not found");

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.ImagePath = dto.ImagePath;

            _feedbackRepo.Update(entity);
            await _feedbackRepo.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(FeedbackUpdateStatusDto dto)
        {
            var entity = await _feedbackRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.FeedbackId)
                ?? throw new Exception("Feedback not found");

            entity.Status = dto.Status;
            _feedbackRepo.Update(entity);
            await _feedbackRepo.SaveChangesAsync();
        }

        public async Task RespondAsync(FeedbackRespondDto dto)
        {
            var entity = await _feedbackRepo.Get().FirstOrDefaultAsync(x => x.Id == dto.FeedbackId)
                ?? throw new Exception("Feedback not found");

            entity.ResponseMessage = dto.ResponseMessage;
            entity.ResponseAttachment = dto.ResponseAttachment;
            entity.RespondedAt = DateTimeOffset.UtcNow;
            entity.Status = FeedbackStatus.Resolved;

            _feedbackRepo.Update(entity);
            await _feedbackRepo.SaveChangesAsync();
        }
    }

}

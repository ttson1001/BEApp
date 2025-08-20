using AutoMapper;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Report;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Report> _repo;
        private readonly IMapper _mapper;

        public ReportService(IRepository<Report> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ReportDto>> GetAllAsync()
        {
            var data = await _repo.Get().ToListAsync();
            return _mapper.Map<List<ReportDto>>(data);
        }

        public async Task<ReportDto> GetByIdAsync(Guid id)
        {
            var report = await _repo.Get().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new Exception("Report not found");
            return _mapper.Map<ReportDto>(report);
        }

        public async Task<List<ReportDto>> GetByUserIdAsync(Guid userId)
        {
            var data = await _repo.Get().Where(r => r.UserId == userId).ToListAsync();
            return _mapper.Map<List<ReportDto>>(data);
        }

        public async Task<List<ReportDto>> GetByConsultantIdAsync(Guid consultantId)
        {
            var data = await _repo.Get().Where(r => r.ConsultantId == consultantId).ToListAsync();
            return _mapper.Map<List<ReportDto>>(data);
        }

        public async Task<List<ReportDto>> GetByUserAndConsultantAsync(Guid userId, Guid consultantId)
        {
            var data = await _repo.Get()
                .Where(r => r.UserId == userId && r.ConsultantId == consultantId)
                .ToListAsync();
            return _mapper.Map<List<ReportDto>>(data);
        }

        public async Task<PagedResult<ReportDto>> SearchAsync(ReportSearchDto dto)
        {
            var query = _repo.Get().Include(x => x.Consultant).AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                var keyword = dto.Keyword.Trim().ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(keyword) ||
                    r.Description.ToLower().Contains(keyword));
            }

            if (dto.ConsultantId.HasValue)
            {
                query = query.Where(r => r.ConsultantId == dto.ConsultantId);
            }

            query = dto.SortBy.ToLower() switch
            {
                "title" => dto.SortAscending ? query.OrderBy(r => r.Title) : query.OrderByDescending(r => r.Title),
                "description" => dto.SortAscending ? query.OrderBy(r => r.Description) : query.OrderByDescending(r => r.Description),
                "userid" => dto.SortAscending ? query.OrderBy(r => r.UserId) : query.OrderByDescending(r => r.UserId),
                "consultantid" => dto.SortAscending ? query.OrderBy(r => r.ConsultantId) : query.OrderByDescending(r => r.ConsultantId),
                _ => dto.SortAscending ? query.OrderBy(r => r.CreationDate) : query.OrderByDescending(r => r.CreationDate),
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagedResult<ReportDto>
            {
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = total,
                Items = _mapper.Map<List<ReportDto>>(items)
            };
        }

        public async Task CreateAsync(ReportCreateDto dto)
        {
            var report = _mapper.Map<Report>(dto);
            await _repo.AddAsync(report);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReportUpdateDto dto)
        {
            var report = await _repo.Get().FirstOrDefaultAsync(r => r.Id == dto.Id)
                ?? throw new Exception("Report not found");

            _mapper.Map(dto, report);
            _repo.Update(report);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var report = await _repo.Get().FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new Exception("Report not found");

            _repo.Delete(report);
            await _repo.SaveChangesAsync();
        }
    }

}

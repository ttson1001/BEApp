using BEAPI.Dtos.Common;
using BEAPI.Dtos.Report;

namespace BEAPI.Services.IServices
{
    public interface IReportService
    {
        Task<List<ReportDto>> GetAllAsync();
        Task<ReportDto> GetByIdAsync(Guid id);
        Task<List<ReportDto>> GetByUserIdAsync(Guid userId);
        Task<List<ReportDto>> GetByConsultantIdAsync(Guid consultantId);
        Task<List<ReportDto>> GetByUserAndConsultantAsync(Guid userId, Guid consultantId);
        Task<PagedResult<ReportDto>> SearchAsync(ReportSearchDto dto);
        Task CreateAsync(ReportCreateDto dto);
        Task UpdateAsync(ReportUpdateDto dto);
    }
}

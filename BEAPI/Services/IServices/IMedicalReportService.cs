using BEAPI.Dtos.Category;

namespace BEAPI.Services.IServices
{
    public interface IMedicalReportService
    {
        Task CreateListMedicalReport(List<CreateCategoryValueDto> categoryValueDtos);
        Task<List<CategoryValueDto>> GetListValueMedicalReport();
    }
}

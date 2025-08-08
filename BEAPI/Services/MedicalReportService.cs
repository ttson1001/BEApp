using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IRepository<Value> _valueRepo;
        private readonly IMapper _mapper;
        public MedicalReportService(IRepository<ListOfValue> repository, IMapper mapper, IRepository<Value> valueRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _valueRepo = valueRepo;
        }
        public async Task CreateListMedicalReport(List<CreateCategoryValueDto> categoryValueDtos)
        {
            var root = await _repository.Get()
              .Include(x => x.Values)
              .FirstOrDefaultAsync(x => x.Note == "MEDICAL_REPORT_TYPE" && x.Type == Entities.Enum.MyValueType.MedicalReport)
              ?? throw new Exception("ListOfValue not found");

            var values = categoryValueDtos.Select(val => new Value
            {
                Code = val.Code,
                Label = val.Label,
                Description = val.Description,
                Type = Entities.Enum.MyValueType.MedicalReport,
                ListOfValueId = root.Id
            }).ToList();

            await _valueRepo.AddRangeAsync(values);
            await _valueRepo.SaveChangesAsync();
        }

        public async Task<List<CategoryValueDto>> GetListValueMedicalReport()
        {
            var category = await _repository.Get().Include(x => x.Values).Where(x => x.Note == "MEDICAL_REPORT_TYPE").FirstOrDefaultAsync() ?? throw new Exception("MedicalReport not found");
            return _mapper.Map<List<CategoryValueDto>>(category.Values);
        }
    }
}

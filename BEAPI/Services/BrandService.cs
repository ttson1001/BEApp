using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Entities;
using BEAPI.Entities.Enum;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class BrandService : IBrandService
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IRepository<Value> _valueRepo;
        private readonly IMapper _mapper;
        public BrandService(IRepository<ListOfValue> repository, IMapper mapper, IRepository<Value> valueRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _valueRepo = valueRepo;
        }
        public async Task CreateListBrand(List<CreateCategoryValueDto> categoryValueDtos)
        {
            var root = await _repository.Get()
              .Include(x => x.Values)
              .FirstOrDefaultAsync(x => x.Note == "BRAND" && x.Type == Entities.Enum.MyValueType.Brand)
              ?? throw new Exception("ListOfValue not found");

            var values = categoryValueDtos.Select(val => new Value
            {
                Code = val.Code,
                Label = val.Label,
                Description = val.Description,
                Type = Entities.Enum.MyValueType.Brand,
                ListOfValueId = root.Id
            }).ToList();

            await _valueRepo.AddRangeAsync(values);
            await _valueRepo.SaveChangesAsync();
        }
        public async Task<List<CategoryValueDto>> GetListValueBrand()
        {
            var category = await _repository.Get().Include(x => x.Values).Where(x => x.Note == "BRAND").FirstOrDefaultAsync() ?? throw new Exception("Brand not found");
            return _mapper.Map<List<CategoryValueDto>>(category.Values);
        }

        public async Task DeactivateOrActiveBrandAsync(Guid valueId)
        {
            var value = await _valueRepo.Get()
                .FirstOrDefaultAsync(v => v.Id == valueId && v.Type == MyValueType.Brand);

            if (value == null)
            {
                throw new Exception("Brand not found");
            }

            value.IsDeleted = !value.IsDeleted;

            _valueRepo.Update(value);
            await _valueRepo.SaveChangesAsync();
        }

        public async Task EditBrandAsync(UpdateCategoryValueDto dto)
        {
            var value = await _valueRepo.Get()
                .FirstOrDefaultAsync(v => v.Id == dto.Id && v.Type == MyValueType.Brand);

            if (value == null)
            {
                throw new Exception("Brand not found");
            }

            value.Code = dto.Code;
            value.Label = dto.Label;
            value.Description = dto.Description;

            _valueRepo.Update(value);
            await _valueRepo.SaveChangesAsync();
        }
    }
}

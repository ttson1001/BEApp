using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class RelationShipService: IRelationShipService
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IRepository<Value> _valueRepo;
        private readonly IMapper _mapper;
        public RelationShipService(IRepository<ListOfValue> repository, IMapper mapper, IRepository<Value> valueRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _valueRepo = valueRepo;
        }
        public async Task CreateListRelationship(List<CreateCategoryValueDto> categoryValueDtos)
        {
            var root = await _repository.Get()
              .Include(x => x.Values)
              .FirstOrDefaultAsync(x => x.Note == "RELATIONSHIP" && x.Type == Entities.Enum.MyValueType.Relationship)
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
        public async Task<List<CategoryValueDto>> GetListValueRelationship()
        {
            var category = await _repository.Get().Include(x => x.Values).Where(x => x.Note == "RELATIONSHIP").FirstOrDefaultAsync() ?? throw new Exception("Brand not found");
            return _mapper.Map<List<CategoryValueDto>>(category.Values);
        }
    }
}

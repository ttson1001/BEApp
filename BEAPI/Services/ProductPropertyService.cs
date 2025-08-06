using AutoMapper;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ProductPropertyService : IProductPropertySerivce
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IRepository<Value> _valueRepo;
        private readonly IMapper _mapper;
        public ProductPropertyService(IRepository<ListOfValue> repository, IMapper mapper, IRepository<Value> valueRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _valueRepo = valueRepo;
        }

        public async Task CreateListOfValueWithValuesAsync(ListOfValueWithValuesCreateDto dto)
        {
            var listEntity = new ListOfValue
            {
                Label = dto.Label,
                Note = dto.Note,
                Type = Entities.Enum.MyValueType.ProductProperty
            };

            var values = dto.Values.Select(val => new Value
            {
                Code = val.Code,
                Label = val.Label,
                Description = val.Description,
                Type = Entities.Enum.MyValueType.ProductProperty,
                ListOfValue = listEntity
            }).ToList();

            listEntity.Values = values;

            await _repository.AddAsync(listEntity);
            await _repository.SaveChangesAsync();
        }

        public async Task<List<ListOfValueDto>> GetListProductProperty()
        {
            var list = await _repository.Get().Include(x => x.Values).Where(x => x.Type == Entities.Enum.MyValueType.ProductProperty).ToListAsync();

            return _mapper.Map<List<ListOfValueDto>>(list);
        }

        public async Task<List<ValueDto>> GetAllValueProductProperty()
        {
            var list = await _valueRepo.Get().Where(x => x.Type == Entities.Enum.MyValueType.ProductProperty).ToListAsync();

            return _mapper.Map<List<ValueDto>>(list);
        }
    }
}

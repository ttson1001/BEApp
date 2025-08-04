using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Value;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IRepository<Value> _valueRepo;
        private readonly IMapper _mapper;
        public CategoryService(IRepository<ListOfValue> repository, IMapper mapper, IRepository<Value> valueRepo)
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
                Type = Entities.Enum.MyValueType.Category
            };

            var values = dto.Values.Select(val => new Value
            {
                Code = val.Code,
                Label = val.Label,
                Description = val.Description,
                Type = Entities.Enum.MyValueType.Category,
                ListOfValue = listEntity
            }).ToList();

            listEntity.Values = values;

            await _repository.AddAsync(listEntity);
            await _repository.SaveChangesAsync();
        }

        public async Task<ListOfValueTreeDto> GetListOfValueTreeAsync()
        {
            var root = await _repository.Get()
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.Note == "CATEGORY" && x.Type == Entities.Enum.MyValueType.Category)
                ?? throw new Exception("ListOfValue not found");

            return await GetListOfValueTreeOptimizedAsync(root.Id);
        }

        public async Task<ListOfValueTreeDto> GetListOfValueTreeOptimizedAsync(Guid rootId)
        {
            var allLists = await _repository.Get()
                .Include(x => x.Values)
                .ToListAsync();

            var listDict = allLists.ToDictionary(x => x.Id);
            var valueDict = allLists.SelectMany(x => x.Values).ToDictionary(v => v.Id);

            ListOfValueTreeDto BuildListTree(Guid listId)
            {
                if (!listDict.TryGetValue(listId, out var list)) return null;

                var dto = new ListOfValueTreeDto
                {
                    ListId = list.Id.ToString(),
                    Label = list.Label,
                    Note = list.Note,
                    Type = list.Type,
                    Values = new List<ValueTreeNodeDto>()
                };

                foreach (var val in list.Values)
                {
                    var node = new ValueTreeNodeDto
                    {
                        ValueId = val.Id.ToString(),
                        Code = val.Code,
                        Label = val.Label,
                        Description = val.Description,
                        Type = val.Type,
                        Children = new List<ListOfValueTreeDto>()
                    };

                    if (val.ChildListOfValueId.HasValue)
                    {
                        var childTree = BuildListTree(val.ChildListOfValueId.Value);
                        if (childTree != null)
                            node.Children.Add(childTree);
                    }
                    dto.Values.Add(node);
                }
                return dto;
            }
            return BuildListTree(rootId);
        }
    }
}

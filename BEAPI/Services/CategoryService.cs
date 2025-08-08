using AutoMapper;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Dtos.Value;
using BEAPI.Entities;
using BEAPI.Helper;
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

        public async Task CreateListCategory(List<CreateCategoryValueDto> categoryValueDtos)
        {
            var root = await _repository.Get()
              .Include(x => x.Values)
              .FirstOrDefaultAsync(x => x.Note == "CATEGORY" && x.Type == Entities.Enum.MyValueType.Category)
              ?? throw new Exception("ListOfValue not found");

            var values = categoryValueDtos.Select(val => new Value
            {
                Code = val.Code,
                Label = val.Label,
                Description = val.Description,
                Type = Entities.Enum.MyValueType.Category,
                ListOfValueId = root.Id
            }).ToList();

            await _valueRepo.AddRangeAsync(values);
            await _valueRepo.SaveChangesAsync();
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

        public async Task<List<ListOfValueDto>> GetListCategory()
        {
            var listCaterory = await _repository.Get().Include(x => x.Values).ThenInclude(x => x.ChildListOfValue).Where(x => x.Type == Entities.Enum.MyValueType.Category).ToListAsync();

            return _mapper.Map<List<ListOfValueDto>>(listCaterory); 
        }

        public async Task<List<CategoryValueDto>> GetListValueCategoryById(string categoryId)
        {
            var guuiCategoryId = GuidHelper.ParseOrThrow(categoryId, nameof(categoryId));
            var category = await _repository.Get().Include(x => x.Values).ThenInclude(x => x.ChildListOfValue).Where(x => x.Id == guuiCategoryId).FirstOrDefaultAsync() ?? throw new Exception("Category not found");
            return _mapper.Map<List<CategoryValueDto>>(category.Values);
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

        public async Task LinkSubCategory(LinkCategoryDto linkCategoryDto)
        {
            var valueId = GuidHelper.ParseOrThrow(linkCategoryDto.CategoryId, nameof(linkCategoryDto.CategoryId));
            var value = await _valueRepo.Get().FirstOrDefaultAsync(x => x.Id == valueId)
                         ?? throw new Exception("Category not found");

            if (string.IsNullOrWhiteSpace(linkCategoryDto.SubCategoryId))
            {
                value.ChildListOfValueId = null;
                await _valueRepo.SaveChangesAsync();
                return;
            }

            var sublistCategoryId = GuidHelper.ParseOrThrow(linkCategoryDto.SubCategoryId, nameof(linkCategoryDto.SubCategoryId));
            var root = await _repository.Get().FirstOrDefaultAsync(x => x.Id == sublistCategoryId)
                         ?? throw new Exception("Sublist category not found");

            if (root.Note == "CATEGORY")
                throw new Exception("Don't link root category");

            if (await HasCircularReference(value.Id, sublistCategoryId))
                throw new Exception("Circular reference detected. Cannot link category.");

            value.ChildListOfValueId = sublistCategoryId;
            await _valueRepo.SaveChangesAsync();
        }

        public List<CategoryValueLeafWithPathDto> GetLeafNodesWithPaths(ListOfValueTreeDto tree)
        {
            var result = new List<CategoryValueLeafWithPathDto>();

            void Traverse(List<ValueTreeNodeDto> nodes, List<string> path)
            {
                foreach (var node in nodes)
                {
                    var currentPath = new List<string>(path) { node.ValueId };

                    if (node.Children != null && node.Children.Any())
                    {
                        foreach (var child in node.Children)
                        {
                            Traverse(child.Values, currentPath);
                        }
                    }
                    else
                    {
                        result.Add(new CategoryValueLeafWithPathDto
                        {
                            ValueId = node.ValueId,
                            Code = node.Code,
                            Label = node.Label,
                            Description = node.Description,
                            Type = node.Type,
                            Path = currentPath
                        });
                    }
                }
            }

            Traverse(tree.Values, new List<string>());
            return result;
        }

        public async Task<List<ListOfValueDto>> GetListCategoryNoValue()
        {
            var listCaterory = await _repository.Get().Where(x => x.Type == Entities.Enum.MyValueType.Category).ToListAsync();

            return _mapper.Map<List<ListOfValueDto>>(listCaterory);
        }

        private async Task<bool> HasCircularReference(Guid parentId, Guid childId)
        {
            var current = await _valueRepo.Get().FirstOrDefaultAsync(x => x.Id == childId);
            while (current != null)
            {
                if (current.ChildListOfValueId == null)
                    return false;

                if (current.ChildListOfValueId == parentId)
                    return true;

                current = await _valueRepo.Get().FirstOrDefaultAsync(x => x.Id == current.ChildListOfValueId);
            }

            return false;
        }
    }
}

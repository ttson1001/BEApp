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
    public class CategoryService : ICategoryService
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

            var values = new List<Value>();

            foreach (var val in categoryValueDtos)
            {
                var childLov = new ListOfValue
                {
                    Id = Guid.NewGuid(),
                    Label = val.Label,
                    Note = val.Code,
                    Type = Entities.Enum.MyValueType.Category,
                    Values = new List<Value>()
                };

                var value = new Value
                {
                    Code = val.Code,
                    Label = val.Label,
                    Description = val.Description,
                    Type = Entities.Enum.MyValueType.Category,
                    ListOfValueId = root.Id,
                    ChildListOfValueId = childLov.Id,
                    ChildListOfValue = childLov
                };

                values.Add(value);
            }

            await _valueRepo.AddRangeAsync(values);
            await _valueRepo.SaveChangesAsync();
        }

        public async Task UpdateCategoryValue(UpdateCategoryValueDto dto)
        {
            var value = await _valueRepo.Get()
                .Include(v => v.ChildListOfValue)
                .FirstOrDefaultAsync(v => v.Id == dto.Id && v.Type == Entities.Enum.MyValueType.Category);

            if (value == null)
                throw new Exception("Category Value not found");

            value.Code = dto.Code;
            value.Label = dto.Label;
            value.Description = dto.Description;

            if (value.ChildListOfValue != null)
            {
                value.ChildListOfValue.Label = dto.Label;
                value.ChildListOfValue.Note = dto.Code;
            }

            _valueRepo.Update(value);
            await _valueRepo.SaveChangesAsync();
        }


        public async Task CreateListSubCategory(CreateSubCategoryValueDto categorySubValueDtos)
        {
            var root = await _repository.Get()
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.Id == categorySubValueDtos.Id && x.Type == Entities.Enum.MyValueType.Category)
                ?? throw new Exception("ListOfValue not found");

            var values = new List<Value>();

            foreach (var val in categorySubValueDtos.CreateCategoryValueDtos)
            {
                var childLov = new ListOfValue
                {
                    Id = Guid.NewGuid(),
                    Label = val.Label,
                    Note = val.Code,
                    Type = Entities.Enum.MyValueType.Category,
                    Values = new List<Value>()
                };

                var value = new Value
                {
                    Code = val.Code,
                    Label = val.Label,
                    Description = val.Description,
                    Type = Entities.Enum.MyValueType.Category,
                    ListOfValueId = root.Id,
                    ChildListOfValueId = childLov.Id,
                    ChildListOfValue = childLov
                };

                values.Add(value);
            }

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

        public async Task<List<CategoryValueDto>> GetRootListValueCategory()
        {
            var category = await _repository.Get().Include(x => x.Values).ThenInclude(x => x.ChildListOfValue).Where(x => x.Note == "CATEGORY").FirstOrDefaultAsync() ?? throw new Exception("Category not found");
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
            if (value.ListOfValueId == sublistCategoryId)
            {
                throw new Exception("Circular reference detected. Cannot link category.");
            }
            var root = await _repository.Get().FirstOrDefaultAsync(x => x.Id == sublistCategoryId)
                     ?? throw new Exception("Sublist category not found");

            if (root.Note == "CATEGORY")
                throw new Exception("Don't link root category");

            if (await HasCircularReference(value.ListOfValueId, sublistCategoryId))
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

                    result.Add(new CategoryValueLeafWithPathDto
                    {
                        ValueId = node.ValueId,
                        Code = node.Code,
                        Label = node.Label,
                        Description = node.Description,
                        Type = node.Type,
                        Path = currentPath
                    });

                    if (node.Children != null && node.Children.Any())
                    {
                        foreach (var child in node.Children)
                        {
                            Traverse(child.Values, currentPath);
                        }
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

        private async Task<bool> HasCircularReference(Guid sourceListOfValueId, Guid? targetListOfValueId)
        {
            if (targetListOfValueId == null)
                return false;

            var visited = new HashSet<Guid>();
            var toVisit = new Queue<Guid>();
            toVisit.Enqueue(targetListOfValueId.Value);

            while (toVisit.Count > 0)
            {
                var currentListId = toVisit.Dequeue();

                // Nếu đã duyệt qua rồi thì bỏ qua (chặn vòng lặp)
                if (!visited.Add(currentListId))
                    continue;

                // Nếu vòng tròn xảy ra
                if (currentListId == sourceListOfValueId)
                    return true;

                // Truy vấn tất cả các Value trong List hiện tại mà có liên kết tới list khác
                var values = await _valueRepo.Get()
                    .Where(v => v.ListOfValueId == currentListId && v.ChildListOfValueId != null)
                    .ToListAsync();

                foreach (var value in values)
                {
                    if (value.ChildListOfValueId.HasValue)
                        toVisit.Enqueue(value.ChildListOfValueId.Value);
                }
            }

            return false;
        }
    }
}

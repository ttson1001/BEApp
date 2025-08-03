using AutoMapper;
using BEAPI.Constants;
using BEAPI.Database;
using BEAPI.Dtos.Value;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ValueService : IValueService
    {
        private readonly IRepository<Value> _valueRepo;
        private readonly IRepository<ListOfValue> _listOfValueRepo;

        private readonly IMapper _mapper;

        public ValueService(IRepository<Value> valueRepo, IMapper mapper, IRepository<ListOfValue> listOfValueRepo)
        {
            _valueRepo = valueRepo;
            _mapper = mapper;
            _listOfValueRepo = listOfValueRepo;
        }
        public async Task Create(ValueCreateDto dto)
        {
            if (await _valueRepo.Get().AnyAsync(x => x.Code == dto.Code))
                throw new Exception($"{nameof(Value)} with code '{dto.Code}' already exists");

            var listOfValueId = GuidHelper.ParseOrThrow(dto.ListOfValueId, nameof(dto.ListOfValueId));

            if (!await _listOfValueRepo.Get().AnyAsync(x => x.Id == listOfValueId))
                throw new Exception($"{nameof(ListOfValue)} with id '{listOfValueId}' not found");

            Guid? childListOfValueId = null;
            if (!string.IsNullOrWhiteSpace(dto.ChildListOfValueId))
            {
                childListOfValueId = GuidHelper.ParseOrThrow(dto.ChildListOfValueId, nameof(dto.ChildListOfValueId));

                bool childExists = await _listOfValueRepo.Get().AnyAsync(x => x.Id == childListOfValueId);
                if (!childExists)
                    throw new Exception($"{nameof(ListOfValue)} with id '{childListOfValueId}' not found");
            }

            var entity = _mapper.Map<Value>(dto);
            entity.ListOfValueId = listOfValueId;
            entity.ChildListOfValueId = childListOfValueId;

            await _valueRepo.AddAsync(entity);
            await _valueRepo.SaveChangesAsync();
        }

        public async Task Update(ValueUpdateDto dto)
        {
            var id = GuidHelper.ParseOrThrow(dto.Id, nameof(dto.Id));
            var existing = await _valueRepo.Get()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new Exception($"{nameof(Value)} not found");

            if (await _valueRepo.Get().AnyAsync(x => x.Code == dto.Code && x.Id != id))
                throw new Exception($"{nameof(Value)} with code '{dto.Code}' already exists");

            var listOfValueId = GuidHelper.ParseOrThrow(dto.ListOfValueId, nameof(dto.ListOfValueId));
            if (!await _listOfValueRepo.Get().AnyAsync(x => x.Id == listOfValueId))
                throw new Exception($"{nameof(ListOfValue)} with id '{listOfValueId}' not found");

            Guid? childListOfValueId = null;
            if (!string.IsNullOrWhiteSpace(dto.ChildListOfValueId))
            {
                childListOfValueId = GuidHelper.ParseOrThrow(dto.ChildListOfValueId, nameof(dto.ChildListOfValueId));

                if (!await _listOfValueRepo.Get().AnyAsync(x => x.Id == childListOfValueId))
                    throw new Exception($"{nameof(ListOfValue)} with id '{childListOfValueId}' not found");
            }

            _mapper.Map(dto, existing);

            existing.ListOfValueId = listOfValueId;
            existing.ChildListOfValueId = childListOfValueId;

            _valueRepo.Update(existing);
            await _valueRepo.SaveChangesAsync();
        }


        public async Task<List<ValueDto>> GetValuesByListIdAsync(string listOfValueId)
        {
            var listId = GuidHelper.ParseOrThrow(listOfValueId, nameof(listOfValueId));

            var listExists = await _listOfValueRepo.Get().AnyAsync(x => x.Id == listId);
            if (!listExists)
                throw new Exception($"{nameof(ListOfValue)} not found");

            var values = await _valueRepo.Get()
                .Where(x => x.ListOfValueId == listId)
                .ToListAsync();

            return _mapper.Map<List<ValueDto>>(values);
        }

        public async Task<List<ValueDto>> GetValuesByNoteAsync(string note)
        {
            var listExists = await _listOfValueRepo.Get().AnyAsync(x => x.Note == note);
            if (!listExists)
                throw new Exception($"{nameof(ListOfValue)} not found");
            var values = await _valueRepo.Get().Include(x => x.ListOfValue)
                .Where(x => x.ListOfValue != null && x.ListOfValue.Note.Contains(note))
                .ToListAsync();

            return _mapper.Map<List<ValueDto>>(values);
        }

        public async Task<List<ValueDto>> GetAllValuesAsync()
        {
            var values = await _valueRepo.Get()
                .Include(x => x.ListOfValue)
                .ToListAsync();

            return _mapper.Map<List<ValueDto>>(values);
        }

        public async Task<ValueTreeDto> GetValueWithChildrenAsync(Guid valueId)
        {
            var value = await _valueRepo.Get()
                .Include(v => v.ChildListOfValue)
                    .ThenInclude(clv => clv.Values)
                .FirstOrDefaultAsync(v => v.Id == valueId)
                ?? throw new Exception("Value not found");

            return BuildValueTreeRecursive(value);
        }

        private ValueTreeDto BuildValueTreeRecursive(Value value)
        {
            return new ValueTreeDto
            {
                Id = value.Id.ToString(),
                Label = value.Label,
                Code = value.Code,
                Description = value.Description,
                Children = value.ChildListOfValue != null
                    ? value.ChildListOfValue.Values.Select(BuildValueTreeRecursive).ToList()
                    : new List<ValueTreeDto>()
            };
        }
    }
}

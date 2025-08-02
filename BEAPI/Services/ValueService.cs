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
        public async Task Create(ValueCreateDto valueCreateDto)
        {
            var exists = await _valueRepo
                .Get().AnyAsync(x => x.Code == valueCreateDto.Code);
            if (exists)
                throw new Exception($"{nameof(Value)} with code '{valueCreateDto.Code}' already exists");

            var entity = _mapper.Map<Value>(valueCreateDto);
            entity.ListOfValueId = Guid.Parse(valueCreateDto.ListOfValueId);

            var listId = GuidHelper.ParseOrThrow(valueCreateDto.ListOfValueId, nameof(valueCreateDto.ListOfValueId));
            var listExists = await _listOfValueRepo.Get()
                   .AnyAsync(x => x.Id == listId);
            if (!listExists)
                throw new Exception($"{nameof(ListOfValue)} not found");
            await _valueRepo.AddAsync(entity);
            await _valueRepo.SaveChangesAsync();
        }

        public async Task Update(ValueUpdateDto valueUpdateDto)
        {
            var id = GuidHelper.ParseOrThrow(valueUpdateDto.Id, nameof(valueUpdateDto.Id));
            var existing = await _valueRepo.Get()
                .FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception($"{nameof(Value)} not found");

            var duplicate = await _valueRepo.Get()
                .AnyAsync(x => x.Code == valueUpdateDto.Code && x.Id != id);
            if (duplicate)
                throw new Exception($"{nameof(Value)} with code '{valueUpdateDto.Code}' already exists");

            var listId = GuidHelper.ParseOrThrow(valueUpdateDto.ListOfValueId, nameof(valueUpdateDto.ListOfValueId));

            var listExists = await _valueRepo.Get()
                .AnyAsync(x => x.Id == listId);
            if (!listExists)
                throw new Exception($"{nameof(ListOfValue)} not found");

            _mapper.Map(valueUpdateDto, existing);
            existing.ListOfValueId = listId;

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
    }
}

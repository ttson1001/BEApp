using AutoMapper;
using BEAPI.Database;
using BEAPI.Dtos.ListOfValue;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ListOfValueService : IListOfValueService
    {
        private readonly IRepository<ListOfValue> _repository;
        private readonly IMapper _mapper;
        public ListOfValueService(IRepository<ListOfValue> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task Create(ListOfValueCreateDto listOfValueCreateDto)
        {
            var listOfValueExists = await _repository.Get().FirstOrDefaultAsync(x => x.Note == listOfValueCreateDto.Note || x.Label == listOfValueCreateDto.Note);
            if (listOfValueExists != null)
            {
                throw new Exception(ExceptionConstant.ListOfValueAlreadyExists);
            }
            var listOfValue = _mapper.Map<ListOfValue>(listOfValueCreateDto);
            await _repository.AddAsync(listOfValue);
            await _repository.SaveChangesAsync();
        }

        public async Task Update(ListOfValueUpdateDto listOfValueUpdateDto)
        {
            var listOfValueExists = await _repository.Get().FirstOrDefaultAsync(x => x.Id == Guid.Parse(listOfValueUpdateDto.Id)) ?? throw new Exception(ExceptionConstant.ListOfValueNotFounds);
            var listOfValue = _mapper.Map<ListOfValue>(listOfValueUpdateDto);
            _repository.Update(listOfValue);
            await _repository.SaveChangesAsync();
        }

        public async Task<List<ListOfValueDto>> GetAllAsync()
        {
            var entities = await _repository.Get()
                .Include(l => l.Values)
                .ToListAsync();

            return _mapper.Map<List<ListOfValueDto>>(entities);
        }
        public async Task<ListOfValueDto> GetByIdAsync(string id)
        {
            var guid = GuidHelper.ParseOrThrow(id, nameof(id));

            var entity = await _repository.Get()
                .Include(l => l.Values)
                .FirstOrDefaultAsync(l => l.Id == guid);

            return entity == null ? throw new Exception($"{nameof(ListOfValue)} not found") : _mapper.Map<ListOfValueDto>(entity);
        }

        public async Task<List<ListOfValueDto>> GetByNoteAsync(string note)
        {
            var entities = await _repository.Get()
                .Include(l => l.Values)
                .Where(l => l.Note.Contains(note))
                .ToListAsync();

            return _mapper.Map<List<ListOfValueDto>>(entities);
        }
    }
}

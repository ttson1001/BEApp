using AutoMapper;
using BEAPI.Dtos.AppsetiingDto;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class AppDbSettingsService : IAppDbSettingsService
    {
        private readonly IRepository<AppDbSettings> _repository;
        private readonly IMapper _mapper;

        public AppDbSettingsService(IRepository<AppDbSettings> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<AppDbSettingsDto>> GetAllAsync()
        {
            var settings = await _repository.Get().ToListAsync();
            return _mapper.Map<List<AppDbSettingsDto>>(settings);
        }

        public async Task<AppDbSettingsDto> GetByIdAsync(Guid id)
        {
            var setting = await _repository.Get().FirstOrDefaultAsync(x => x.Id == id)
                          ?? throw new Exception("Setting not found");
            return _mapper.Map<AppDbSettingsDto>(setting);
        }

        public async Task<AppDbSettingsDto> CreateAsync(CreateAppDbSettingsDto dto)
        {
            var entity = _mapper.Map<AppDbSettings>(dto);

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return _mapper.Map<AppDbSettingsDto>(entity);
        }

        public async Task EditAsync(UpdateAppDbSettingsDto dto)
        {
            var setting = await _repository.Get().FirstOrDefaultAsync(x => x.Id == dto.Id)
                          ?? throw new Exception("Setting not found");

            setting.Key = dto.Key;
            setting.Value = dto.Value;

            _repository.Update(setting);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var setting = await _repository.Get().FirstOrDefaultAsync(x => x.Id == id)
                          ?? throw new Exception("Setting not found");

            _repository.Delete(setting);
            await _repository.SaveChangesAsync();
        }
    }
}
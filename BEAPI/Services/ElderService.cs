using AutoMapper;
using BEAPI.Dtos.Elder;
using BEAPI.Entities;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ElderService : IElderService
    {
        private readonly IRepository<User> _repository;
        private readonly IMapper _mapper;

        public ElderService(IMapper mapper, IRepository<User> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<List<ElderDto>> GetElderByCusId(string cusId)
        {
            var guidCus = GuidHelper.ParseOrThrow(cusId, nameof(cusId));
            var list = await _repository.Get().Where(x => x.GuardianId == guidCus).ToListAsync();
            return _mapper.Map<List<ElderDto>>(list);
        }

        public async Task UpdateElderAsync(ElderUpdateDto dto)
        {
            var userId = GuidHelper.ParseOrThrow(dto.Id, nameof(dto.Id));
            var user = await _repository.Get().FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("User not found");
            _mapper.Map(dto, user);

            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }

        public async Task ChangeIsDeletedAsync(string id)
        {
            var userId = GuidHelper.ParseOrThrow(id, "UserId");

            var user = await _repository.Get().FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("User not found");
            if (user.IsDeleted) { user.DeletionDate = DateTimeOffset.UtcNow; }
            else { user.DeletionDate = null; }
            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }
    }
}

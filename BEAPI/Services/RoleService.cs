using AutoMapper;
using BEAPI.Dtos.Role;
using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _repository;
        private readonly IMapper _mapper;
        public RoleService(IRepository<Role> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            var excludeIds = new Guid[]
            {
        Guid.Parse("33333333-3333-3333-3333-333333333333")
            };

            var rs = await _repository.Get()
                .Where(r => !excludeIds.Contains(r.Id))
                .ToListAsync();

            return _mapper.Map<List<RoleDto>>(rs);
        }

        public async Task<List<RoleDto>> GetRolesFilterAsync()
        {
            var excludeIds = new Guid[]{Guid.Parse("33333333-3333-3333-3333-333333333333") };

            var rs = await _repository.Get()
                .Where(r => !excludeIds.Contains(r.Id))
                .ToListAsync();

            return _mapper.Map<List<RoleDto>>(rs);
        }

    }
}

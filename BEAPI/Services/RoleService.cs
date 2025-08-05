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
            var rs = await _repository.Get().ToListAsync();
            return _mapper.Map<List<RoleDto>>(rs);
        }
    }
}

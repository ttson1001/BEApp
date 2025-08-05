using BEAPI.Dtos.Role;

namespace BEAPI.Services.IServices
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetRolesAsync();
    }
}

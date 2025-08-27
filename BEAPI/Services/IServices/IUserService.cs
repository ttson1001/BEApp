using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.User;

namespace BEAPI.Services.IServices
{
    public interface IUserService
    {
        Task CreateElder(ElderRegisterDto elderRegisterDto, Guid userId);
        Task<string> LoginByQrAsync(string token, string? deviceId);
        Task<(string Token, string QrBase64)> GenerateElderLoginQrAsync(Guid elderId);
        Task<PagedResult<UserListDto>> FilterUsersAsync(UserFilterDto request);
        Task CreateUserAsync(UserCreateDto dto);
        Task UpdateUserAsync(UserUpdateDto dto);
        Task BanOrUnbanUserAsync(string userId);
        Task<UserDetailDto> GetDetailAsync(Guid id);
    }
}

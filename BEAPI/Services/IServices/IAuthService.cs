using BEAPI.Dtos.Auth;
using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface IAuthService
    {
        Task RegisterAsync (RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto dto);
        Task<User> FindUserByEmailOrPhoneAsync(string emailOrPhone);
        Task ResetPasswordAsync(ResetPasswordDto request);
        Task ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    }
}

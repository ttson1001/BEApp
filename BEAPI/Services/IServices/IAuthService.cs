using BEAPI.Dtos.Auth;
using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface IAuthService
    {
        Task RegisterAsync (RegisterDto registerDto);
        Task<(string Token, User User)> LoginAsync(LoginDto dto);
        Task<User> FindUserByEmailOrPhoneAsync(string emailOrPhone);
        Task ResetPasswordAsync(ResetPasswordDto request);
        Task VerifyUserAsync(string otp);
        Task ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    }
}

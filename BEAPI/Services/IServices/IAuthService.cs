using BEAPI.Dtos.Auth;

namespace BEAPI.Services.IServices
{
    public interface IAuthService
    {
        Task RegisterAsync (RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto dto);
    }
}

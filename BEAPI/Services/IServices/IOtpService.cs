using BEAPI.Entities;

namespace BEAPI.Services.IServices
{
    public interface IOtpService
    {
        Task GenerateAndSendOtpAsync(User user);
        Task VerifyOtpAsync(User user, string otpCode);
        Task VerifyUserAsync(User user, string otpCode);
    }
}

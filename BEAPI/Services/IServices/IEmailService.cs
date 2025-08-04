namespace BEAPI.Services.IServices
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
    }
}

using BEAPI.Database;
using BEAPI.Entities;
using BEAPI.Services.IServices;

public class OtpService : IOtpService
{
    private readonly IEmailService _emailService;
    private readonly BeContext _db;

    public OtpService(IEmailService emailService, BeContext db)
    {
        _emailService = emailService;
        _db = db;
    }

    public async Task GenerateAndSendOtpAsync(User user)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        user.OtpCode = otp;
        user.OtpExpiredAt = DateTimeOffset.UtcNow.AddMinutes(10);
        user.IsOtpUsed = false;

        await _emailService.SendOtpAsync(user.Email!, otp);
        await _db.SaveChangesAsync();
    }

    public async Task VerifyOtpAsync(User user, string otpCode)
    {
        if (user.OtpCode != otpCode)
            throw new Exception("OTP không chính xác.");

        if (user.OtpExpiredAt < DateTimeOffset.UtcNow)
            throw new Exception("OTP đã hết hạn.");

        if (user.IsOtpUsed)
            throw new Exception("OTP đã được sử dụng.");

        user.IsOtpUsed = true;
        await _db.SaveChangesAsync();
    }

    public async Task VerifyUserAsync(User user, string otpCode)
    {
        if (user.OtpCode != otpCode)
            throw new Exception("OTP không chính xác.");

        if (user.OtpExpiredAt < DateTimeOffset.UtcNow)
            throw new Exception("OTP đã hết hạn.");

        if (user.IsOtpUsed)
            throw new Exception("OTP đã được sử dụng.");

        user.IsOtpUsed = true;
        user.IsVerified = true;

        await _db.SaveChangesAsync();
    }


}

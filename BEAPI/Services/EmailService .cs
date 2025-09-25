using BEAPI.Entities;
using BEAPI.Model;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IRepository<User> _userRepository;

    public EmailService(IOptions<EmailSettings> settings, IRepository<User> userRepository)
    {
        _settings = settings.Value;
        _userRepository = userRepository;
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Complete Your Register: Your OTP Verification Code";
        var user = await _userRepository.Get()
                .FirstOrDefaultAsync(u => u.Email == toEmail);
        string userName = user != null ? user.FullName : toEmail.Split('@')[0];
        email.Body = new TextPart("html")
        {
            Text = $@"
<html>
<body
    style='background: #f7fbff; margin: 0; padding: 0; font-family: \""Segoe UI\"", Arial, sans-serif; min-height:100vh;'>
    <div
        style='max-width: 440px; margin: 40px auto; background: #fff; border-radius: 18px; box-shadow: 0 6px 32px rgba(255,163,108,0.10); border: 1.5px solid #fff3ea; padding: 40px 36px 32px 36px;'>
        <div style='text-align: center; margin-bottom: 18px;'>
            <img src='https://res.cloudinary.com/dr66bqpgp/image/upload/v1758761971/LOGO_Silver_Cart_vxn2ug.png' alt='Silver Cart logo'
                style='width: 100px; height: 100px; object-fit:contain; margin-bottom:8px;' />
        </div>
        <h2
            style='color: #FF974A; font-size: 1.6em; font-weight: 800; margin: 0 0 18px 0; text-align:center; letter-spacing:0.5px;'>
            Mã xác thực OTP của bạn</h2>
        <p style='font-size: 17px; margin: 0 0 16px 0; text-align:center; color:#35677D;'>
            Xin chào <span style='font-weight: bold; color: #FF974A;'>{userName}</span>,
        </p>
        <p style='font-size: 15.5px; margin-bottom: 22px; color: #3689A6; text-align:center;'>
            Cảm ơn bạn đã đăng ký Silver Cart!<br>
            Vui lòng dùng mã xác thực bên dưới để hoàn tất đăng ký tài khoản:
        </p>
        <div
            style='background: #fff3ea; border-radius: 14px; font-size: 2.1em; font-weight: 900; color: #FF974A; text-align: center; padding: 18px 0; margin: 0 0 18px 0; letter-spacing: 8px; box-shadow: 0 2px 10px #ff974a14;'>
            {otp}
        </div>
        <div
            style='background: #e5f7ee; border-radius: 8px; padding: 10px 16px; font-size: 14.5px; color: #368a6e; text-align:center; margin-bottom: 18px; border-left: 4px solid #8CD993;'>
            Mã OTP này có hiệu lực trong <b>10 phút</b>. <b>Không chia sẻ</b> mã này với bất kỳ ai vì lý do bảo mật.
        </div>
        <p style='font-size: 14px; color: #35677D; text-align:center; margin-bottom: 32px;'>
            Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email hoặc liên hệ hỗ trợ Silver Cart.
        </p>
        <div style='text-align: center; margin-top: 32px;'>
            <span style='font-size: 15px; color: #FF974A; font-weight: 700;'>Thân ái,<br>
                <span style='color: #3689A6; font-weight: 800;'>Silver Cart Team</span></span>
        </div>
    </div>
    <div style='text-align:center; color:#93d77c; font-size:11px; margin-top:18px;'>
        &copy; {DateTime.UtcNow.Year} Silver Cart. All rights reserved.
    </div>
</body>
</html>"
        };
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}

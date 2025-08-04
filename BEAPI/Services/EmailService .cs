using BEAPI.Model;
using BEAPI.Services.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Your OTP Code";

        email.Body = new TextPart("plain")
        {
            Text = $"Your OTP code is: {otp}\nThis code will expire in 10 minutes."
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}

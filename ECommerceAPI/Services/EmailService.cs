using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ECommerceAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendOtpEmail(string toEmail, string userName, string otpCode)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]!);
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var smtpFrom = _configuration["Smtp:From"];

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpFrom));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Your OTP - ECommerce App";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family:Arial; max-width:500px; margin:auto;'>
                    <h2 style='color:#1F3864;'>Email Verification</h2>
                    <p>Hi <b>{userName}</b>,</p>
                    <p>Your OTP code is:</p>
                    <h1 style='letter-spacing:8px; color:#2E5DA6;'>{otpCode}</h1>
                    <p>This OTP is valid for <b>15 minutes</b>.</p>
                    <p style='color:gray; font-size:12px;'>If you didn't request this, please ignore.</p>
                </div>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(smtpUser, smtpPass);
            smtp.Send(message);
            smtp.Disconnect(true);
        }
    }
}
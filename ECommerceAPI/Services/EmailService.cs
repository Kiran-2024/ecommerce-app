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

        // Common SMTP send logic — anni email methods ee helper vadtai
        private void SendEmail(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]!);
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var smtpFrom = _configuration["Smtp:From"];

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpFrom));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(smtpUser, smtpPass);
            smtp.Send(message);
            smtp.Disconnect(true);
        }

        public void SendOtpEmail(string toEmail, string userName, string otpCode)
        {
            var body = $@"
                <div style='font-family:Arial; max-width:500px; margin:auto;'>
                    <h2 style='color:#1F3864;'>Email Verification</h2>
                    <p>Hi <b>{userName}</b>,</p>
                    <p>Your OTP code is:</p>
                    <h1 style='letter-spacing:8px; color:#2E5DA6;'>{otpCode}</h1>
                    <p>This OTP is valid for <b>15 minutes</b>.</p>
                    <p style='color:gray; font-size:12px;'>If you didn't request this, please ignore.</p>
                </div>";

            SendEmail(toEmail, "Your OTP - ECommerce App", body);
        }

        public void SendOrderConfirmation(string toEmail, string userName, int orderId,
            decimal totalAmount, string paymentMethod, List<(string ProductName, int Quantity, decimal TotalPrice)> items)
        {
            var itemsHtml = string.Join("", items.Select(i => $@"
                <tr>
                    <td style='padding:8px; border-bottom:1px solid #eee;'>{i.ProductName}</td>
                    <td style='padding:8px; border-bottom:1px solid #eee; text-align:center;'>{i.Quantity}</td>
                    <td style='padding:8px; border-bottom:1px solid #eee; text-align:right;'>₹{i.TotalPrice}</td>
                </tr>"));

            var body = $@"
                <div style='font-family:Arial; max-width:550px; margin:auto;'>
                    <h2 style='color:#1D9E75;'>✅ Order Confirmed!</h2>
                    <p>Hi <b>{userName}</b>,</p>
                    <p>Your order <b>#{orderId}</b> has been placed successfully.</p>

                    <table style='width:100%; border-collapse:collapse; margin-top:16px;'>
                        <thead>
                            <tr style='background:#f0fdf8;'>
                                <th style='padding:8px; text-align:left;'>Product</th>
                                <th style='padding:8px; text-align:center;'>Qty</th>
                                <th style='padding:8px; text-align:right;'>Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>

                    <p style='font-size:16px; font-weight:bold; text-align:right; margin-top:12px;'>
                        Total: ₹{totalAmount}
                    </p>

                    <p>Payment Method: <b>{paymentMethod}</b></p>
                    <p style='color:gray; font-size:12px; margin-top:20px;'>
                        Thank you for shopping with us!
                    </p>
                </div>";

            SendEmail(toEmail, $"Order Confirmed - #{orderId}", body);
        }

        public void SendStatusUpdate(string toEmail, string userName, int orderId, string newStatus)
        {
            var body = $@"
                <div style='font-family:Arial; max-width:500px; margin:auto;'>
                    <h2 style='color:#2E5DA6;'>📦 Order Status Updated</h2>
                    <p>Hi <b>{userName}</b>,</p>
                    <p>Your order <b>#{orderId}</b> status has been updated to:</p>
                    <h2 style='color:#1D9E75;'>{newStatus}</h2>
                    <p style='color:gray; font-size:12px; margin-top:20px;'>
                        Thank you for shopping with us!
                    </p>
                </div>";

            SendEmail(toEmail, $"Order #{orderId} - Status: {newStatus}", body);
        }
    }
}
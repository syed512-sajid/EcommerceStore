using EcommerceStore.Models;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EcommerceStore.Services
{
    //public interface IEmailService
    //{
    //    Task SendOrderConfirmationAsync(Order order, List<CartItem> cart);
    //    Task SendAdminNotificationAsync(Order order, List<CartItem> cart);
    //}

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, Microsoft.Extensions.Options.IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;

            // Load from environment variables (Railway)
            var envUser = Environment.GetEnvironmentVariable("info.bazario.store@gmail.com");
            var envPass = Environment.GetEnvironmentVariable("zihx tkid hisi svht");

            if (!string.IsNullOrEmpty(envUser))
            {
                _emailSettings.SmtpUser = envUser;
                _emailSettings.FromEmail = envUser;
            }

            if (!string.IsNullOrEmpty(envPass))
            {
                _emailSettings.SmtpPass = envPass;
            }
        }

        public async Task SendOrderConfirmationAsync(Order order, List<CartItem> cart)
        {
            try
            {
                if (string.IsNullOrEmpty(_emailSettings.SmtpUser) || string.IsNullOrEmpty(_emailSettings.SmtpPass))
                {
                    _logger.LogWarning("⚠️ Email credentials not configured. Skipping customer email.");
                    return;
                }

                _logger.LogInformation("📧 Preparing customer email for Order #{OrderId}", order.Id);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(order.CustomerName, order.Email));
                message.Subject = $"✅ Order Confirmed - BAZARIO #{order.Id}";

                string body = BuildCustomerEmailBody(order, cart);
                message.Body = new TextPart("html") { Text = body };

                await SendEmailAsync(message);
                _logger.LogInformation("✅ Customer email sent to {Email}", order.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send customer email for Order #{OrderId}", order.Id);
            }
        }

        public async Task SendAdminNotificationAsync(Order order, List<CartItem> cart)
        {
            try
            {
                if (string.IsNullOrEmpty(_emailSettings.SmtpUser) || string.IsNullOrEmpty(_emailSettings.SmtpPass))
                {
                    _logger.LogWarning("⚠️ Email credentials not configured. Skipping admin email.");
                    return;
                }

                _logger.LogInformation("📧 Preparing admin email for Order #{OrderId}", order.Id);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("Admin", "sajidabbas6024@gmail.com"));
                message.Subject = $"🔔 New Order #{order.Id} from {order.CustomerName}";

                string body = BuildAdminEmailBody(order, cart);
                message.Body = new TextPart("html") { Text = body };

                await SendEmailAsync(message);
                _logger.LogInformation("✅ Admin email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send admin email for Order #{OrderId}", order.Id);
            }
        }
        public async Task SendOtpAsync(string email, string otp)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "🔐 Admin Login OTP - BAZARIO";

                message.Body = new TextPart("html")
                {
                    Text = $@"
            <div style='font-family:Arial; padding:20px;'>
                <h2>Admin Login Verification</h2>
                <p>Your OTP is:</p>
                <h1 style='letter-spacing:4px;'>{otp}</h1>
                <p>This OTP will expire in <b>5 minutes</b>.</p>
                <p>If this was not you, please ignore this email.</p>
            </div>"
                };

                await SendEmailAsync(message);
                _logger.LogInformation("✅ OTP email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send OTP email");
            }
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient();

            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.CheckCertificateRevocation = false;
            client.Timeout = 30000; // 30 second timeout

            _logger.LogInformation("🔌 Connecting to SMTP {Host}:{Port}", _emailSettings.SmtpHost, _emailSettings.SmtpPort);

            var secureSocketOptions = _emailSettings.SmtpPort == 587
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureSocketOptions);
            _logger.LogInformation("🔐 Authenticating as {User}", _emailSettings.SmtpUser);

            await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
            _logger.LogInformation("📤 Sending email...");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent successfully");
        }

        private string BuildCustomerEmailBody(Order order, List<CartItem> cart)
        {
            string itemsHtml = "";
            foreach (var item in cart)
            {
                itemsHtml += $@"
        <tr style='border-bottom:1px solid #eee;'>
            <td style='padding:10px;'>{item.ProductName}</td>
            <td style='padding:10px; text-align:center;'>{item.Quantity}</td>
            <td style='padding:10px; text-align:right;'>Rs. {item.Price:N0}</td>
        </tr>";
            }

            return $@"
<div style='font-family: Arial, sans-serif; max-width:600px; margin:0 auto; background:#f5f5f5; padding:20px;'>

    <div style='background:linear-gradient(135deg,#28a745,#218838); color:#fff; padding:30px; text-align:center; border-radius:10px 10px 0 0;'>
        <h1 style='margin:0;'>✅ Order Confirmed</h1>
        <p style='margin-top:10px;'>Thank you for shopping with BAZARIO</p>
    </div>

    <div style='background:#fff; padding:30px; border-radius:0 0 10px 10px;'>
        <p style='font-size:16px;'>Hi <strong>{order.CustomerName}</strong>,</p>

        <p style='font-size:16px; line-height:1.6;'>
            Your order <strong>#{order.Id}</strong> has been successfully placed.
            Our team will start processing it shortly.
        </p>

        <div style='background:#e9f7ef; padding:20px; border-radius:8px; margin:20px 0; border-left:4px solid #28a745;'>
            <p><strong>Order ID:</strong> #{order.Id}</p>
            <p><strong>Order Date:</strong> {order.OrderDate:dd MMM yyyy, hh:mm tt}</p>
            <p><strong>Payment Method:</strong> {order.PaymentMethod}</p>
            <p><strong>Payment Method:</strong> {order.TrackingId}</p>
        </div>

        <h3 style='margin-top:30px;'>🛒 Order Details</h3>
        <table style='width:100%; border-collapse:collapse;'>
            <thead>
                <tr style='background:#28a745; color:#fff;'>
                    <th style='padding:10px; text-align:left;'>Product</th>
                    <th style='padding:10px;'>Qty</th>
                    <th style='padding:10px; text-align:right;'>Price</th>
                </tr>
            </thead>
            <tbody>{itemsHtml}</tbody>
            <tfoot>
                <tr style='font-weight:bold;'>
                    <td colspan='2' style='padding:10px;'>Total</td>
                    <td style='padding:10px; text-align:right; color:#28a745; font-size:18px;'>Rs. {order.TotalAmount:N0}</td>
                </tr>
            </tfoot>
        </table>

        <div style='background:#f8f9fa; padding:20px; border-radius:8px; margin:20px 0;'>
            <h3>🚚 Delivery Address</h3>
            <p>{order.Address}</p>
            <p><strong>Phone:</strong> {order.Phone}</p>
        </div>

        <p style='margin-top:20px;'>
            You will receive another email once your order is processed.
        </p>

        <p>
            Regards,<br>
            <strong>BAZARIO Team</strong>
        </p>
    </div>

    <div style='text-align:center; margin-top:15px; font-size:12px; color:#888;'>
        © {DateTime.Now.Year} BAZARIO Store
    </div>
</div>";
        }

        private string BuildAdminEmailBody(Order order, List<CartItem> cart)
        {
            string itemsHtml = "";
            foreach (var item in cart)
            {
                itemsHtml += $@"
        <tr>
            <td style='padding:8px;'>{item.ProductName}</td>
            <td style='padding:8px; text-align:center;'>{item.Quantity}</td>
            <td style='padding:8px; text-align:right;'>Rs. {item.Price:N0}</td>
        </tr>";
            }

            return $@"
<div style='font-family:Arial,sans-serif; max-width:600px; margin:0 auto; background:#f4f6f8; padding:20px;'>

    <div style='background:linear-gradient(135deg,#6f42c1,#563d7c); color:#fff; padding:25px; border-radius:10px 10px 0 0;'>
        <h2 style='margin:0;'>📢 New Order Alert</h2>
        <p style='margin-top:8px;'>Order #{order.Id} received</p>
    </div>

    <div style='background:#fff; padding:25px; border-radius:0 0 10px 10px;'>
        <div style='background:#fff3cd; padding:15px; border-left:4px solid #ffc107; border-radius:5px;'>
            <strong>Customer:</strong> {order.CustomerName}<br>
            <strong>Phone:</strong> {order.Phone}<br>
            <strong>Email:</strong> {order.Email}
        </div>

        <h3 style='margin-top:25px;'>📦 Order Items</h3>
        <table style='width:100%; border-collapse:collapse;'>
            <thead>
                <tr style='background:#6f42c1; color:#fff;'>
                    <th style='padding:8px; text-align:left;'>Product</th>
                    <th style='padding:8px;'>Qty</th>
                    <th style='padding:8px; text-align:right;'>Price</th>
                </tr>
            </thead>
            <tbody>{itemsHtml}</tbody>
            <tfoot>
                <tr style='font-weight:bold;'>
                    <td colspan='2' style='padding:8px;'>Total</td>
                    <td style='padding:8px; text-align:right; color:#28a745;'>Rs. {order.TotalAmount:N0}</td>
                </tr>
            </tfoot>
        </table>

        <div style='background:#e7f3ff; padding:15px; border-radius:8px; margin-top:20px;'>
            <p><strong>Order Date:</strong> {order.OrderDate:dd MMM yyyy hh:mm tt}</p>
            <p><strong>Payment:</strong> {order.PaymentMethod}</p>
            <p><strong>Delivery Address:</strong> {order.Address}</p>
        </div>

        <p style='margin-top:20px; font-weight:bold; color:#dc3545;'>
            ⚠ Please process this order as soon as possible.
        </p>
    </div>
</div>";
        }
    }
}
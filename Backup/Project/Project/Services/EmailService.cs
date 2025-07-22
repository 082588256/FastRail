namespace Project.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
        Task SendEmailAsync(string to, string subject, string htmlContent, List<string> attachments);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            await SendEmailAsync(to, subject, htmlContent, new List<string>());
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent, List<string> attachments)
        {
            try
            {
                // 🎭 MÔ PHỎNG GỬI EMAIL - KHÔNG GỬI THẬT
                _logger.LogWarning("🚨 SIMULATION MODE: This is a mock email - no real email will be sent! 🚨");

                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings.GetValue<string>("SmtpHost", "smtp.gmail.com");
                var smtpPort = emailSettings.GetValue<int>("SmtpPort", 587);
                var fromEmail = emailSettings.GetValue<string>("FromEmail", "noreply@trainbooking.vn");
                var fromName = emailSettings.GetValue<string>("FromName", "Đường sắt Việt Nam");
                var enableSsl = emailSettings.GetValue<bool>("EnableSsl", true);
                var username = emailSettings.GetValue<string>("Username", "");
                var password = emailSettings.GetValue<string>("Password", "");

                // Mô phỏng delay gửi email
                await Task.Delay(Random.Shared.Next(500, 2000));

                // Log thông tin email (thay vì gửi thật)
                _logger.LogInformation("📧 Mock Email Sent:");
                _logger.LogInformation("   To: {To}", to);
                _logger.LogInformation("   Subject: {Subject}", subject);
                _logger.LogInformation("   From: {FromName} <{FromEmail}>", fromName, fromEmail);
                _logger.LogInformation("   SMTP: {SmtpHost}:{SmtpPort}", smtpHost, smtpPort);
                _logger.LogInformation("   HTML Content Length: {Length} chars", htmlContent.Length);

                if (attachments.Any())
                {
                    _logger.LogInformation("   Attachments: {Count}", attachments.Count);
                }

                // Mô phỏng thành công/thất bại (95% thành công)
                var success = Random.Shared.NextDouble() > 0.05;

                if (!success)
                {
                    throw new Exception("🎭 Simulation: Random email failure (5% chance)");
                }

                _logger.LogInformation("✅ Mock email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending mock email to {To}", to);
                throw;
            }
        }

        // Method để gửi email thật (khi cần)
        public async Task SendRealEmailAsync(string to, string subject, string htmlContent)
        {
            // Implementation thật với MailKit hoặc SendGrid
            _logger.LogWarning("Real email sending not implemented yet!");
            await Task.CompletedTask;
        }
    }
}

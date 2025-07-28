using MailKit.Net.Smtp;
using MimeKit;

public interface IEmailService
{
    Task SendRealEmailAsync(string to, string subject, string htmlContent);
    Task SendBookingConfirmationAsync(string to, string bookingCode, string passengerName, decimal totalAmount);
    Task SendPaymentConfirmationAsync(string to, string bookingCode, decimal amount);
    Task SendBookingExpirationAsync(string to, string bookingCode, DateTime expirationTime);
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

    public async Task SendBookingConfirmationAsync(string to, string bookingCode, string passengerName, decimal totalAmount)
    {
        var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Xác nhận đặt vé</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c5aa0;'>Xác nhận đặt vé thành công</h2>
                    <p>Xin chào {passengerName},</p>
                    <p>Cảm ơn bạn đã đặt vé. Dưới đây là thông tin đặt vé của bạn:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p><strong>Mã đặt vé:</strong> {bookingCode}</p>
                        <p><strong>Tổng tiền:</strong> {totalAmount:N0} VND</p>
                    </div>
                    
                    <p>Vui lòng thanh toán trong vòng 15 phút để hoàn tất đặt vé.</p>
                    
                    <div style='margin-top: 20px; padding: 20px; background-color: #fff3cd; border-radius: 5px;'>
                        <p style='margin: 0;'><strong>Lưu ý:</strong> Đơn đặt vé sẽ tự động hủy nếu không được thanh toán đúng hạn.</p>
                    </div>
                </div>
            </body>
            </html>";

        await SendRealEmailAsync(to, "Xác nhận đặt vé tàu", htmlContent);
    }

    public async Task SendPaymentConfirmationAsync(string to, string bookingCode, decimal amount)
    {
        var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Xác nhận thanh toán</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c5aa0;'>Xác nhận thanh toán thành công</h2>
                    
                    <div style='background-color: #e8f5e8; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p><strong>Mã đặt vé:</strong> {bookingCode}</p>
                        <p><strong>Số tiền:</strong> {amount:N0} VND</p>
                        <p><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Trạng thái:</strong> Thanh toán thành công</p>
                    </div>
                    
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi.</p>
                </div>
            </body>
            </html>";

        await SendRealEmailAsync(to, "Xác nhận thanh toán vé tàu", htmlContent);
    }

    public async Task SendBookingExpirationAsync(string to, string bookingCode, DateTime expirationTime)
    {
        var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Cảnh báo hết hạn đặt vé</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #dc3545;'>Cảnh báo: Đặt vé sắp hết hạn</h2>
                    
                    <div style='background-color: #fff3cd; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <p><strong>Mã đặt vé:</strong> {bookingCode}</p>
                        <p><strong>Thời gian hết hạn:</strong> {expirationTime:dd/MM/yyyy HH:mm}</p>
                    </div>
                    
                    <p>Vui lòng hoàn tất thanh toán trước thời gian trên để không bị hủy đặt vé.</p>
                </div>
            </body>
            </html>";

        await SendRealEmailAsync(to, "Cảnh báo: Đặt vé sắp hết hạn", htmlContent);
    }

    public async Task SendRealEmailAsync(string to, string subject, string htmlContent)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = emailSettings.GetValue<string>("SmtpServer"); // Cập nhật key cho đúng với appsettings.json
        var smtpPort = emailSettings.GetValue<int>("SmtpPort");
        var fromEmail = emailSettings.GetValue<string>("SenderEmail"); // Cập nhật key
        var username = emailSettings.GetValue<string>("Username");
        var password = emailSettings.GetValue<string>("Password");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("FastRail System", fromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlContent };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, true);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email đã gửi tới: {To}", to);
    }
}
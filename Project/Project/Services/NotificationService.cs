using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Services
{
    public interface INotificationService
    {
        //Task SendBookingConfirmationEmailAsync(int bookingId);
        Task SendBookingExpirationWarningAsync(int bookingId);
    }

    public class NotificationService : INotificationService
    {
        private readonly FastRailDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            FastRailDbContext context,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        //public async Task SendBookingConfirmationEmailAsync(int bookingId)
        //{
        //    try
        //    {
        //        var booking = await _context.Bookings
        //            .Include(b => b.User)
        //            .Include(b => b.Trip)
        //                .ThenInclude(t => t.Train)
        //            .Include(b => b.Trip)
        //                .ThenInclude(t => t.Route)
        //                    .ThenInclude(r => r.DepartureStation)
        //            .Include(b => b.Trip)
        //                .ThenInclude(t => t.Route)
        //                    .ThenInclude(r => r.ArrivalStation)
        //            .Include(b => b.Tickets)
        //            .Include(b => b.SeatSegments)
        //                .ThenInclude(ss => ss.Seat)
        //                    .ThenInclude(s => s.Carriage)
        //            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        //        if (booking == null || booking.Tickets.Count == 0)
        //        {
        //            _logger.LogWarning("Cannot send confirmation email: booking {BookingId} not found or no tickets", bookingId);
        //            return;
        //        }

        //        var ticket = booking.Tickets.First();
        //        var seat = booking.SeatSegments.First().Seat;

        //        var emailContent = GenerateConfirmationEmailContent(booking, ticket, seat);

        //        await _emailService.SendEmailAsync(
        //            booking.User.Email,
        //            "Xác nhận đặt vé tàu thành công",
        //            emailContent);

        //        // Lưu thông báo vào database
        //        var notification = new Notification
        //        {
        //            UserId = booking.UserId,
        //            Type = "BookingConfirmation",
        //            Title = "Xác nhận đặt vé thành công",
        //            Message = $"Vé tàu {booking.Trip.Train.TrainNumber} đã được đặt thành công. Mã vé: {ticket.TicketCode}",
        //            Priority = "High",
        //            RelatedEntityType = "Booking",
        //            RelatedEntityId = bookingId,
        //            EmailSent = true,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        _context.Notifications.Add(notification);
        //        await _context.SaveChangesAsync();

        //        _logger.LogInformation("Sent booking confirmation email for booking {BookingId}", bookingId);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sending booking confirmation email for booking {BookingId}", bookingId);
        //    }
        //}

        public async Task SendBookingExpirationWarningAsync(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.BookingStatus == "Temporary");

                if (booking == null) return;

                var emailContent = $@"
                <h2>Cảnh báo: Booking sắp hết hạn</h2>
                <p>Xin chào {booking.User.FullName},</p>
                <p>Booking {booking.BookingCode} của bạn sẽ hết hạn trong vài phút nữa.</p>
                <p>Vui lòng hoàn tất thanh toán để giữ chỗ.</p>
                <p>Trân trọng,<br/>Đường sắt Việt Nam</p>
            ";

                await _emailService.SendEmailAsync(
                    booking.User.Email,
                    "Cảnh báo: Booking sắp hết hạn",
                    emailContent);

                _logger.LogInformation("Sent booking expiration warning for booking {BookingId}", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking expiration warning for booking {BookingId}", bookingId);
            }
        }

        private string GenerateConfirmationEmailContent(Booking booking, Ticket ticket, Models.Seat seat)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Xác nhận đặt vé tàu</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c5aa0;'>Xác nhận đặt vé tàu thành công</h2>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h3>Thông tin vé</h3>
                        <p><strong>Mã vé:</strong> {ticket.TicketCode}</p>
                        <p><strong>Mã booking:</strong> {booking.BookingCode}</p>
                        <p><strong>Hành khách:</strong> {ticket.PassengerName}</p>
                    </div>
                    
                    <div style='background-color: #e3f2fd; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h3>Thông tin chuyến đi</h3>
                        <p><strong>Tàu:</strong> {booking.Trip.Train.TrainNumber}</p>
                        <p><strong>Tuyến:</strong> {booking.Trip.Route.RouteName}</p>
                        <p><strong>Khởi hành:</strong> {booking.Trip.DepartureTime:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Đến:</strong> {booking.Trip.ArrivalTime:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Ghế:</strong> {seat.SeatNumber} (Toa {seat.Carriage.CarriageNumber})</p>
                        <p><strong>Loại ghế:</strong> {seat.SeatClass} - {seat.SeatType}</p>
                    </div>
                    
                    <div style='background-color: #e8f5e8; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h3>Thông tin thanh toán</h3>
                        <p><strong>Tổng tiền:</strong> {booking.TotalPrice:N0} VND</p>
                        <p><strong>Trạng thái:</strong> Đã thanh toán</p>
                        <p><strong>Thời gian:</strong> {booking.ConfirmedAt:dd/MM/yyyy HH:mm}</p>
                    </div>
                    
                    <div style='background-color: #fff3cd; padding: 20px; margin: 20px 0; border-radius: 5px;'>
                        <h3>⚠️ Lưu ý quan trọng</h3>
                        <ul>
                            <li>Vui lòng có mặt tại ga trước giờ khởi hành ít nhất 30 phút</li>
                            <li>Mang theo giấy tờ tùy thân để kiểm tra</li>
                            <li>Giữ gìn vé và mã vé để sử dụng khi cần</li>
                        </ul>
                    </div>
                    
                    <p style='text-align: center; color: #666; margin-top: 30px;'>
                        Cảm ơn bạn đã sử dụng dịch vụ đặt vé tàu<br>
                        <strong>Đường sắt Việt Nam</strong>
                    </p>
                </div>
            </body>
            </html>
        ";
        }
    }
}

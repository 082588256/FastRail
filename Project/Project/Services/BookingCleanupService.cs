using Microsoft.EntityFrameworkCore;
using Project;

public class BookingCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BookingCleanupService> _logger;

    public BookingCleanupService(
        IServiceProvider services,
        ILogger<BookingCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredBookings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa booking hết hạn");
            }

            // Chạy mỗi 1 phút
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CleanupExpiredBookings()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FastRailDbContext>();

        var now = DateTime.UtcNow;
        _logger.LogInformation("Bắt đầu dọn dẹp booking hết hạn lúc {now}", now);

        var expiredBookings = await context.Bookings
            .Include(b => b.SeatSegments)
            .Include(b => b.Tickets)
            .Where(b => b.PaymentStatus == "Pending" && b.ExpirationTime <= now)
            .ToListAsync();

        _logger.LogInformation("Tìm thấy {count} booking hết hạn", expiredBookings.Count);

        foreach (var booking in expiredBookings)
        {
            try
            {
                _logger.LogInformation(
                    "Xóa booking {BookingId} - Code: {BookingCode}, ExpirationTime: {ExpirationTime}",
                    booking.BookingId,
                    booking.BookingCode,
                    booking.ExpirationTime);

                // Xóa SeatSegments
                _logger.LogInformation("Xóa {count} SeatSegments", booking.SeatSegments.Count);
                context.SeatSegment.RemoveRange(booking.SeatSegments);

                // Xóa Tickets
                _logger.LogInformation("Xóa {count} Tickets", booking.Tickets.Count);
                context.Ticket.RemoveRange(booking.Tickets);

                // Xóa Booking
                context.Bookings.Remove(booking);

                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Đã xóa thành công booking {BookingCode}",
                    booking.BookingCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi xóa booking {BookingCode}",
                    booking.BookingCode);
            }
        }
    }
}
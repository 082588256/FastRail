using Microsoft.EntityFrameworkCore;
using Project;
using Project.Models;

public static class DataSeeder
{
    public static async Task SeedSeatsAsync(FastRailDbContext context)
    {
        // Kiểm tra xem đã có ghế trong database chưa
        if (await context.Seat.AnyAsync())
        {
            // Nếu đã có ghế thì không cần seed nữa
            return;
        }

        // Reset identity seed của bảng Seat để SeatId bắt đầu lại từ 1
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Seat', RESEED, 0)");

        // Lấy danh sách toa đang hoạt động và nhóm theo TrainId
        var carriageGroups = await context.Carriage
            .Where(c => c.IsActive)
            .GroupBy(c => c.TrainId)
            .ToListAsync();

        var newSeats = new List<Seat>();

        foreach (var group in carriageGroups)
        {
            // Tính tổng số ghế cho mỗi tàu
            var totalTrainSeats = group.Sum(c => c.TotalSeats);
            var currentSeatCount = 0; // Biến đếm số ghế đã tạo cho tàu

            foreach (var carriage in group)
            {
                // Gán loại ghế dựa vào CarriageType
                string seatClass = carriage.CarriageType switch
                {
                    "HardSeat" => "Economy",
                    "SoftSeat" => "Economy",
                    "Sleeper" => "Business",
                    "VIP" => "VIP",
                    _ => "Economy"
                };

                int carriageTotalSeats = carriage.TotalSeats;

                // Tính số ghế cần tạo cho toa hiện tại
                for (int i = 0; i < carriageTotalSeats / 2; i++)
                {
                    currentSeatCount += 2; // Tăng số ghế đã tạo
                    int row = (currentSeatCount - 1) / 2 + 1;

                    // Ghế A (Window)
                    newSeats.Add(new Seat
                    {
                        CarriageId = carriage.CarriageId,
                        SeatNumber = $"{row}A",
                        SeatType = "Window",
                        SeatClass = seatClass,
                        IsActive = true
                    });

                    // Ghế B (Aisle)
                    newSeats.Add(new Seat
                    {
                        CarriageId = carriage.CarriageId,
                        SeatNumber = $"{row}B",
                        SeatType = "Aisle",
                        SeatClass = seatClass,
                        IsActive = true
                    });
                }

                // Xử lý ghế lẻ nếu có
                if (carriageTotalSeats % 2 != 0)
                {
                    currentSeatCount++;
                    int row = (currentSeatCount - 1) / 2 + 1;

                    newSeats.Add(new Seat
                    {
                        CarriageId = carriage.CarriageId,
                        SeatNumber = $"{row}C",
                        SeatType = "Window",
                        SeatClass = seatClass,
                        IsActive = true
                    });
                }
            }
        }

        // Thêm ghế mới vào database
        await context.Seat.AddRangeAsync(newSeats);
        await context.SaveChangesAsync();
    }
}
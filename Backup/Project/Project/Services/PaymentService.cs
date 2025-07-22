using Project.Models;

namespace Project.Services
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(int bookingId, string paymentMethod);
    }

    public class PaymentService : IPaymentService
    {
        private readonly FastRailDbContext _context;

        public PaymentService(FastRailDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ProcessPaymentAsync(int bookingId, string paymentMethod)
        {
            // Mock payment - luôn thành công
            await Task.Delay(1000); // Giả lập xử lý 1 giây

            // Lưu thông tin thanh toán
            var payment = new Payment
            {
                BookingId = bookingId,
                PaymentMethod = paymentMethod,
                Amount = 100000, // Số tiền mẫu
                Currency = "VND",
                TransactionId = $"TXN{DateTime.Now:yyyyMMddHHmmss}",
                Status = "Completed",
                PaymentTime = DateTime.Now,
                ConfirmedTime = DateTime.Now
            };

            _context.Payment.Add(payment);
            await _context.SaveChangesAsync();

            return true; // Luôn thành công cho demo
        }
    }
}

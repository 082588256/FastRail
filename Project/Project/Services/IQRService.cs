namespace Project.Services
{
    public interface IQRService
    {
        Task<string> GenerateQRCodeAsync(string bookingCode);
        Task<byte[]> GenerateQRCodeImageAsync(string bookingCode);
        Task<string> DecodeQRCodeAsync(string qrCodeData);
        Task<string> DecodeQRCodeFromImageAsync(byte[] imageData);
    }
} 
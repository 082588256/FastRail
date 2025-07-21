namespace Project.Services
{
    public interface IQRService
    {
        Task<string> GenerateQRCodeAsync(string ticketCode);
        Task<byte[]> GenerateQRCodeImageAsync(string ticketCode);
        Task<string> DecodeQRCodeAsync(string qrCodeData);
        Task<string> DecodeQRCodeFromImageAsync(byte[] imageData);
    }
} 
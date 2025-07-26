using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace Project.Services
{
    public class QRService : IQRService
    {
        private readonly ILogger<QRService> _logger;

        public QRService(ILogger<QRService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GenerateQRCodeAsync(string bookingCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookingCode))
                {
                    throw new ArgumentException("Booking code cannot be null or empty");
                }

                // Create QR code data with additional security features
                var qrCodeData = new
                {
                    bookingCode = bookingCode,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    type = "train_booking",
                    version = "1.0",
                    checksum = GenerateChecksum(bookingCode)
                };

                var jsonData = System.Text.Json.JsonSerializer.Serialize(qrCodeData);
                return jsonData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for booking {BookingCode}", bookingCode);
                throw;
            }
        }

        public async Task<byte[]> GenerateQRCodeImageAsync(string bookingCode)
        {
            try
            {
                var qrData = await GenerateQRCodeAsync(bookingCode);
                
                // For production, you would use a proper QR code generation library
                // For now, we'll return a placeholder image or use a different approach
                // This is a temporary solution until we resolve the QRCoder compatibility issues
                throw new NotImplementedException("QR code image generation is temporarily disabled. Please use text-based QR codes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image for booking {BookingCode}", bookingCode);
                throw;
            }
        }

        public async Task<string> DecodeQRCodeAsync(string qrCodeData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qrCodeData))
                {
                    throw new ArgumentException("QR code data cannot be null or empty");
                }

                // Try to parse as JSON first (our format)
                try
                {
                    var bookingData = JsonSerializer.Deserialize<JsonElement>(qrCodeData);
                    
                    // Check for bookingCode first (new format)
                    if (bookingData.TryGetProperty("bookingCode", out var bookingCodeElement))
                    {
                        var bookingCode = bookingCodeElement.GetString();
                        
                        // Validate checksum if present
                        if (bookingData.TryGetProperty("checksum", out var checksumElement))
                        {
                            var expectedChecksum = checksumElement.GetString();
                            var actualChecksum = GenerateChecksum(bookingCode);
                            
                            if (expectedChecksum != actualChecksum)
                            {
                                throw new InvalidOperationException("QR code checksum validation failed");
                            }
                        }
                        
                        return bookingCode;
                    }
                    
                    // Fallback for old ticketCode format
                    if (bookingData.TryGetProperty("ticketCode", out var ticketCodeElement))
                    {
                        var ticketCode = ticketCodeElement.GetString();
                        
                        // Validate checksum if present
                        if (bookingData.TryGetProperty("checksum", out var checksumElement))
                        {
                            var expectedChecksum = checksumElement.GetString();
                            var actualChecksum = GenerateChecksum(ticketCode);
                            
                            if (expectedChecksum != actualChecksum)
                            {
                                throw new InvalidOperationException("QR code checksum validation failed");
                            }
                        }
                        
                        return ticketCode;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // If not JSON, treat as plain booking code
                    return qrCodeData;
                }

                throw new InvalidOperationException("Invalid QR code data format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding QR code data");
                throw;
            }
        }

        /// <summary>
        /// Decodes a QR code from an image byte array using ZXing.Net.
        /// Supports common image formats (JPG, PNG, BMP).
        /// Returns the decoded ticket code if successful, or throws a descriptive exception.
        /// </summary>
        /// <param name="imageData">The image data as a byte array.</param>
        /// <returns>The decoded ticket code as a string.</returns>
        public async Task<string> DecodeQRCodeFromImageAsync(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data cannot be null or empty", nameof(imageData));

            try
            {
                using var ms = new MemoryStream(imageData);
                using var bitmap = new System.Drawing.Bitmap(ms);

                var reader = new BarcodeReader
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                        TryHarder = true
                    }
                };

                var result = reader.Decode(bitmap);
                if (result == null || string.IsNullOrWhiteSpace(result.Text))
                {
                    _logger.LogWarning("No QR code found in the provided image.");
                    return string.Empty;
                }

                // Optionally, validate/parse the QR code content as in DecodeQRCodeAsync
                var ticketCode = await DecodeQRCodeAsync(result.Text);
                return ticketCode;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid image data provided for QR decoding.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding QR code from image");
                throw new InvalidOperationException("Failed to decode QR code from image. Please ensure the image is clear and contains a valid QR code.", ex);
            }
        }

        private string GenerateChecksum(string ticketCode)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ticketCode + "FASTRAIL_SECRET"));
            return Convert.ToBase64String(hashBytes).Substring(0, 8);
        }
    }
} 
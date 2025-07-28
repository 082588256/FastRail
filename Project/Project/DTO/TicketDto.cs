namespace Project.DTO
{
    public class TicketDto
    {
        public int TicketId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty; // Thêm thuộc tính này
        public decimal FinalPrice { get; set; }
        public DateTime PurchaseTime { get; set; } = DateTime.UtcNow;
        public int TripId { get; set; }
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    "Valid" => "Chưa bán",
                    "Sold" => "Đã bán",
                    "Cancelled" => "Đã huỷ",
                    _ => "Không rõ"
                };
            }
        }
    }
}
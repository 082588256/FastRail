// DTOs/TicketDto.cs
namespace Project.DTO
{
    public class TicketDto
    {
        public int TicketId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TripId { get; set; }
    }
}

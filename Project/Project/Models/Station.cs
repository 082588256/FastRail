using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Station
    {
        [Key]
        public int StationId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
    }

}

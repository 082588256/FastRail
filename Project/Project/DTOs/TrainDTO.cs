using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.DTOs
{
    public class TrainDTO
    {
        [Key]
        public int TrainId { get; set; }
        public string Name { get; set; }
        public int? TrainType { get; set; }
        public int NumberOfCarriages { get; set; }
        public bool? Status { get; set; }
        
    }
}

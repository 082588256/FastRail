using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Train
    {
        [Key]
        public int TrainId { get; set; }
        public string Name { get; set; }
        public int? TrainType { get; set; }
        public int NumberOfCarriages { get; set; }
        public bool? Status { get; set; }

        public ICollection<Carriage>? Carriages { get; set; }=new List<Carriage>();
    }

}

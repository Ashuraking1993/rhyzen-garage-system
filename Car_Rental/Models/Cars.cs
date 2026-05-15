using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Rental.Models
{
    [Table("Cars")]
    public class Car
    {
        [Key]
        public int CarId { get; set; }

        public string? PlateNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public decimal? DailyRate { get; set; }

        public string? Status { get; set; }

        public string? TransmissionType { get; set; }

        public ICollection<CarLocation>? CarLocations { get; set; }

        public string? Features { get; set; }
        public string? InteriorDetails { get; set; }

        public ICollection<CarImage> CarImages { get; set; }
    }
}
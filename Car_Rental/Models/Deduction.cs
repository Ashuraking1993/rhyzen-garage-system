using System.ComponentModel.DataAnnotations;

namespace Car_Rental.Models
{
    public class Deduction
    {
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Optional reference to booking
        public int? BookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
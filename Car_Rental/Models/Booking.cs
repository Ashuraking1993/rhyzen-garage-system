namespace Car_Rental.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public int? TotalDays { get; set; }
        public decimal? TotalAmount { get; set; }

        public string? BookingStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public Car? Car { get; set; }
        public int CarId { get; set; }   // 
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        //public string BookingCode { get; set; }

        public string? BookingCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public const string Confirmed = "Confirmed";
        public const string Pending = "Pending";

    }
}
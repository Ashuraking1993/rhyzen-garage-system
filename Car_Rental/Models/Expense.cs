using System.ComponentModel.DataAnnotations;

namespace Car_Rental.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Optional category (Fuel, Maintenance, Salary, etc.)
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
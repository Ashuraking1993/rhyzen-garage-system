using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Car_Rental.Models;

namespace Car_Rental.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Booking> Bookings { get; set; }   // 🔥 ADD THIS
        public DbSet<Car> Cars { get; set; }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Deduction> Deductions { get; set; }

        public DbSet<CarLocation> CarLocations { get; set; }
        public DbSet<CarImage> CarImages { get; set; }


       
    }



}
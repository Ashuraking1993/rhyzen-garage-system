using Car_Rental.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Car_Rental.Models.ViewModels;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalBookings = await _context.Bookings.CountAsync();

            var activeRentals = await _context.Bookings
                .CountAsync(b => b.BookingStatus == "Confirmed");

            // FIXED Revenue (only Confirmed + Paid)
            var totalRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == "Paid"
                         && b.BookingStatus == "Confirmed")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var pendingPayments = await _context.Bookings
                .CountAsync(b => b.PaymentStatus == "Unpaid");

            //  NEW Monthly Revenue
            var monthlyRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == "Paid"
                         && b.BookingStatus == "Confirmed"
                         && b.CreatedAt.Month == DateTime.UtcNow.Month
                         && b.CreatedAt.Year == DateTime.UtcNow.Year)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var model = new AdminDashboardViewModel
            {
                TotalBookings = totalBookings,
                ActiveRentals = activeRentals,
                TotalRevenue = totalRevenue,
                PendingPayments = pendingPayments,
                MonthlyRevenue = monthlyRevenue
            };

            return View(model);
        }
    }
}
using Car_Rental.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOccupancyHeatmap()
        {
            var totalCars = await _context.Cars.CountAsync();

            var bookings = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed")
                .GroupBy(b => b.StartDate.Date)
                .Select(g => new
                {
                    date = g.Key,
                    bookedCount = g.Count()
                })
                .ToListAsync();

            var result = bookings.Select(b => new
            {
                date = b.date.ToString("yyyy-MM-dd"),
                occupancy = totalCars == 0 ? 0 :
                    (double)b.bookedCount / totalCars * 100,
                totalBookings = b.bookedCount
            });

            return Json(result);
        }
    }
}
using Car_Rental.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingSearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingSearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string bookingCode)
        {
            if (string.IsNullOrEmpty(bookingCode))
                return View("Index");

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);

            return View("Index", booking);
        }
    }
}
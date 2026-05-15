using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Car_Rental.Data;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CarsLocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsLocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.CarLocations)
                .ToListAsync();

            return View(cars);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveLocations()
        {
            var cars = await _context.Cars
                .Include(c => c.CarLocations)
                .ToListAsync();

            var result = cars.Select(car =>
            {
                var latest = car.CarLocations
                    .OrderByDescending(l => l.UpdatedAt)
                    .FirstOrDefault();

                return new
                {
                    plate = car.PlateNumber,
                    brand = car.Brand,
                    model = car.Model,
                    status = car.Status,
                    driver = latest?.DriverName,
                    lat = latest?.Latitude,
                    lng = latest?.Longitude,
                    updatedAt = latest?.UpdatedAt
                };
            });

            return Json(result);
        }
    }
}
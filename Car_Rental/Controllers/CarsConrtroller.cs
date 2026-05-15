using Car_Rental.Data;
using Car_Rental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // LIST (Garage Page)
        // ===========================
        public IActionResult Cars()
        {
            var today = DateTime.Today;

            var cars = _context.Cars.ToList();

            // 🔥 Auto mark completed bookings
            var expiredBookings = _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed" && b.EndDate < today)
                .ToList();

            foreach (var booking in expiredBookings)
            {
                booking.BookingStatus = "Completed";
            }

            //  Update car availability dynamically
            foreach (var car in cars)
            {
                bool isCurrentlyBooked = _context.Bookings.Any(b =>
                    b.CarId == car.CarId &&
                    b.BookingStatus == "Confirmed" &&
                    b.StartDate <= today &&
                    b.EndDate >= today);

                bool hasFutureBooking = _context.Bookings.Any(b =>
                    b.CarId == car.CarId &&
                    b.BookingStatus == "Confirmed" &&
                    b.StartDate > today);

                if (isCurrentlyBooked)
                    car.Status = "Booked";
                else if (hasFutureBooking)
                    car.Status = "Reserved";
                else
                    car.Status = "Available";
            }

            _context.SaveChanges();

            return View(cars);
        }

        // ===========================
        // CREATE
        // ===========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Add(car);
                _context.SaveChanges();
                return RedirectToAction("Cars"); //  FIXED
            }

            return View(car);
        }

        // ===========================
        // EDIT
        // ===========================
        public IActionResult Edit(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);

            if (car == null)
                return NotFound();

            return View(car);
        }

        [HttpPost]
        public IActionResult Edit(Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Cars.Update(car);
                _context.SaveChanges();
                return RedirectToAction("Cars"); //  FIXED
            }

            return View(car);
        }

        // ===========================
        // DELETE
        // ===========================
        public IActionResult Delete(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);

            if (car == null)
                return NotFound();

            _context.Cars.Remove(car);
            _context.SaveChanges();

            return RedirectToAction("Cars"); //  FIXED
        }
    }
}
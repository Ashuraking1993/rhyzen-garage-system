using Car_Rental.Data;
using Car_Rental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Car_Rental.Services;
using System.Linq;

[Authorize]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PayMongoService _payMongo;
    private readonly EmailService _emailService;

    public BookingController(ApplicationDbContext context ,PayMongoService payMongo, EmailService emailService)

    {
        _context = context;
        _payMongo = payMongo;
        _emailService = emailService;
    }

    // 🔹 Load Booking Page
     public async Task<IActionResult> Create(int carId)
    {

        var connection = _context.Database.GetDbConnection();
        Console.WriteLine("SERVER: " + connection.DataSource);
        Console.WriteLine("DATABASE: " + connection.Database);
        var car = await _context.Cars
            .Include(c => c.CarImages)   // 
            .FirstOrDefaultAsync(c => c.CarId == carId);

        if (car == null)
            return NotFound();


        ViewBag.Car = car;

        
        return View(new Booking { CarId = carId });
    }
    public IActionResult Cancel(int id)
    {
        var booking = _context.Bookings.FirstOrDefault(b => b.Id == id);

        if (booking == null)
            return NotFound();

        booking.BookingStatus = "Cancelled";

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // 🔹 Save Booking
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        var car = await _context.Cars
            .FirstOrDefaultAsync(c => c.CarId == booking.CarId);

        if (car == null)
            return NotFound();

        // Prevent overlapping CONFIRMED bookings only
        var overlapping = await _context.Bookings.AnyAsync(b =>
            b.CarId == booking.CarId &&
            b.BookingStatus == "Confirmed" &&
            booking.StartDate < b.EndDate &&
            booking.EndDate > b.StartDate);

        if (overlapping)
        {
            ModelState.AddModelError("", "This car is already booked for selected dates.");
            ViewBag.Car = car;
            return View(booking);
        }

        booking.TotalDays = (booking.EndDate - booking.StartDate).Days;

        if (booking.TotalDays <= 0)
        {
            ModelState.AddModelError("", "Invalid booking dates.");
            ViewBag.Car = car;
            return View(booking);
        }

        //  Compute total
        booking.TotalAmount = booking.TotalDays * (car.DailyRate ?? 0);

        // Payment rule
        booking.BookingStatus = "Pending";
        booking.PaymentStatus = "Unpaid";
        booking.CreatedAt = DateTime.UtcNow;

        booking.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //  KEEP customer details from form (no reassignment needed)
        // booking.CustomerName
        // booking.CustomerEmail
        // booking.CustomerPhone

        //  Reserve car temporarily
        car.Status = "Reserved";


        booking.BookingCode = await GenerateBookingCodeAsync();
        _context.Bookings.Add(booking);
       
        await _context.SaveChangesAsync();

        return RedirectToAction("Payment", new { id = booking.Id });
    }

    public async Task<IActionResult> Payment(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Car)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound();

        // Create PayMongo checkout link
        var checkoutUrl = await _payMongo.CreateGCashPayment(
            booking.TotalAmount ?? 0,
            $"Car Rental - {booking.Car.Brand} {booking.Car.Model}"
        );

        return Redirect(checkoutUrl);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Car)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound();

        booking.PaymentStatus = "Paid";
        booking.BookingStatus = "Confirmed";

        if (booking.Car != null)
            booking.Car.Status = "Rented";

        await _context.SaveChangesAsync();
        await _emailService.SendReceiptAsync(booking);

        return RedirectToAction("Dashboard");
    }


    [Authorize]
    public async Task<IActionResult> Success()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var booking = await _context.Bookings
            .Include(b => b.Car)          // 
            //.Include(b => b.User)         //  
            .Where(b => b.UserId == userId && b.PaymentStatus == "Unpaid")
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (booking != null)
        {
            booking.PaymentStatus = "Paid";
            booking.BookingStatus = "Confirmed";

            await _context.SaveChangesAsync();
            await _emailService.SendReceiptAsync(booking);
        }

        return View();
    }

    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var bookings = await _context.Bookings
            .Include(b => b.Car)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    private async Task<string> GenerateBookingCodeAsync()
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();

        string code;
        bool exists;

        do
        {
            var letterPart = new string(Enumerable.Range(0, 3)
                .Select(_ => letters[random.Next(letters.Length)]).ToArray());

            var numberPart = random.Next(1000, 9999);

            code = $"{letterPart}{numberPart}";

            exists = await _context.Bookings
                .AnyAsync(b => b.BookingCode == code);

        } while (exists);

        return code;
    }

}
using Car_Rental.Data;
using Microsoft.EntityFrameworkCore;

public class AutoCancelService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AutoCancelService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider
                                   .GetRequiredService<ApplicationDbContext>();

                var expiredBookings = await context.Bookings
                    .Where(b =>
                        b.BookingStatus == "Pending" &&
                        b.PaymentStatus == "Unpaid" &&
                        b.CreatedAt.AddMinutes(20) <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var booking in expiredBookings)
                {
                    booking.BookingStatus = "Cancelled";
                    booking.PaymentStatus = "Expired";

                    var car = await context.Cars.FindAsync(booking.CarId);
                    if (car != null)
                        car.Status = "Available";
                }

                await context.SaveChangesAsync();
            }

            // Check every 1 minute (for testing)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
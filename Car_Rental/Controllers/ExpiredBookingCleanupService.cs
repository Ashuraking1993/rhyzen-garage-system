using Car_Rental.Data;
using Microsoft.EntityFrameworkCore;

public class ExpiredBookingCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExpiredBookingCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var threshold = DateTime.UtcNow.AddDays(-2); // delete after 7 days

            var expiredBookings = await context.Bookings
                .Where(b => b.PaymentStatus == "Expired"
                         && b.CreatedAt < threshold)
                .ToListAsync(stoppingToken);

            if (expiredBookings.Any())
            {
                context.Bookings.RemoveRange(expiredBookings);
                await context.SaveChangesAsync(stoppingToken);
            }

            // Run every 6 hours
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
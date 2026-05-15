namespace Car_Rental.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int ActiveRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingPayments { get; set; }
        public decimal MonthlyRevenue { get; set; }

        public List<MonthlyRevenueDto> MonthlyRevenueData { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenueData { get; set; } = new();

    }

    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Deductions { get; set; }
    }

    public class DailyRevenueDto
    {
        public int Day { get; set; }
        public decimal Total { get; set; }
    }
}
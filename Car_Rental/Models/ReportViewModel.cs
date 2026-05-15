namespace Car_Rental.Models
{
    public class ReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal Revenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetProfit { get; set; }

        public List<Expense> Expenses { get; set; }
        public List<Deduction> Deductions { get; set; }
    }
}

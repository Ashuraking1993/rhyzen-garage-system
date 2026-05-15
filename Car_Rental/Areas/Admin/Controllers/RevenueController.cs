using Car_Rental.Data;
using Car_Rental.Models;
using Car_Rental.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RevenueController : Controller
{
    private readonly ApplicationDbContext _context;

    public RevenueController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var selectedYear = year ?? DateTime.Now.Year;

        var model = new AdminDashboardViewModel();

        // 1️⃣ Get confirmed bookings for the year
        var bookings = await _context.Bookings
            .Where(b => b.BookingStatus == "Confirmed"
                        && b.StartDate.Year == selectedYear)
            .ToListAsync();

        // 2️⃣ Get all expenses for the year
        var expenses = await _context.Expenses
            .Where(e => e.Date.Year == selectedYear)
            .ToListAsync();

        // 3️⃣ Get all deductions for the year
        var deductions = await _context.Deductions
            .Where(d => d.Date.Year == selectedYear)
            .ToListAsync();

        // 4️⃣ Build monthly summary manually (SAFE & RELIABLE)
        var monthlyData = bookings
            .GroupBy(b => b.StartDate.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Month = g.Key,

                Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0,

                Expenses = expenses
                    .Where(e => e.Date.Month == g.Key)
                    .Sum(e => e.Amount),

                Deductions = deductions
                    .Where(d => d.Date.Month == g.Key)
                    .Sum(d => d.Amount)
            })
            .OrderBy(x => x.Month)
            .ToList();

        model.MonthlyRevenueData = monthlyData;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetDailySummary(DateTime date)
    {
        var selectedDate = date.Date;

        // =========================
        // TOTAL SALES
        // =========================
        var totalSales = await _context.Bookings
            .Where(b =>
                b.BookingStatus == "Confirmed" &&
                b.StartDate.Date == selectedDate)
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

        // =========================
        // TOTAL EXPENSES
        // =========================
        var totalExpenses = await _context.Expenses
            .Where(e => e.Date.Date == selectedDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // =========================
        // TOTAL DEDUCTIONS
        // =========================
        var totalDeductions = await _context.Deductions
            .Where(d => d.Date.Date == selectedDate)
            .SumAsync(d => (decimal?)d.Amount) ?? 0;

        return Json(new
        {
            totalSales,
            totalExpenses,
            totalDeductions,
            netProfit = totalSales - totalExpenses - totalDeductions
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddExpense(string description, decimal amount, DateTime? expenseDate)
    {
        if (string.IsNullOrWhiteSpace(description) || amount <= 0)
            return Json(new { success = false, message = "Invalid expense data." });

        var date = expenseDate ?? DateTime.Today;

        var expense = new Expense
        {
            Description = description.Trim(),
            Amount = amount,
            Date = date.Date,
            CreatedAt = DateTime.Now
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> AddDeduction(string reason, decimal amount, DateTime? deductionDate)
    {
        if (string.IsNullOrWhiteSpace(reason) || amount <= 0)
            return Json(new { success = false, message = "Invalid deduction data." });

        var date = deductionDate ?? DateTime.Today;

        var deduction = new Deduction
        {
            Reason = reason.Trim(),
            Amount = amount,
            Date = date.Date,
            CreatedAt = DateTime.Now
        };

        _context.Deductions.Add(deduction);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
}
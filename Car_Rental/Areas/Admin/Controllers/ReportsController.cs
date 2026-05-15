using Car_Rental.Data;
using Car_Rental.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace Car_Rental.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

      

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            var model = new AdminDashboardViewModel();

            var monthlyData = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed"
                            && b.StartDate.Year == selectedYear)
                .GroupBy(b => b.StartDate.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0,
                    Expenses = _context.Expenses
                        .Where(e => e.Date.Year == selectedYear
                                 && e.Date.Month == g.Key)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    Deductions = _context.Deductions
                        .Where(d => d.Date.Year == selectedYear
                                 && d.Date.Month == g.Key)
                        .Sum(d => (decimal?)d.Amount) ?? 0
                })
                .ToListAsync();

            model.MonthlyRevenueData = monthlyData;

            return View(model);
        }

        // ==========================
     
        private byte[] GenerateMonthlyChart(List<MonthlyRevenueDto> data)
        {
            using var bitmap = new SKBitmap(800, 400);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            var paint = new SKPaint
            {
                Color = SKColors.ForestGreen,
                StrokeWidth = 40,
                IsAntialias = true
            };

            float startX = 100;
            float baseY = 350;
            float spacing = 120;

            var maxValue = data.Max(x => x.Revenue);

            for (int i = 0; i < data.Count; i++)
            {
                float height = maxValue == 0
                    ? 0
                    : (float)(data[i].Revenue / maxValue * 250);

                canvas.DrawLine(
                    startX + i * spacing,
                    baseY,
                    startX + i * spacing,
                    baseY - height,
                    paint
                );
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var stream = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);

            return stream.ToArray();
        }

        private byte[] GenerateMonthlyComparisonChart(decimal currentNet, decimal previousNet)
        {
            int width = 700;
            int height = 350;

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            float baseY = 280f;

            decimal maxDecimal = Math.Max(Math.Abs(currentNet), Math.Abs(previousNet));
            if (maxDecimal < 1)
                maxDecimal = 1;

            // Convert to float AFTER decimal math
            float max = (float)maxDecimal;

            float currentValue = (float)Math.Abs(currentNet);
            float previousValue = (float)Math.Abs(previousNet);

            float currentHeight = (currentValue / max) * 200f;
            float previousHeight = (previousValue / max) * 200f;

            var paintCurrent = new SKPaint
            {
                Color = currentNet >= 0 ? SKColors.ForestGreen : SKColors.IndianRed,
                StrokeWidth = 60,
                IsAntialias = true
            };

            var paintPrevious = new SKPaint
            {
                Color = previousNet >= 0 ? SKColors.SteelBlue : SKColors.OrangeRed,
                StrokeWidth = 60,
                IsAntialias = true
            };

            // Draw bars
            canvas.DrawLine(250, baseY, 250, baseY - currentHeight, paintCurrent);
            canvas.DrawLine(450, baseY, 450, baseY - previousHeight, paintPrevious);

            using var image = SKImage.FromBitmap(bitmap);
            using var stream = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);

            return stream.ToArray();
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int year)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var monthlyData = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed"
                            && b.StartDate.Year == year)
                .GroupBy(b => b.StartDate.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0,
                    Expenses = _context.Expenses
                        .Where(e => e.Date.Year == year && e.Date.Month == g.Key)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    Deductions = _context.Deductions
                        .Where(d => d.Date.Year == year && d.Date.Month == g.Key)
                        .Sum(d => (decimal?)d.Amount) ?? 0
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            var chartImage = GenerateMonthlyChart(monthlyData);

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"Car Rental Yearly Report - {year}")
                        .FontSize(24)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        //  Insert Chart
                        col.Item().Image(chartImage);

                        col.Item().LineHorizontal(1);

                        foreach (var m in monthlyData)
                        {
                            var net = m.Revenue - m.Expenses - m.Deductions;

                            col.Item().Text(
                                $"Month {m.Month} | Revenue: ₱ {m.Revenue:N2} | " +
                                $"Expenses: ₱ {m.Expenses:N2} | " +
                                $"Deductions: ₱ {m.Deductions:N2} | " +
                                $"Net: ₱ {net:N2}"
                            );
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"FinancialReport_{year}.pdf");
        }


        [HttpGet]
        public async Task<IActionResult> ExportDailyPdf(DateTime date)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var revenue = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed"
                            && b.StartDate.Date == date.Date)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var expenses = await _context.Expenses
                .Where(e => e.Date.Date == date.Date)
                .ToListAsync();

            var deductions = await _context.Deductions
                .Where(d => d.Date.Date == date.Date)
                .ToListAsync();

            var totalExpenses = expenses.Sum(e => e.Amount);
            var totalDeductions = deductions.Sum(d => d.Amount);
            var net = revenue - totalExpenses - totalDeductions;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"Car Rental Daily Report - {date:yyyy-MM-dd}")
                        .FontSize(20)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text($"Total Revenue: ₱ {revenue:N2}");
                        col.Item().Text($"Total Expenses: ₱ {totalExpenses:N2}");
                        col.Item().Text($"Total Deductions: ₱ {totalDeductions:N2}");
                        col.Item().Text($"Net Profit: ₱ {net:N2}")
                                  .Bold()
                                  .FontColor(net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Expenses").Bold();
                        foreach (var e in expenses)
                            col.Item().Text($"{e.Description} | ₱ {e.Amount:N2}");

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Deductions").Bold();
                        foreach (var d in deductions)
                            col.Item().Text($"{d.Reason} | ₱ {d.Amount:N2}");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"DailyReport_{date:yyyyMMdd}.pdf");
        }



        [HttpGet]
        public async Task<IActionResult> ExportMonthlyPdf(int year, int month)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var currentRevenue = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed"
                            && b.StartDate.Year == year
                            && b.StartDate.Month == month)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var currentExpenses = await _context.Expenses
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var currentDeductions = await _context.Deductions
                .Where(d => d.Date.Year == year && d.Date.Month == month)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var currentNet = currentRevenue - currentExpenses - currentDeductions;

            //  Previous Month Logic
            var prevDate = new DateTime(year, month, 1).AddMonths(-1);
            var prevRevenue = await _context.Bookings
                .Where(b => b.BookingStatus == "Confirmed"
                            && b.StartDate.Year == prevDate.Year
                            && b.StartDate.Month == prevDate.Month)
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

            var prevExpenses = await _context.Expenses
                .Where(e => e.Date.Year == prevDate.Year
                         && e.Date.Month == prevDate.Month)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var prevDeductions = await _context.Deductions
                .Where(d => d.Date.Year == prevDate.Year
                         && d.Date.Month == prevDate.Month)
                .SumAsync(d => (decimal?)d.Amount) ?? 0;

            var prevNet = prevRevenue - prevExpenses - prevDeductions;

            var trend = currentNet - prevNet;
           

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"Car Rental Monthly Report - {month}/{year}")
                        .FontSize(22)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        var chartImage = GenerateMonthlyComparisonChart(currentNet, prevNet);

                        col.Item().Height(250).Image(chartImage);

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Total Revenue: ₱ {currentRevenue:N2}");
                        col.Item().Text($"Total Expenses: ₱ {currentExpenses:N2}");
                        col.Item().Text($"Total Deductions: ₱ {currentDeductions:N2}");
                        col.Item().Text($"Net Profit: ₱ {currentNet:N2}")
                            .Bold()
                            .FontColor(currentNet >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);

                        col.Item().LineHorizontal(1);

                        col.Item().Text("📈 Profit Trend Comparison").Bold();

                        col.Item().Text($"Previous Month Net: ₱ {prevNet:N2}");
                        col.Item().Text($"Change: ₱ {trend:N2}")
                            .FontColor(trend >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"MonthlyReport_{month}_{year}.pdf");
        }
    }
}
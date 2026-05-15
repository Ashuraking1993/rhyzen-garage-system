using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Car_Rental.Models;

public class PdfService
{
    public byte[] GenerateReceiptPdf(Booking booking)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // ✅ Auto Invoice Number
        var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{booking.Id}";

        // ✅ Generate QR Code
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(
            $"Invoice: {invoiceNumber}\nBooking: {booking.Id}\nCustomer: {booking.CustomerName}\nAmount: ₱{booking.TotalAmount:N2}",
            QRCoder.QRCodeGenerator.ECCLevel.Q);

        var qrCode = new QRCoder.PngByteQRCode(qrData);
        byte[] qrBytes = qrCode.GetGraphic(20);

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/ryzen-logo.png");
        byte[] logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Content().Column(col =>
                {
                    col.Spacing(15);

                    // ✅ Logo
                    if (logoBytes != null)
                    {
                      col.Item()
                   .AlignCenter()
                   .Height(60)
                   .Image(logoBytes);
                    }

                    col.Item().AlignCenter().Text("RYZEN GARAGE")
                        .FontSize(22)
                        .Bold();

                    col.Item().AlignCenter().Text("OFFICIAL RECEIPT")
                        .FontSize(14);

                    col.Item().LineHorizontal(1);

                    col.Item().Text($"Invoice No: {invoiceNumber}").Bold();
                    col.Item().Text($"Booking Code: {booking.BookingCode}").Bold().FontSize(14);
                    col.Item().Text($"Customer: {booking.CustomerName}");
                    col.Item().Text($"Vehicle: {booking.Car?.Brand} {booking.Car?.Model}");
                    col.Item().Text($"Start Date: {booking.StartDate:MMM dd, yyyy}");
                    col.Item().Text($"End Date: {booking.EndDate:MMM dd, yyyy}");
                    col.Item().Text($"Total Days: {booking.TotalDays}");
                    col.Item().Text($"Total Amount: ₱{booking.TotalAmount:N2}");

                    col.Item().LineHorizontal(1);
                    col.Item().Text("Status: PAID").Bold();

                    col.Item().AlignCenter().Text("Scan to verify invoice");

                    // ✅ QR Code
                    col.Item()
                   .AlignCenter()
                   .Height(120)
                   .Image(qrBytes);
                });
            });
        });

        return document.GeneratePdf();
    }
}
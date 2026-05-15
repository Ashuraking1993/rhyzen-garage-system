using Car_Rental.Models;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using System.Net;
using System.Net.Mail;

namespace Car_Rental.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly PdfService _pdfService;

        public EmailService(IConfiguration config, PdfService pdfService)
        {
            _config = config;
            _pdfService = pdfService;
        }

        public async Task SendReceiptAsync(Booking booking)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_config["EmailSettings:SenderEmail"]);
            message.To.Add(booking.CustomerEmail);
            message.Subject = $"Official Receipt - Booking #{booking.BookingCode}";
            message.IsBodyHtml = true;

            message.Body = $@"
                <h2>RYZEN GARAGE - OFFICIAL RECEIPT</h2>
                <hr/>
                <p><strong>Booking ID:</strong> {booking.BookingCode}</p>
                <p><strong>Customer:</strong> {booking.CustomerName}</p>
                <p><strong>Vehicle:</strong> {booking.Car.Brand} {booking.Car.Model}</p>
                <p><strong>Start Date:</strong> {booking.StartDate:MMM dd, yyyy}</p>
                <p><strong>End Date:</strong> {booking.EndDate:MMM dd, yyyy}</p>
                <p><strong>Total Days:</strong> {booking.TotalDays}</p>
                <p><strong>Total Amount:</strong> ₱{booking.TotalAmount:N2}</p>
                <hr/>
                <p>Status: <strong>PAID</strong></p>
                <br/>
                <p>Thank you for choosing Ryzen Garage 🚗</p>
            ";
            // ✅ Generate PDF
            var pdfBytes = _pdfService.GenerateReceiptPdf(booking);

            var stream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(stream, $"Receipt_{booking.BookingCode}.pdf", "application/pdf");
            message.Attachments.Add(attachment);

            var smtp = new SmtpClient(
                _config["EmailSettings:SmtpServer"],
                int.Parse(_config["EmailSettings:Port"])
            )
            {
                Credentials = new NetworkCredential(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:SenderPassword"]
                ),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }

        private byte[] GenerateReceiptPdf(Booking booking)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text("RYZEN GARAGE - OFFICIAL RECEIPT")
                            .FontSize(20)
                            .Bold();

                        col.Item().LineHorizontal(1);

                        col.Item().Text($"Booking ID: {booking.Id}");
                        col.Item().Text($"Customer: {booking.CustomerName}");
                        col.Item().Text($"Vehicle: {booking.Car?.Brand} {booking.Car?.Model}");
                        col.Item().Text($"Start Date: {booking.StartDate:MMM dd, yyyy}");
                        col.Item().Text($"End Date: {booking.EndDate:MMM dd, yyyy}");
                        col.Item().Text($"Total Days: {booking.TotalDays}");
                        col.Item().Text($"Total Amount: ₱{booking.TotalAmount:N2}");

                        col.Item().LineHorizontal(1);
                        col.Item().Text("Status: PAID").Bold();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
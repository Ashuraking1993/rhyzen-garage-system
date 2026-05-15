using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Car_Rental.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Do nothing (no real email sending)
            return Task.CompletedTask;
        }
    }
}
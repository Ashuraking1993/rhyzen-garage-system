using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Car_Rental.Services
{
    public class PayMongoService
    {
        private readonly HttpClient _http;

        public PayMongoService(HttpClient http)
        {
            _http = http;
        }
        public async Task<string> CreateGCashPayment(decimal amount, string description)
        {
            var secretKey = "sk_test_HVBiEeNqsxPKvCToVEjKUgBC";

            var byteArray = Encoding.ASCII.GetBytes(secretKey + ":");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
            {
                new
                {
                    currency = "PHP",
                    amount = (int)(amount * 100),
                    name = description,
                    quantity = 1
                }
            },
                        payment_method_types = new[] { "gcash" },
                        success_url = "http://localhost:5004/Booking/Success",
                        cancel_url = "http://localhost:5004/Booking/Cancel",
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _http.PostAsync(
            "https://api.paymongo.com/v1/checkout_sessions",
            content);

            var result = await response.Content.ReadAsStringAsync();
            

          

            using var doc = JsonDocument.Parse(result);
            

            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                var checkoutUrl = data
                    .GetProperty("attributes")
                    .GetProperty("checkout_url")
                    .GetString();

                return checkoutUrl;
            }
            else
            {
                throw new Exception("Unexpected PayMongo response: " + result);
            }
        }
    }
}
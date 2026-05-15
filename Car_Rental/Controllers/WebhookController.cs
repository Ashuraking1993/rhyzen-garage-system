using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/webhook")]
[AllowAnonymous]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _config;

    public WebhookController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("paymongo")]
    public async Task<IActionResult> PayMongoWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var signatureHeader = Request.Headers["Paymongo-Signature"].ToString();
        var secret = _config["PayMongo:WebhookSecret"];

        if (!VerifyPayMongoSignature(body, signatureHeader, secret))
        {
            return Unauthorized();
        }

        var json = JsonDocument.Parse(body);

        var eventType = json.RootElement
            .GetProperty("data")
            .GetProperty("attributes")
            .GetProperty("type")
            .GetString();

        if (eventType == "checkout_session.payment.paid")
        {
            Console.WriteLine("PAYMENT SUCCESS CONFIRMED");
        }

        return Ok();
    }

    private bool VerifyPayMongoSignature(string payload, string signatureHeader, string secret)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return false;

        var parts = signatureHeader.Split(',');

        var timestampPart = parts.FirstOrDefault(p => p.StartsWith("t="));
        var signaturePart = parts.FirstOrDefault(p => p.StartsWith("v1="));

        if (timestampPart == null || signaturePart == null)
            return false;

        var timestamp = timestampPart.Substring(2);
        var signature = signaturePart.Substring(3);

        var signedPayload = $"{timestamp}.{payload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

        return computedSignature == signature;
    }
}
using EcommerceStore.Models;
using EcommerceStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailTestController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail(string recipientEmail)
        {
            try
            {
                _logger.LogInformation("📧 Sending test email to {Email}", recipientEmail);

                // Create a dummy order for testing
                var testOrder = new Order
                {
                    Id = 9999,
                    CustomerName = "Test Customer",
                    Email = recipientEmail,
                    Address = "123 Test Street, Test City",
                    Landmark = "Near Test Mall",
                    Phone = "0300-1234567",
                    PaymentMethod = "Cash on Delivery",
                    OrderDate = DateTime.Now,
                    TotalAmount = 5000,
                    Status = "Test Order",
                    TrackingId = "BAZTEST123"
                };

                var testCart = new List<CartItem>
                {
                    new CartItem
                    {
                        ProductId = 1,
                        ProductName = "Test Product",
                        Price = 2500,
                        Quantity = 2,
                        Size = "M"
                    }
                };

                await _emailService.SendOrderConfirmationAsync(testOrder, testCart);

                _logger.LogInformation("✅ Test email sent successfully");
                TempData["Success"] = $"Test email sent successfully to {recipientEmail}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send test email");
                TempData["Error"] = $"Failed to send test email: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
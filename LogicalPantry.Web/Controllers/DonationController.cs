using LogicalPantry.DTOs.PayPalSettingDtos;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Donation")]
    public class DonationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public DonationController(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("PayPal")]
        public IActionResult PayPal()
        {
            _logger.LogInformation($"PayPal method call started");

            _logger.LogInformation($"PayPal method call ended");

            return View();
        }

        [HttpPost]
        [Route("CompletePayment")]
        public async Task<IActionResult> CompletePayment([FromBody] PayPalPaymentDto paymentDto)
        {
            _logger.LogInformation($"CompletePayment method call started");



            if (paymentDto == null || string.IsNullOrEmpty(paymentDto.OrderId) || string.IsNullOrEmpty(paymentDto.PayerId))
            {
                return BadRequest("Invalid payment details");
            }

            // Here you would typically verify and save the payment details
            // For example, save to your database, notify users, etc.

            // Just for demo purposes, let's return OK

            _logger.LogInformation($"CompletePayment method call started");

            return Ok();
        }

        public IActionResult Success()
        {
            _logger.LogInformation($"Success method call started");

            _logger.LogInformation($"Success method call ended");


            return View();
        }
    }

}

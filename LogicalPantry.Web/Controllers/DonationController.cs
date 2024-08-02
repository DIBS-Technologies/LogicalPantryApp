using LogicalPantry.DTOs.PayPalSettingDtos;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Donation")]
    public class DonationController : Controller
    {
            private readonly IConfiguration _configuration;

            public DonationController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

        [HttpGet("PayPal")]
        public IActionResult PayPal()
        {
            return View();
        }

        [HttpPost]
            [Route("CompletePayment")]
            public async Task<IActionResult> CompletePayment([FromBody] PayPalPaymentDto paymentDto)
            {
                if (paymentDto == null || string.IsNullOrEmpty(paymentDto.OrderId) || string.IsNullOrEmpty(paymentDto.PayerId))
                {
                    return BadRequest("Invalid payment details");
                }

                // Here you would typically verify and save the payment details
                // For example, save to your database, notify users, etc.

                // Just for demo purposes, let's return OK
                return Ok();
            }

            public IActionResult Success()
            {
                return View();
            }
    }
    
}

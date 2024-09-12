using LogicalPantry.DTOs.PayPalSettingDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Models.Test.ModelTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    /// <summary>
    /// Controller responsible for handling donation-related actions.
    /// </summary>
    [Route("Donation")]
    public class DonationController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DonationController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="logger">The logger service.</param>
        public DonationController(IConfiguration configuration, ILogger<DonationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Displays the PayPal donation page.
        /// </summary>
        /// <returns>The view for the PayPal donation page.</returns>
        [HttpGet("PayPal")]
        public IActionResult PayPal()
        {
            _logger.LogInformation($"PayPal method call started");

            ViewBag.TenantId = TenantId;
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            ViewBag.PageName = TenantDisplayName;
            _logger.LogInformation($"PayPal method call ended");

            return View();
        }


        /// <summary>
        /// Completes the PayPal payment process.
        /// </summary>
        /// <param name="paymentDto">The PayPal payment details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        [Route("CompletePayment")]
        public async Task<IActionResult> CompletePayment([FromBody] PayPalPaymentDto paymentDto)
        {
            _logger.LogInformation($"CompletePayment method call started");
           if (paymentDto == null || string.IsNullOrEmpty(paymentDto.OrderId) || string.IsNullOrEmpty(paymentDto.PayerId))
            {
                return BadRequest("Invalid payment details");
            }
            _logger.LogInformation($"CompletePayment method call ended");

            return Ok();
        }

        /// <summary>
        /// Displays the success page after a successful payment.
        /// </summary>
        /// <returns>The view for the success page.</returns>
        public IActionResult Success()
        {
            _logger.LogInformation($"Success method call started");
            _logger.LogInformation($"Success method call ended");
            return View();
        }
    }

}

using LogicalPantry.DTOs.PayPalSettingDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Models.Test.ModelTest;
using LogicalPantry.Services.InformationService;
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
        private readonly IInformationService _informationService;
        /// <summary>
        /// Initializes a new instance of the <see cref="DonationController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="logger">The logger service.</param>
        public DonationController(IInformationService informationService, IConfiguration configuration, ILogger<DonationController> logger)
        {
            _informationService = informationService;
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

            if (TenantId.HasValue)
            {
                // Fetch tenant information (including PayPal email) if TenantId is not null
                var response = _informationService.GetTenant(TenantId.Value); // Use .Value to access the int
                if (response != null)
                {
                    // Pass the PayPal email to the view
                    ViewBag.PayPalEmail = response.Result.Data.PaypalId; // Assuming 'PaypalId' holds the PayPal email
                }
                else
                {
                    _logger.LogWarning("Tenant information could not be retrieved.");
                    // Handle the case where tenant information is missing, if needed
                }
            }
            else
            {
                _logger.LogWarning("TenantId is null.");
                // Handle the case where TenantId is null, e.g., redirect or return an error view
                return BadRequest("Tenant ID is required.");
            }
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

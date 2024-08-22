using LogicalPantry.DTOs.PayPalSettingDtos;
using LogicalPantry.Models.Models;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("Donation")]
    public class DonationController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public DonationController(IConfiguration configuration, ILogger<DonationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("PayPal")]
        public IActionResult PayPal()
        {
            _logger.LogInformation($"PayPal method call started");

            var tenantIdString = HttpContext.Session.GetString("TenantId");


            var Logo = HttpContext.Session.GetString("TenantImage");
            if (!int.TryParse(tenantIdString, out int tenantId) || tenantId == 0)
            {
                return BadRequest("Invalid tenant ID");
            }
            var PageName = HttpContext.Session.GetString("PageName");

            ViewBag.TenantId = tenantId;
            ViewBag.PageName = PageName;
            ViewBag.Logo = "~/" + Logo;

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
            _logger.LogInformation($"CompletePayment method call ended");

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

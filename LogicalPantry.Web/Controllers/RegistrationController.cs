using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogicalPantry.Web.Controllers
{
    [Route("Registration")]
    public class RegistrationController : BaseController
    {
        IInformationService _informationService;
        IRegistrationService _registrationService;
        private readonly ILogger _logger;
        public RegistrationController(IRegistrationService registrationService, IInformationService informationService, ILogger<RegistrationController> logger)
        {
                _registrationService = registrationService;
                _logger = logger;
            _informationService = informationService;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation($"Index method call started");
            _logger.LogInformation($"Index method call ended");
            return View();
        }

        
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto user) 
        {
            _logger.LogInformation($"Register method call started");

            var tenantId = HttpContext.Items["TenantId"]?.ToString();

            user.TenantId = int.Parse(tenantId);

            var response= await _registrationService.RegisterUser(user);

           

            if (response != null && response.Success)
            {
                @TempData["MessageClass"] = "alert-success";
                @TempData["SuccessMessageUser"] = "Registartion Successfull";
            }
            else
            {
                @TempData["MessageClass"] = "alert-danger";
                @TempData["SuccessMessageUser"] = "Failed to Save User server error.";
                return View("Index");

            }

            _logger.LogInformation($"Register method call ended");
            //return RedirectToAction("UserCalendar", "TimeSlot", new { area = "" });
            return Redirect($"/{TenantName}/TimeSlot/UserCalendar");
        }
        [HttpGet]
        public object ValidateEmail(string emailId) 
        {
            _logger.LogInformation($"ValidateEmail method call started");
             var response = _registrationService.CheckEmailIsExist(emailId);
            _logger.LogInformation($"ValidateEmail method call ended");

            return response;
        }
    }
}

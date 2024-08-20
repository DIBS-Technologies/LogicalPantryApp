using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogicalPantry.Web.Controllers
{
    public class RegistrationController : BaseController
    {
        IRegistrationService _registrationService;
        private readonly ILogger _logger;
        public RegistrationController(IRegistrationService registrationService, ILogger<RegistrationController> logger)
        {
                _registrationService = registrationService;
                _logger = logger;
        }
        public IActionResult Index()
        {
            _logger.LogInformation($"Index method call started");
            _logger.LogInformation($"Index method call ended");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserDto user) 
        {
            _logger.LogInformation($"Register method call started");
            var response= await _registrationService.RegisterUser(user);

            _logger.LogInformation($"Register method call ended");
         
            if(response != null && response.Success)
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

            return RedirectToAction("UserCalendar", "TimeSlot", new { area = "" });

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

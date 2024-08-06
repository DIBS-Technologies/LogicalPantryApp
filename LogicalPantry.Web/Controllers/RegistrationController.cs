using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace LogicalPantry.Web.Controllers
{
    public class RegistrationController : Controller
    {
        IRegistrationService _registrationService;
        public RegistrationController(IRegistrationService registrationService)
        {
                _registrationService = registrationService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(UserDto user) 
        {
            var response=_registrationService.RegisterUser(user).Result;

            if (response.Success)
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
            var response = _registrationService.CheckEmailIsExist(emailId);
            return response;
        }
    }
}

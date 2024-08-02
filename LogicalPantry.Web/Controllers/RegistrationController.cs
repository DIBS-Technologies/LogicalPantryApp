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
        public object Register(UserDto user) 
        {
            var response=_registrationService.RegisterUser(user).Result;
            return response;
        }
        [HttpGet]
        public object ValidateEmail(string emailId) 
        {
            var response = _registrationService.CheckEmailIsExist(emailId);
            return response;
        }
    }
}

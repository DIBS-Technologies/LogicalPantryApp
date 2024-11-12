using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Web.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogicalPantry.Web.Controllers
{
    /// <summary>
    /// Handles user registration and validation actions for the application.
    /// </summary>
    [Route("Registration")]
    public class RegistrationController : BaseController
    {
        IInformationService _informationService;
        IRegistrationService _registrationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationController"/> class.
        /// </summary>
        /// <param name="registrationService">Service for handling user registration operations.</param>
        /// <param name="informationService">Service for handling tenant information.</param>
        /// <param name="logger">Logger for logging controller actions.</param>
        public RegistrationController(IRegistrationService registrationService, IInformationService informationService, ILogger<RegistrationController> logger)
        {
                _registrationService = registrationService;
                _logger = logger;
            _informationService = informationService;
        }


        /// <summary>
        /// Displays the registration index page.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> representing the result of the action.</returns>
        //[Authorize(Roles = $"{UserRoleEnum.User}")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation($"Index method call started");
            _logger.LogInformation($"Index method call ended");
            return View();
        }


        /// <summary>
        /// Registers a new user based on the provided user information.
        /// </summary>
        /// <param name="user">The user data to register.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the registration operation.</returns>
       // [Authorize(Roles = $"{UserRoleEnum.User}")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserDto user) 
        {
            _logger.LogInformation($"Register method call started");

            // Retrieve tenant ID from HttpContext.
            var tenantId = HttpContext.Items["TenantId"]?.ToString();

            user.TenantId = int.Parse(tenantId);

            // Register the user using the registration service.
            var response = await _registrationService.RegisterUser(user);


            // Check the result of the registration process.
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


        /// <summary>
        /// Validates if the provided email address is already in use.
        /// </summary>
        /// <param name="emailId">The email address to validate.</param>
        /// <returns>A response indicating whether the email exists or not.</returns>
        [HttpGet]
        public object ValidateEmail(string emailId) 
        {
            _logger.LogInformation($"ValidateEmail method call started");
            // Check if the email address exists using the registration service.
            var response = _registrationService.CheckEmailIsExist(emailId);
            _logger.LogInformation($"ValidateEmail method call ended");

            return response;
        }
    }
}

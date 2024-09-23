using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Logging;
using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Web.Models;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using LogicalPantry.Web.Helper;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.DTOs.UserDtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.Models.Models;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using LogicalPantry.Services.RegistrationService;
using Azure;

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : BaseController
    {
        IUserService _userService;
        IRegistrationService _registrationService;

        private readonly ILogger _logger;
        public UserController(IUserService userService, ILogger<UserController> logger, IRegistrationService registrationService)
        {
            _userService = userService;
            _logger = logger;
            _registrationService = registrationService;
        }

        /// <summary>
        /// Displays the default index page for the UserController.
        /// </summary>
        /// <returns>Returns the view for the index page.</returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Index method call started.");

            _logger.LogInformation("Index method call ended.");
            return View();
        }


        /// <summary>
        /// Manages users by displaying all registered users for a specific tenant.
        /// </summary>
        /// <returns>Returns the view with a list of all registered users.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpGet]
        [Route("ManageUsers")]
        public async Task<IActionResult> ManageUsers()
        {
            _logger.LogInformation("GetAllusers object call started.");
            var tenantId = TenantId;
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");

            ViewBag.TenantId = tenantId;
            ViewBag.PageName = TenantDisplayName;
            var response = await _userService.GetAllRegisteredUsersAsync((int)tenantId);
            _logger.LogInformation("GetAllusers object call ended.");

            return View(response.Data);
        }


        /// <summary>
        /// Retrieves session data such as user email and username.
        /// </summary>
        /// <returns>Returns user email and username if found, otherwise returns NotFound.</returns>
        [HttpGet("session")]
        public IActionResult GetSessionData()
        {
            _logger.LogInformation("GetSessionData method call started");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }
            _logger.LogInformation("GetSessionData method call ended");
            return Ok(new { UserEmail = userEmail, UserName = userName });
        }


        /// <summary>
        /// Updates user information based on user ID and allowance status.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="isAllow">The allowance status to set for the user.</param>
        /// <returns>Returns Ok if the update is successful, otherwise returns StatusCode 500 for internal server error.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("UpdateUser")] //ThisExpressionSyntax is ForbidResult allow method
        public async Task<IActionResult> UpdateUser(string userId, bool isAllow)
        {
            

            var userDto = new UserDto { Id = int.Parse(userId), IsAllow = isAllow };

            var response = await _userService.UpdateUserAsync(userDto);

            if (response.Success)
            {
                TempData["MessageClass"] = "alert-success";
                TempData["SuccessMessageUser"] = "Changes Saved Successfully";
                // Log the ending of the Index method execution.
                _logger.LogInformation("UpdateUser post method call ended");
                return Ok(new { success = true });
            }
            else
            {
                TempData["MessageClass"] = "alert-danger";
                TempData["SuccessMessageUser"] = "Internal server error.";
                // Log the ending of the Index method execution.
                _logger.LogInformation("UpdateUser post method call ended");
                return StatusCode(500, "Internal server error.");
            }
        }

        /// <summary>
        /// Updates multiple users' attendance statuses in batch.
        /// </summary>
        /// <param name="userStatuses">A list of UserDto objects containing user IDs and their updated statuses.</param>
        /// <returns>Returns Ok if the update is successful; otherwise, returns StatusCode 500 for internal server error.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("UpdateUserBatch")]
        public async Task<IActionResult> UpdateUserBatch([FromBody] List<UserDto> userStatuses)
        {
            _logger.LogInformation("UpdateUserBatch method call started");
            if (userStatuses == null || !userStatuses.Any())
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                //get response from service for users
                var response = await _userService.UpdateUserAttendanceStatusAsync(userStatuses);

                if (response.Success)
                {
                    TempData["MessageClass"] = "alert-success";
                    TempData["SuccessMessageUserBatch"] = "Changes Saved Successfully";
                    _logger.LogInformation("UpdateUserBatch method call ended");
                    return Ok(new { success = true });
                }
                else
                {
                    TempData["MessageClass"] = "alert-danger";
                    TempData["SuccessMessageUserBatch"] = "Internal server error.";
                    return StatusCode(500, "Internal server error.");
                }
            }
            catch (Exception ex)
            {
                TempData["MessageClass"] = "alert-danger";
                TempData["SuccessMessageUserBatch"] = $"Error: {ex.Message}";
                _logger.LogCritical($"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }


        /// <summary>
        /// Retrieves the user ID based on the provided email address.
        /// </summary>
        /// <param name="dto">UserDto object containing the email address.</param>
        /// <returns>Returns Ok with the user ID if found; otherwise, returns BadRequest for invalid email or StatusCode 500 for internal server error.</returns>
        [HttpPost("GetUserIdByEmail")]
        public async Task<IActionResult> GetUserIdByEmail([FromBody]UserDto dto)
        {
            _logger.LogInformation("GetUserIdByEmail method call Started");
            if (dto == null || string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new { Message = "Invalid email." });
            }

            var response = await _userService.GetUserIdByEmail(dto.Email);

            if (response.Success)
            {
                _logger.LogInformation("GetUserIdByEmail method call ended");
                return Ok(new { UserId = response.Data });
            }
            else
            {
                return StatusCode(500, response);
            }

        }



        /// <summary>
        /// Deletes a user based on the provided user ID.
        /// </summary>
        /// <param name="userId">User ID to be deleted.</param>
        /// <returns>Returns Ok if the user is successfully deleted; otherwise, returns BadRequest for invalid ID format or StatusCode 500 for internal server error.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            _logger.LogInformation("DeleteUser method call started");
            try
            {
                
                if (!int.TryParse(userId, out int user))
                {
                    return BadRequest("Invalid user ID format.");
                }

                // Call the service to delete the user
                var result = await _userService.DeleteUserAsync(user);

                if (result.Success)
                {
                    _logger.LogInformation("DeleteUser method call ended");
                    //return View("Index"); 
                    return Ok(result);
                }
                else
                {
                    return View("Index");
                }
                
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }


        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>Returns the view for the registration page.</returns>
        [HttpGet("Register")]
        public async Task<IActionResult> Register()
        {

            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");

            //ViewBag.TenantId = tenantId;
            ViewBag.PageName = TenantDisplayName;

            return View();
        }


        /// <summary>
        /// Handles user registration by processing the provided <see cref="UserDto"/>.
        /// Redirects users based on the result of the registration process.
        /// </summary>
        /// <param name="user">The user data transfer object containing user registration details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that redirects to either the PayPal donation page or the User Calendar page, 
        /// depending on the registration result. Displays appropriate success or error messages using TempData.
        /// </returns>
        /// <remarks>
        /// The method uses the <see cref="HttpContext.Items"/> to get the tenant ID. It then uses this ID to set the tenant 
        /// information on the <paramref name="user"/> object before calling the <see cref="IRegistrationService"/> to 
        /// register the user. Based on the registration outcome, it redirects to the appropriate page and sets TempData 
        /// messages for user feedback.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when the tenant ID cannot be parsed from the context.</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during the registration process.</exception>
        [Authorize(Roles = $"{UserRoleEnum.User}")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(/*[FromBody]*/ UserDto user)
        {
            _logger.LogInformation($"Register method call started");

            var tenantId = HttpContext.Items["TenantId"]?.ToString();

            user.TenantId = int.Parse(tenantId);

            var response = await _registrationService.RegisterUser(user);


            if (response != null && response.Success)
            {
                @TempData["MessageClass"] = "alert-success";
                @TempData["SuccessMessageUser"] = "Registartion Successfull";

                if (!response.Data)
                {
                    return Redirect($"/{TenantName}/TimeSlot/UserCalendar");
                }
                else
                {
                    return Redirect($"/{TenantName}/Donation/PayPal");
                }
            }
            else
            {
                @TempData["MessageClass"] = "alert-danger";
                @TempData["SuccessMessageUser"] = "Failed to Save User server error.";
                return Redirect($"/{TenantName}/TimeSlot/UserCalendar");

            }
            _logger.LogInformation($"Register method call ended");
            //return RedirectToAction("UserCalendar", "TimeSlot", new { area = "" });
        }

        /// <summary>
        /// Displays the user's profile page with their details fetched by email.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the profile view with user details if found; otherwise, renders the profile view with no data.
        /// </returns>
        /// <remarks>
        /// The method retrieves the user's email from the authenticated user and fetches their details from the user service. 
        /// If the details are successfully retrieved, they are passed to the view. Otherwise, an empty profile view is returned.
        /// </remarks>
        /// <exception cref="Exception">Thrown for unexpected errors while fetching user details.</exception>
        [Authorize(Roles = $"{UserRoleEnum.Admin},{UserRoleEnum.User}")]
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var email = UserEmail;
            if (email != null)
            {
                var response = await _userService.GetUserDetailsByEmail(email); 
                if (response != null && response.Success) 
                 {
                    return View("Profile", response.Data);
                }
            }
            return View();
        }


        /// <summary>
        /// Handles the profile update and profile picture upload for the user.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> containing updated user details.</param>
        /// <param name="ProfilePicture">The profile picture file to upload.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that redirects to the profile page upon successful update or displays an error message if the update fails.
        /// </returns>
        /// <remarks>
        /// The method validates the user input and profile picture. If valid, it uploads the picture and updates the user's profile. 
        /// It then redirects to the profile page with a success or error message based on the result of the update operation.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when the tenant ID cannot be parsed from the context.</exception>
        /// <exception cref="Exception">Thrown for unexpected errors during profile update or file upload.</exception>
        [Authorize(Roles = $"{UserRoleEnum.Admin},{UserRoleEnum.User}")]
        [HttpPost("Profile")]
        public async Task<IActionResult> Profile(UserDto user, IFormFile ProfilePicture)
        {
            _logger.LogInformation("Profile method call started");

            if (!ModelState.IsValid)
            {
                TempData["MessageClass1"] = "alert-danger";
                TempData["SuccessMessageUser1"] = "Invalid data provided.";
                return View("Profile", user);
            }

            var tenantId = HttpContext.Items["TenantId"]?.ToString();
            if (string.IsNullOrEmpty(tenantId))
            {
                TempData["MessageClass1"] = "alert-danger";
                TempData["SuccessMessageUser1"] = "Failed to retrieve Tenant ID.";
                return View("Profile", user);
            }

            // Handle profile picture upload
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
                var extension = Path.GetExtension(ProfilePicture.FileName)?.ToLower();

                _logger.LogInformation($"Uploaded file extension: {extension}");
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    TempData["MessageClass1"] = "alert-danger";
                    TempData["SuccessMessageUser1"] = "Invalid file type. Please upload a JPG, JPEG, PNG, GIF, or SVG file.";
                    _logger.LogInformation($"File type validation failed. Allowed extensions: {string.Join(", ", allowedExtensions)}");
                    return View("Profile", user);
                }

                // Ensure the directory exists
                var profileDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile");
                if (!Directory.Exists(profileDir))
                {
                    Directory.CreateDirectory(profileDir);
                }

                // Generate unique file name and save the file
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(profileDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                // Assign the uploaded picture URL to the user DTO
                user.ProfilePictureUrl = $"/profile/{fileName}";
            }

            user.TenantId = int.Parse(tenantId);
            var response = await _userService.ProfileRagistration(user);

            if (response != null && response.Success)
            {
                _logger.LogInformation("Profile method call ended successfully");
                TempData["MessageClass1"] = "alert-success";
                TempData["SuccessMessageUser1"] = "Profile saved successfully.";

                // Use Post/Redirect/Get to avoid duplicate submissions
                // return RedirectToAction("Profile");
               return Redirect($"/{TenantName}/User/Profile");
            }
            else
            {
                TempData["MessageClass1"] = "alert-danger";
                TempData["SuccessMessageUser1"] = "Failed to save profile due to a server error.";
                _logger.LogInformation("Profile method call ended with errors");
                return Redirect($"/{TenantName}/User/Profile");
            }
        }



        /// <summary>
        /// Validates if the provided email ID exists by checking it through the registration service.
        /// </summary>
        /// <param name="emailId">The email address to validate.</param>
        /// <returns>
        /// An <see cref="object"/> representing the result of the email existence check.
        /// </returns>
        /// <remarks>
        /// This method calls the registration service to check whether the provided email address is already registered.
        /// It logs the start and end of the method execution for debugging purposes.
        /// </remarks>
        /// <exception cref="Exception">Thrown for unexpected errors while checking email existence.</exception>
        [HttpGet]
        public object ValidateEmail(string emailId)
        {
            _logger.LogInformation($"ValidateEmail method call started");
            var response = _registrationService.CheckEmailIsExist(emailId);
            _logger.LogInformation($"ValidateEmail method call ended");

            return response;
        }


        /// <summary>
        /// Retrieves user details based on the provided email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that returns an <see cref="OkObjectResult"/> with the user's allowance status if found,
        /// a <see cref="NotFoundResult"/> if the user data is not found, or a <see cref="StatusCodeResult"/> for internal server errors.
        /// </returns>
        /// <remarks>
        /// This method attempts to fetch user details based on the provided email. If the email is found, it returns whether the user is allowed.
        /// If the email is not found, it returns a 404 Not Found response. In case of exceptions, it returns a 500 Internal Server Error response.
        /// </remarks>
        /// <exception cref="Exception">Thrown for unexpected errors while retrieving user details.</exception>
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string email)
        {
            _logger.LogInformation("Get Object call started");
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    var userExisist = await _userService.GetUserByEmailAsync(email, 0);

                    if (userExisist?.Data == null)
                    {
                        return NotFound("Tenant data not found");
                    }
                    _logger.LogInformation("Get Object call ended");
                    return Ok(userExisist.Data.IsAllow);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tenant.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }


}

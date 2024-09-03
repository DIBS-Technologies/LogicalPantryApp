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

        public IActionResult Index()
        {
            _logger.LogInformation("Index method call started.");

            _logger.LogInformation("Index method call ended.");
            return View();
        }

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

        //public object GetUserbyId(int tenentId) 
        //{
        //    _logger.LogInformation("GetUserbyId object call Started.");

        //    if (tenentId == 0)return null;
        //    var response = _userService.GetUserByIdAsync(tenentId).Result;
        //    _logger.LogInformation("GetUserbyId object call ended.");

        //    return response;
        //}
        //[HttpGet]
        //public object GetUsersbyTimeSlot(DateTime timeslot, int tenentId) 
        //{
        //    _logger.LogInformation("GetUsersbyTimeSlot object call Started.");
        //    if (tenentId == 0 || timeslot == null) return null;
        //    var response = _userService.GetUsersbyTimeSlot(timeslot,tenentId).Result;
        //    _logger.LogInformation("GetUsersbyTimeSlot object call ended.");

        //    return response;
        //}
        //[HttpPost]
        //public object PutUserStatus(List<UserAttendedDto> userDto)
        //{
        //    _logger.LogInformation("PutUserStatus object call started.");

        //    if (userDto != null)
        //    {
        //        var response = _userService.UpdateUserAllowStatusAsync(userDto).Result;
        //        _logger.LogInformation("PutUserStatus object call ended.");
        //        return response;

        //    }
        //    else { return null; }


        //}

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

        //[Route("UpdateUser")]
        //public async Task<IActionResult> UpdateUser([FromBody]  string updatedNotificationList)
        //{
        //    if (updatedNotificationList == null)
        //    {
        //        return BadRequest("Invalid data.");
        //    }

        //    var updatedNotificationListObject = JsonConvert.DeserializeObject<UserAllowStatusDto>(updatedNotificationList);

        //    var userDto = new UserDto { Id = updatedNotificationListObject.Id, IsAllow = updatedNotificationListObject.IsAllow };


        //    if (userDto != null)
        //    {
        //        var response = _userService.UpdateUserAsync(userDto).Result;

        //        if (response != null)
        //        {
        //            @TempData["MessageClass"] = "alert-success";
        //            @TempData["SuccessMessageUser"] = "User Saved Successfully";

        //            return Ok(new { success = true });
        //        }
        //        else
        //        {
        //            @TempData["MessageClass"] = "alert-success";
        //            @TempData["SuccessMessageUser"] = "Internal server error.";
        //            return StatusCode(500, "Internal server error.");
        //        }
        //    }
        //    return Ok(new { success = false });
        //}

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

        [HttpGet("Register")]
        public async Task<IActionResult> Register()
        {

            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");

            //ViewBag.TenantId = tenantId;
            ViewBag.PageName = TenantDisplayName;

            return View();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDto user)
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


        [HttpGet]
        public object ValidateEmail(string emailId)
        {
            _logger.LogInformation($"ValidateEmail method call started");
            var response = _registrationService.CheckEmailIsExist(emailId);
            _logger.LogInformation($"ValidateEmail method call ended");

            return response;
        }

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

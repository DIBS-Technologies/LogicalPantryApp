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

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : BaseController
    {
        IUserService _userService;
        private readonly ILogger _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
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
            var tenantIdString = HttpContext.Session.GetString("TenantId");

            if (!int.TryParse(tenantIdString, out int tenantId) || tenantId == 0)
            {
                return BadRequest("Invalid tenant ID");
            }
            var PageName = HttpContext.Session.GetString("PageName");

            ViewBag.TenantId = tenantId;
            ViewBag.PageName = PageName;
            var response = await _userService.GetAllRegisteredUsersAsync();
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
        public async Task<IActionResult> UpdateUser(int userId, bool isAllow)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var userDto = new UserDto { Id = userId, IsAllow = isAllow };

            var response = await _userService.UpdateUserAsync(userDto);

            if (response.Success)
            {
                TempData["MessageClass"] = "alert-success";
                TempData["SuccessMessageUser"] = "User Saved Successfully";
                return Ok(new { success = true });
            }
            else
            {
                TempData["MessageClass"] = "alert-danger";
                TempData["SuccessMessageUser"] = "Internal server error.";
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
                    TempData["SuccessMessageUserBatch"] = "User Saved Successfully";
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
        public async Task<IActionResult> GetUserIdByEmail([FromBody] UserDto dto)
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

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("DeleteUser method call started");
            try
            {
                
                if (!int.TryParse(id, out int userId))
                {
                    return BadRequest("Invalid user ID format.");
                }

                // Call the service to delete the user
                var result = await _userService.DeleteUserAsync(userId);

                if (result.Success)
                {
                    _logger.LogInformation("DeleteUser method call ended");
                    return View("Index"); 
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


    }


}

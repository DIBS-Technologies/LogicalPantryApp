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

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("ManageUsers")]
        public async Task<IActionResult> ManageUsers()
        {
            var response = _userService.GetAllRegisteredUsersAsync().Result.Data;
            return View(response);
        }

        public object GetUserbyId(int tenentId) 
        {
            if(tenentId == 0)return null;
            var response = _userService.GetUserByIdAsync(tenentId).Result;
            return response;
        }
        [HttpGet]
        public object GetUsersbyTimeSlot(DateTime timeslot, int tenentId) 
        {
            if(tenentId == 0 || timeslot == null) return null;
            var response = _userService.GetUsersbyTimeSlot(timeslot,tenentId).Result;
            return response;
        }
        [HttpPost]
        public object PutUserStatus(List<UserAttendedDto> userDto)
        {
            if (userDto != null)
            {
                var response = _userService.UpdateUserAllowStatusAsync(userDto).Result;
                return response;

            }
            else { return null; }


        }

        [HttpGet("session")]
        public IActionResult GetSessionData()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userName))
            {
                return NotFound();
            }

            return Ok(new { UserEmail = userEmail, UserName = userName });
        }

        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(string updatedNotificationList)
        {
            if (updatedNotificationList == null)
            {
                return BadRequest("Invalid data.");
            }

            var updatedNotificationListObject = JsonConvert.DeserializeObject<UserAllowStatusDto>(updatedNotificationList);

            var userDto = new UserDto { Id = updatedNotificationListObject.Id, IsAllow = updatedNotificationListObject.IsAllow };


            if (userDto != null)
            {
                var response = _userService.UpdateUserAsync(userDto).Result;

                if (response != null)
                {
                    @TempData["MessageClass"] = "alert-success";
                    @TempData["SuccessMessageUser"] = "User Saved Successfully";

                    return Ok(new { success = true });
                }
                else
                {
                    @TempData["MessageClass"] = "alert-success";
                    @TempData["SuccessMessageUser"] = "Internal server error.";
                    return StatusCode(500, "Internal server error.");
                }
            }
            return Ok(new { success = false });
        }

        [Route("UpdateUserBatch")]
        [HttpGet]
        //public async Task<IActionResult> UpdateUserBatch(string userStatuses)
        //{
        //    if (userStatuses == null)
        //    {
        //        return BadRequest("Invalid data.");
        //    }

        //   var updatedNotificationListObject = JsonConvert.DeserializeObject<List<UserAllowStatusDto>>(userStatuses);


        //    if (updatedNotificationListObject != null)
        //    {
        //        var response = _userService.UpdateUserAllowStatusAsync(updatedNotificationListObject).Result;

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

        [Route("UpdateUserBatch")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserBatch([FromBody] List<UserAttendedDto> userStatuses)
        {
            if (userStatuses == null || !userStatuses.Any())
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var response = await _userService.UpdateUserAllowStatusAsync(userStatuses);

                if (response.Success)
                {
                    TempData["MessageClass"] = "alert-success";
                    TempData["SuccessMessageUserBatch"] = "User Saved Successfully";
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("GetUserIdByEmail")]
        public async Task<IActionResult> GetUserIdByEmail([FromBody] UserDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new { Message = "Invalid email." });
            }

            var response = await _userService.GetUserIdByEmail(dto.Email);

            if (response.Success)
            {
                return Ok(new { UserId = response.Data });
            }
            else
            {
                return StatusCode(500, response);
            }
        }



    }


}

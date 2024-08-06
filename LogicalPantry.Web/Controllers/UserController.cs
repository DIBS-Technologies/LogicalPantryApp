using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
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

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        IUserService _userService;
        private readonly ILogger _logger;
        public UserController(IUserService userService, ILogger logger)
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
        public object GetAllusers()
        {
            _logger.LogInformation("GetAllusers object call started.");
            var response = _userService.GetAllRegisteredUsersAsync().Result;
            _logger.LogInformation("GetAllusers object call ended.");

            return response;
        }

        public object GetUserbyId(int tenentId) 
        {
            _logger.LogInformation("GetUserbyId object call Started.");

            if (tenentId == 0)return null;
            var response = _userService.GetUserByIdAsync(tenentId).Result;
            _logger.LogInformation("GetUserbyId object call ended.");

            return response;
        }
        [HttpGet]
        public object GetUsersbyTimeSlot(DateTime timeslot, int tenentId) 
        {
            _logger.LogInformation("GetUsersbyTimeSlot object call Started.");
            if (tenentId == 0 || timeslot == null) return null;
            var response = _userService.GetUsersbyTimeSlot(timeslot,tenentId).Result;
            _logger.LogInformation("GetUsersbyTimeSlot object call ended.");

            return response;
        }
        [HttpPost]
        public object PutUserStatus(List<UserAllowStatusDto> userDto)
        {
            _logger.LogInformation("PutUserStatus object call started.");

            if (userDto != null)
            {
                var response = _userService.UpdateUserAllowStatusAsync(userDto).Result;
                _logger.LogInformation("PutUserStatus object call ended.");
                return response;

            }
            else { return null; }



        }
    }


}

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
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public object GetAllusers()
        {
            var response = _userService.GetAllRegisteredUsersAsync().Result;
            return response;
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
        public object PutUserStatus(List<UserAllowStatusDto> userDto)
        {
            if (userDto != null)
            {
                var response = _userService.UpdateUserAllowStatusAsync(userDto).Result;
                return response;

            }
            else { return null; }


        }
    }


}

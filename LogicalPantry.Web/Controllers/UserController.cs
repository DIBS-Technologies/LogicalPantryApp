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

namespace LogicalPantry.Web.Controllers
{
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
        public object GetAllusers(int tenentId)
        {
            List<UserDto> userDtos = new List<UserDto>();
            userDtos = _userService.Get(tenentId);
            return userDtos;
        }
        [HttpGet]
        public object GetUsersbyTimeSlot(DateTime timeslot, int tenentId) 
        {
            List<UserDto> userDtos = new List<UserDto>();
            userDtos = _userService.GetUsersbyTimeSlot(timeslot,tenentId);
            return userDtos;
        }
        [HttpPost]
        public object Post(List<UserDto> userDto)
        {
            if (userDto == null)
            {
                var response = _userService.Post(userDto);
                return response;

            }
            else { return null; }


        }
    }


}

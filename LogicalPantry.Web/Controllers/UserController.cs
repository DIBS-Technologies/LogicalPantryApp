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
using System.Security.Claims;

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userServices;
        private readonly IRoleService _roleService;

        public UserController(IUserService userServices, IRoleService roleService, ILogger<UserController> logger)
        {
            _userServices = userServices;
            _roleService = roleService;
            _logger = logger;
        }

        [HttpGet("ManageUsers")]
        public async Task<IActionResult> ManageUsers()
        {
            var response = await _userServices.GetAllRegisteredUsersAsync();
            if (response.Success)
            {
                return View(response.Data);
            }

            return BadRequest(response);
        }

        //[HttpPost("UpdateUserAllowStatus")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateUserAllowStatus([FromBody] UserDto userDto)
        //{
        //    if (userDto == null || userDto.Id == 0)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid user data." });
        //    }

        //    var response = await _userServices.UpdateUserAsync(userDto);
        //    if (response.Success)
        //    {
        //        return Ok(new { success = true });
        //    }
        //    else
        //    {
        //        return BadRequest(new { success = false, message = response.Message });
        //    }
        //}


        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var userResponse = await _userServices.GetUserByIdAsync(id);
            if (userResponse.Success)
            {
                return View(userResponse.Data);
            }

            // Handle user not found
            return NotFound();
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _userServices.UpdateUserAsync(userDto);
                if (response.Success)
                {
                    return RedirectToAction("ManageUsers");
                }
                else
                {
                    ModelState.AddModelError("", response.Message);
                }
            }

            return View(userDto);
        }

        //[HttpPost("DeleteUser")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteUser([FromBody] UserDto userDto)
        //{
        //    if (userDto == null || userDto.Id == 0)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid user data." });
        //    }

        //    var response = await _userServices.DeleteUserAsync(userDto.Id);
        //    if (response.Success)
        //    {
        //        return Ok(new { success = true });
        //    }
        //    else
        //    {
        //        return BadRequest(new { success = false, message = response.Message });
        //    }
        //}




        [HttpPost("UpdateUserAllowStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserAllowStatus([FromBody] UserDto userDto)
        {
            if (userDto == null || userDto.Id == 0)
            {
                return BadRequest(new { success = false, message = "Invalid user data." });
            }

            var response = await _userServices.UpdateUserAsync(userDto);
            if (response.Success)
            {
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest(new { success = false, message = response.Message });
            }
        }

        [HttpPost("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromBody] UserDto userDto)
        {
            if (userDto == null || userDto.Id == 0)
            {
                return BadRequest(new { success = false, message = "Invalid user data." });
            }

            var response = await _userServices.DeleteUserAsync(userDto.Id);
            if (response.Success)
            {
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest(new { success = false, message = response.Message });
            }
        }









        // [HttpGet("Register")]
        //public async Task<IActionResult> Register()
        //{
        //    var loggedInUser = await _userContext.GetCurrentUserAsync();
        //    var userDto = new UserDto
        //    {
        //        Email = loggedInUser.Email,
        //        IsRegistered = loggedInUser.IsRegistered
        //    };

        //    return View(userDto);
        //}

        //[HttpPost("Register")]
        //public async Task<IActionResult> Register(UserDto userDto)
        //{
        //    var loggedInUser = await _userContext.GetCurrentUserAsync();

        //    if (userDto.Email != loggedInUser.Email)
        //    {
        //        ModelState.AddModelError("Email", "Please enter the correct email ID.");
        //        return View(userDto);
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var response = await _userServices.UpdateUserAsync(userDto);
        //        if (response.Success)
        //        {
        //            return RedirectToAction("Index", "Home"); // Redirect to a success page or home
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", response.Message);
        //        }
        //    }

        //    return View(userDto);
        //}









        public async Task<IActionResult> UserInfo()
        {
            var identity = User.Identity as ClaimsIdentity;
            var userEmail = identity?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Email claim not found for the current user.");
                return View("Error"); // Handle case where email claim is not found
            }

            var currentUser = await _userServices.GetUserByEmailAsync(userEmail);

            if (currentUser == null)
            {
                _logger.LogWarning($"User with email {userEmail} not found.");
                return View("Error"); // Handle case where user is not found in your application's database
            }

            return View(currentUser);
        }







        ///// <summary>
        ///// Retrieves all users and renders the view with the user data.
        ///// </summary>
        ///// <returns>Returns the view with the user data.</returns>
        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> GetAllUser()
        //{
        //    var serviceResponse = await _userServices.GetAllUsers();
        //    if (serviceResponse?.Success == true)
        //    {
        //        return View(ViewConstants.GETALLUSER, serviceResponse.Data); // Pass the data to the Index view
        //    }
        //    return View("Error");
        //}

        ///// <summary>
        ///// Adds a new user.
        ///// </summary>
        ///// <param name="addUser">The user data to be added.</param>
        ///// <returns>Returns the view with the added user data or error message.</returns>
        ////[Authorize]
        //public async Task<IActionResult> AddUser(AddUserDto addUser)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var serviceResponse = await _userServices.AddUser(addUser);
        //        if (serviceResponse?.Success == true)
        //        {
        //            // Redirect to the user details page with the newly added user's ID
        //            return RedirectToAction(ViewConstants.GETALLUSER, new { id = serviceResponse.Data.UserId });
        //        }
        //    }

        //    // If the ModelState is not valid or if there was an error adding the user, return to the AddUser view
        //    return View();
        //}





        [HttpPost]
        public async Task<IActionResult> UpdateUserAllowStatus([FromBody] UserAllowStatusDto userAllowStatusDto)
        {
            var response = await _userServices.UpdateUserAllowStatusAsync(userAllowStatusDto);
            return Json(response);
        }




    }


}

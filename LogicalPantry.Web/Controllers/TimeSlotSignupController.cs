using Azure;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.TimeSlotSignupService;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Web.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Tweetinvi.Core.Extensions;

namespace LogicalPantry.Web.Controllers
{
    [Route("TimeSlotSignup")]
    public class TimeSlotSignupController : BaseController
    {
        private readonly ITimeSlotService _timeSlotService;
        private readonly ITimeSlotSignupService _timeSlotSignupService;
        private readonly ILogger _logger;
     

        public TimeSlotSignupController( ITimeSlotService timeSlotService , ITimeSlotSignupService timeSlotSignupService, ILogger<TimeSlotSignupController> logger)
        {
            _logger = logger;
            _timeSlotService = timeSlotService;
            _timeSlotSignupService = timeSlotSignupService;
        }


        /// <summary>
        /// Displays the index page for time slot sign-ups.
        /// </summary>
        /// <returns>Returns the view for the index page.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin},{UserRoleEnum.User}")]
        [Route("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation("Index method call started.");

            _logger.LogInformation("Index method call ended.");
             return View();
        }

        /// <summary>
        /// Retrieves users who have signed up for a specific time slot.
        /// </summary>
        /// <param name="timeSlot">The date and time of the time slot.</param>
        /// <returns>Returns a list of users who signed up for the specified time slot.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin},{UserRoleEnum.User}")]
        [HttpGet("GetUsersbyTimeSlot")]
        public object GetUsersbyTimeSlot(DateTime timeSlot)
        {
            _logger.LogInformation("GetUsersbyTimeSlot method call started.");
            if (timeSlot == default)
            {
                return BadRequest(new { Message = "Invalid time slot provided." });
            }

            var response = _timeSlotSignupService.GetUserbyTimeSlot(timeSlot);
            _logger.LogInformation("GetUsersbyTimeSlot method call ended.");
            return response;
        }

        /// <summary>
        /// Adds a new time slot sign-up for a user.
        /// </summary>
        /// <param name="dto">The details of the time slot sign-up.</param>
        /// <returns>Returns a response indicating the success or failure of the sign-up operation.</returns>
        [Authorize(Roles = $"{UserRoleEnum.User}")]
        [HttpPost("AddTimeSlotSignUps")]
        public async Task<IActionResult> AddTimeSlotSignUps([FromBody] TimeSlotSignupDto dto)
        {
            _logger.LogInformation("AddTimeSlotSignUps method call started.");
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            try
            {
                var (success, message) = await _timeSlotSignupService.AddTimeSlotSignUp(dto);

                if (success)
                {
                    _logger.LogInformation("AddTimeSlotSignUps method call ended.");
                    return Ok(new { success = true, message = message });
                }
                else
                {
                    // Use Conflict (409) for cases where there is a conflict (e.g., duplicate sign-up)
                    return Conflict(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                // Log exception
                // Example: logger.LogError(ex, "Exception occurred in AddTimeSlotSignUps.");
                _logger.LogCritical($"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Exception occurred: " + ex.Message });
            }
        }

        
        //Added by kunal karne - 17-12-2024
        /// <summary>
        /// Deregister user from specific timeslot.
        /// </summary>
        /// <param name="dto">The details of the time slot sign-up.</param>
        /// <returns>Returns a response indicating the success or failure of the sign-up operation.</returns>
        [Authorize(Roles = $"{UserRoleEnum.User}")]
        [HttpPost("DeRegisterUserForTimeSlot")]
        public async Task<IActionResult> DeRegisterUserForTimeSlot([FromBody] TimeSlotSignupDto dto)
        {
            _logger.LogInformation("DeleteTimeSlotSignUps method call started.");

            if(dto == null)
            {
                return BadRequest(new {success = false, message = "Invalid data" });
            }

            try
            {
                var (success, message) = await _timeSlotSignupService.DeRegisterUserForTimeSlot(dto);

                if (success)
                {
                    _logger.LogInformation("DeleteTimeSlotSignUps method call ended.");
                    return Ok(new { success = true, message = message });
                }
                else
                {
                    // Use Conflict (409) for cases where there is a conflict (e.g., duplicate sign-up)
                    return Conflict(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                // Log exception
                // Example: logger.LogError(ex, "Exception occurred in AddTimeSlotSignUps.");
                _logger.LogCritical($"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "Exception occurred: " + ex.Message });
            }
           
        }



    }
}

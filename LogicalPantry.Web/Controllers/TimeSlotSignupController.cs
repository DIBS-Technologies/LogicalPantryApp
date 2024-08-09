using Azure;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.TimeSlotSignupService;
using LogicalPantry.Services.UserServices;
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
 
        [Route("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation("Index method call started.");

            _logger.LogInformation("Index method call ended.");
            return View();
        }

        [HttpGet]
        public object GetUsersbyTimeSlot(DateTime timeSlot)
        {
            _logger.LogInformation("GetUsersbyTimeSlot method call started.");
            if (timeSlot == default)
            {
                return BadRequest(new { Message = "Invalid time slot provided." });
            }

            var response = _timeSlotSignupService.GetUserbyTimeSlot(timeSlot).Result;
            _logger.LogInformation("GetUsersbyTimeSlot method call ended.");
            return response;
        }



   


        [HttpPost("AddTimeSlotSignUps")]
        public async Task<IActionResult> AddTimeSlotSignUps([FromBody] TimeSlotSignupDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            try
            {
                var (success, message) = await _timeSlotSignupService.AddTimeSlotSignUp(dto);

                if (success)
                {
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

                return StatusCode(500, new { success = false, message = "Exception occurred: " + ex.Message });
            }
        }



    }
}

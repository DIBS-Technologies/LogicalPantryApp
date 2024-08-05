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
    public class TimeSlotSignupController : Controller
    {


        private readonly ILogger<TimeSlotSignupController> _logger;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ITimeSlotSignupService _timeSlotSignupService;


        public TimeSlotSignupController(ILogger<TimeSlotSignupController> logger, ITimeSlotService timeSlotService , ITimeSlotSignupService timeSlotSignupService)
        {
            _logger = logger;
            _timeSlotService = timeSlotService;
            _timeSlotSignupService = timeSlotSignupService;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public object GetUsersbyTimeSlot(DateTime timeSlot)
        {
            if (timeSlot == default)
            {
                return BadRequest(new { Message = "Invalid time slot provided." });
            }

            var response = _timeSlotSignupService.GetUserbyTimeSlot(timeSlot).Result;
            return response;
        }



        //[HttpPost("AddTimeSlotSignUps")]
        //public async Task<IActionResult> AddTimeSlotSignUps([FromBody] TimeSlotSignupDto dto)
        //{
        //    if (dto == null)
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data" });
        //    }

        //    try
        //    {
        //        // Assuming you have a service to handle the business logic
        //        var result = await _timeSlotSignupService.AddTimeSlotSignUp(dto);

        //        if (result)
        //        {
        //            return Ok(new { success = true });
        //        }
        //        else
        //        {
        //            return StatusCode(500, new { success = false, message = "Error adding sign-up" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log exception
        //        return StatusCode(500, new { success = false, message = "Exception occurred: " + ex.Message });
        //    }
        //}


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

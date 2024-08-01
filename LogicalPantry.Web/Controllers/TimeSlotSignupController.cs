using Azure;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.TimeSlotSignupService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Tweetinvi.Core.Extensions;

namespace LogicalPantry.Web.Controllers
{
    [Route("TimeSlotSignup")]
    public class TimeSlotSignupController : Controller
    {
        ITimeSlotSignupService _repositoryService;
        public TimeSlotSignupController(ITimeSlotSignupService repositoryService)
        {
            _repositoryService = repositoryService;

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

            var response = _repositoryService.GetUserbyTimeSlot(timeSlot).Result;
            return response;
        }
        [HttpPost]
        public object Post(List<TimeSlotSignupDto> timeSlotSignupDtos) 
        {
            if (timeSlotSignupDtos == null || !timeSlotSignupDtos.Any())
            {
                return BadRequest(new { Message = "No time slot signups provided." });
            }

            var response = _repositoryService.PostTimeSlotSignup(timeSlotSignupDtos);
            
            return response;
        }
    }
}

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
        private readonly ILogger _logger;
        public TimeSlotSignupController(ITimeSlotSignupService repositoryService, ILogger logger)
        {
            _repositoryService = repositoryService;
            _logger = logger;
        }
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

            var response = _repositoryService.GetUserbyTimeSlot(timeSlot).Result;
            _logger.LogInformation("GetUsersbyTimeSlot method call ended.");
            return response;
        }
        [HttpPost]
        public object Post(List<TimeSlotSignupDto> timeSlotSignupDtos) 
        {
            _logger.LogInformation("timeSlotSignupDtos Object call started.");

            if (timeSlotSignupDtos == null || !timeSlotSignupDtos.Any())
            {
                return BadRequest(new { Message = "No time slot signups provided." });
            }

            var response = _repositoryService.PostTimeSlotSignup(timeSlotSignupDtos);
            _logger.LogInformation("timeSlotSignupDtos Object call ended.");

            return response;
        }
    }
}

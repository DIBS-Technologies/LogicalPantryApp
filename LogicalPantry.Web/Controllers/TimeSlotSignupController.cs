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
        public List<UserDto> GetUsersbyTimeSlot(DateTime timeSlot) 
        {

            List<UserDto> users = new List<UserDto>();
            //HttpClient client = new HttpClient();
            //client.GetAsync();
            List<UserDto> response = _repositoryService.GetUserbyTimeSlot(timeSlot);
            return users;
        }
        [HttpPost]
        public string Post(List<TimeSlotSignupDto> timeSlotSignupDtos) 
        {
           var response = _repositoryService.PostTimeSlotSignup(timeSlotSignupDtos);
            
            return String.IsNullOrEmpty(response)?string.Empty:response;
        }
    }
}

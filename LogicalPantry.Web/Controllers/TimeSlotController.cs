
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.Services.TimeSlotServices;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("TimeSlot")]
    public class TimeSlotController : Controller
    {

        private readonly ILogger<TimeSlotController> _logger;
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotController(ILogger<TimeSlotController> logger, ITimeSlotService timeSlotService)
        {
            _logger = logger;
            _timeSlotService = timeSlotService;
        }

        [HttpGet("Calendar")]
        public async Task<IActionResult> Calendar()
        {
            _logger.LogInformation("Calendar page accessed");

            // Fetch events from the service
            var timeSlots = await _timeSlotService.GetTimeSlotsAsync();

            var model = new CalendarViewModel
            {
                TenantDto = timeSlots
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent(TimeSlotDto timeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _timeSlotService.AddTimeSlotAsync(timeSlotDto);
                return RedirectToAction(nameof(Calendar));
            }
            return View("Error"); // Handle the error case appropriately
        }


          
        private long ToUnixTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }

        [HttpPost]
        public async Task<IActionResult> SaveEvent(TimeSlotDto timeSlotDto)
        {
            if (ModelState.IsValid)
            {
                if (timeSlotDto.Id == 0)
                {
                    // Add new event
                    await _timeSlotService.AddTimeSlotAsync(timeSlotDto);
                }
                else
                {
                    // Update existing event
                    await _timeSlotService.UpdateTimeSlotAsync(timeSlotDto);
                }
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            if (ModelState.IsValid)
            {
                await _timeSlotService.DeleteTimeSlotAsync(timeSlotDto.Id);
                return Ok();
            }
            return BadRequest();
        }
    }
}

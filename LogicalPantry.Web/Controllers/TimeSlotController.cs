using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
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

     

        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            if (timeSlotDto == null)
            {
                return BadRequest("Event data is null.");
            }

            if (ModelState.IsValid)
            {
                var success = await _timeSlotService.AddTimeSlotAsync(timeSlotDto);

                if (success)
                {
                    return Ok(); // Return a success response
                }
                else
                {
                    return StatusCode(500, "An error occurred while saving the time slot. Please try again."); // Return a server error response
                }
            }
            return BadRequest(ModelState); // Return a bad request response with validation errors
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


        [HttpGet("Calendar")]
        public async Task<IActionResult> Calendar()
        {
            _logger.LogInformation("Calendar page accessed");

            // Fetch events from the database
            var events = await _timeSlotService.GetAllEventsAsync();

            // Log the number of events fetched
            _logger.LogInformation($"Fetched {events.Count()} events from the database.");

            // Log details of each event
            foreach (var e in events)
            {
                _logger.LogInformation($"Event: Start={e.StartTime}, End={e.EndTime}, Title={e.TimeSlotName}");
            }

            // Map to your event model
            var calendarEvents = events.Select(e => new Event
            {
                Start = ToUnixTimestamp(e.StartTime),
                End = ToUnixTimestamp(e.EndTime),
                Title = e.TimeSlotName,
            }).ToList();

            // Log the number of events mapped
            _logger.LogInformation($"Mapped {calendarEvents.Count} events to calendar event model.");

            var model = new CalendarViewModel
            {
                Events = calendarEvents
            };

            return View(model);
        }




        // Helper method to get the start of the current week (Sunday)
        private DateTimeOffset GetStartOfWeek(DateTimeOffset dateTime)
        {
            int diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Sunday;
            return dateTime.AddDays(-diff).Date;
        }

        // Helper method to convert DateTimeOffset to Unix timestamp (seconds)
        private long ToUnixTimestamp(DateTimeOffset dateTime)
        {
            return dateTime.ToUnixTimeSeconds();
        }




        [HttpPost]
        public async Task<IActionResult> GetTimeSlotId([FromBody] TimeSlotDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            var timeSlotId = await _timeSlotService.GetTimeSlotIdAsync(request.StartTime, request.EndTime, request.TimeSlotName);

            if (timeSlotId.HasValue)
            {
                return Ok(new { TimeSlotId = timeSlotId });
            }
            else
            {
                return NotFound("Time slot not found.");
            }
        }
    }

}


    


   



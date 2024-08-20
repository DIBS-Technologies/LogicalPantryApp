﻿using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Tweetinvi.Core.Events;
using LogicalPantry.Services.InformationService;

namespace LogicalPantry.Web.Controllers
{
    [Route("TimeSlot")]
    public class TimeSlotController : BaseController
    {

        private readonly ILogger<TimeSlotController> _logger;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IUserService _userSercvice;
        private readonly IInformationService _informationService;


        public TimeSlotController(ILogger<TimeSlotController> logger, ITimeSlotService timeSlotService,IUserService userService ,IInformationService informationService)
        {
            _logger = logger;
            _timeSlotService = timeSlotService;
            _userSercvice = userService;
            _informationService = informationService;
        }

       
        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            _logger.LogInformation("AddEvent method call started.");

            
            if (timeSlotDto == null)
            {
                return BadRequest("Event data is null.");
            }

            var tenantName = TenantName;
            var tenanatResponse = await _informationService.GetTenantPageNameForUserAsync(tenantName);
            if (tenanatResponse.Success)
            {
                timeSlotDto.TenantId = tenanatResponse.Data.Id;

            }
            var userEmail = UserEmail;
            var userResponse = await _userSercvice.GetUserIdByEmail(userEmail);
            if (userResponse.Success)
            {
                timeSlotDto.UserId = userResponse.Data;
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
            _logger.LogInformation("AddEvent method call ended.");
            return BadRequest(ModelState); // Return a bad request response with validation errors

        }


        //[HttpGet("EditTimeSlotUser")]
        //public async Task<IActionResult> EditTimeSlotUser(string id)
        //{

        //    var response = await _userSercvice.GetUsersbyTimeSlotId(int.Parse(id));
        //    var userDtos = new List<UserDto>(); 
        //    return View(response.Data.ToList()); // Handle the error case appropriately
        //}

        [HttpGet("EditTimeSlotUser")]
        public async Task<IActionResult> EditTimeSlotUser(string id)
        {
            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int timeSlotId))
            {
                return BadRequest("Invalid time slot ID.");
            }

            var response = await _userSercvice.GetUsersbyTimeSlotId(timeSlotId);

            _logger.LogInformation("EditTimeSlotUser method call started.");
            if (response.Success && response.Data != null)
            {
                return View(response.Data.ToList()); // Ensure that you handle the error case appropriately
            }
            else
            {
                return NotFound("Users not found for the specified time slot.");
            }
        }


        private long ToUnixTimestamp(DateTime dateTime)
        {
            _logger.LogInformation("ToUnixTimestamp method call started.");
            _logger.LogInformation("ToUnixTimestamp method call ended.");
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

        }

        [HttpPost]
        public async Task<IActionResult> SaveEvent(TimeSlotDto timeSlotDto)
        {
            _logger.LogInformation("SaveEvent method call started.");

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

            _logger.LogInformation("SaveEvent method call ended.");

            return BadRequest();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            _logger.LogInformation("DeleteEvent method call started.");

            if (ModelState.IsValid)
            {
                await _timeSlotService.DeleteTimeSlotAsync(timeSlotDto.Id);
                return Ok();
            }
            _logger.LogInformation("DeleteEvent method call ended.");

            return BadRequest();
        }

        // Helper method to convert DateTimeOffset to Unix timestamp (seconds)
        private long ToUnixTimestamp(DateTimeOffset dateTime)
        {
            _logger.LogInformation("ToUnixTimestamp method call started.");
            _logger.LogInformation("ToUnixTimestamp method call ended.");
            return dateTime.ToUnixTimeSeconds();
        }


        [HttpPost("GetTimeSlotId")]
        public async Task<IActionResult> GetTimeSlotId([FromBody] TimeSlotDto request)
        {
            _logger.LogInformation("GetTimeSlotId method call started.");
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            DateTime startTime;
            DateTime endTime;
            try
            {
                startTime = DateTime.Parse(request.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                endTime = DateTime.Parse(request.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                TempData["startTime"] = startTime;

            }
            catch (FormatException ex)
            {
                _logger.LogCritical($"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return BadRequest("Invalid date format.");
            }

            var timeSlotId = await _timeSlotService.GetTimeSlotIdAsync(startTime, endTime, request.TimeSlotName);

            if (timeSlotId.HasValue)
            {
                _logger.LogInformation("GetTimeSlotId method call ended.");
                return Ok(new { timeSlotId = timeSlotId });
            }
            else
            {
                return NotFound("Time slot not found.");
            }
        }


        [HttpGet("Calendar")]
        public async Task<IActionResult> Calendar()
        {
            var tenantName = HttpContext.Items["TenantName"] as string;
            _logger.LogInformation("Calendar page accessed");
            _logger.LogInformation("Calendar method call started.");


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
            _logger.LogInformation("Calendar method call ended.");
            return View(model);
        }

        private DateTimeOffset GetStartOfWeek(DateTimeOffset dateTime)
        {
            _logger.LogInformation("GetStartOfWeek method call started.");

            int diff = (int)dateTime.DayOfWeek - (int)DayOfWeek.Sunday;

            _logger.LogInformation("GetStartOfWeek method call ended.");

            return dateTime.AddDays(-diff).Date;

        }


        [HttpGet("UserCalendar")]
        public async Task<IActionResult> UserCalendar()
        {
            _logger.LogInformation("Calendar page accessed");
        // Helper method to get the start of the current week (Sunday)
        
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

            var model = new UserCalendarDto
            {
                Events = calendarEvents
            };

            return View(model);
        }





    }

}


    


   



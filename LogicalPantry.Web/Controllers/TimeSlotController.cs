using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc;
using Tweetinvi.Core.Events;
using LogicalPantry.Services.InformationService;
using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authorization;
using LogicalPantry.Web.Helper;
using Newtonsoft.Json;

namespace LogicalPantry.Web.Controllers
{
    /// <summary>
    /// Handles time slot management actions for events within the application.
    /// </summary>
    [Route("TimeSlot")]
    public class TimeSlotController : BaseController
    {

        private readonly ILogger<TimeSlotController> _logger;
        private readonly ITimeSlotService _timeSlotService;
        private readonly IUserService _userSercvice;
        private readonly IInformationService _informationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSlotController"/> class.
        /// </summary>
        /// <param name="logger">Logger for logging controller actions.</param>
        /// <param name="timeSlotService">Service for handling time slot operations.</param>
        /// <param name="userService">Service for handling user operations.</param>
        /// <param name="informationService">Service for retrieving tenant information.</param>
        public TimeSlotController(ILogger<TimeSlotController> logger, ITimeSlotService timeSlotService, IUserService userService, IInformationService informationService)
        {
            _logger = logger;
            _timeSlotService = timeSlotService;
            _userSercvice = userService;
            _informationService = informationService;
        }

        /// <summary>
        /// Adds a new time slot event for the specified tenant.
        /// </summary>
        /// <param name="timeSlotDto">The time slot event data to be added.</param>
        /// <returns>A JSON response indicating the success or failure of the operation.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("AddEvent")]
        public async Task<IActionResult> AddEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            _logger.LogInformation("AddEvent method call started.");

            if (timeSlotDto == null)
            {
                return BadRequest("Event data is null.");
            }

            var tenantName = TenantName;
            var tenantResponse = await _informationService.GetTenantPageNameForUserAsync(tenantName);
            if (tenantResponse.Success)
            {
                timeSlotDto.TenantId = tenantResponse.Data.Id;
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
                    // return Redirect($"/{tenantName}/TimeSlot/Calendar");
                    // Return a JSON response indicating success
                    return Json(new { success = true });
                    // return Ok(success);
                    //return View();
                }
                else
                {
                    // Return a server error response
                    return StatusCode(500, "An error occurred while saving the time slot. Please try again.");
                }
            }
            else
            {
                _logger.LogInformation("AddEvent method call ended.");
                return BadRequest(ModelState); // Return a bad request response with validation errors
            }
        }


        /// <summary>
        /// Edits the users associated with a specific time slot based on the time slot ID.
        /// </summary>
        /// <param name="Id">The ID of the time slot to retrieve users for.</param>
        /// <returns>Returns a view with a list of users associated with the time slot.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpGet("EditTimeSlotUser")]
        public async Task<IActionResult> EditTimeSlotUser(int Id)
        {
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            ViewBag.PageName = TenantDisplayName;
            // Log the starting of the Index method execution.
            _logger.LogInformation("EditTimeSlotUser Get method call started");
            if (Id == 0)
            {
                return BadRequest("Invalid time slot.");
            }

            //Added by kunal karne for getting time slot based on Id - 17-12-2024.
            if (Id != null)
            {
                //Getting timeslot based on Id.
                var timeslot = _timeSlotService.GetTimeSlotById(Id);

                if (timeslot != null)
                {
                    var startdate = timeslot.Result.StartTime;
                    var endDate = timeslot.Result.EndTime;

                    //Save StartDate time and end time in tempData.
                    string startDateTime = startdate.ToString("MMM dd hh:mm tt");
                    string Endtime = endDate.ToString("h:mm tt");
                    TempData["startTime"] = startDateTime;
                    TempData["endTime"] = Endtime;
                }
            }
            //End by kunal karne

            var response = await _userSercvice.GetUsersbyTimeSlotId(Id);

            // Serialize the response object to JSON
            string responseJson = JsonConvert.SerializeObject(response, Formatting.Indented);

            // Log the serialized response
            _logger.LogInformation("Retrieved user: {ResponseJson}", responseJson);

            if (response.Success && response.Data != null)
            {
                //Log the ending of the Index method execution.
                _logger.LogInformation("EditTimeSlotUser method call started.");
                @TempData["MessageClass"] = "alert-success";
                @TempData["SuccessMessageBatch"] = "Changes Saved Successfully";
                //return View(response.Data.ToList()); 
                //return Redirect(url);
                return View(response.Data.ToList());
            }
            else
            {
                return View(response.Data.ToList());
                //return NotFound("Users not found for the specified time slot.");
            }

            //var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            //ViewBag.PageName = TenantDisplayName;
            //// Log the starting of the Index method execution.
            //_logger.LogInformation("EditTimeSlotUser Get method call started");
            //if (Id == 0)
            //{
            //    return BadRequest("Invalid time slot.");
            //}

            ////Added by kunal karne for getting time slot based on Id - 17-12-2024.
            //if (Id != null)
            //{
            //    //Getting timeslot based on Id.
            //    var timeslot = _timeSlotService.GetTimeSlotById(Id);

            //    if (timeslot != null)
            //    {
            //        var startdate = timeslot.Result.StartTime;
            //        var endDate = timeslot.Result.EndTime;

            //        //Save StartDate time and end time in tempData.
            //        string startDateTime = startdate.ToString("MMM dd hh:mm tt");
            //        string Endtime = endDate.ToString("h:mm tt");
            //        TempData["startTime"] = startDateTime;
            //        TempData["endTime"] = Endtime;
            //    }
            //}
            ////End by kunal karne

            //var response = await _userSercvice.GetUsersbyTimeSlotId(Id);

            //// Serialize the response object to JSON
            //string responseJson = JsonConvert.SerializeObject(response, Formatting.Indented);

            //// Log the serialized response
            //_logger.LogInformation("Retrieved user: {ResponseJson}", responseJson);

            //if (response.Success && response.Data != null)
            //{
            //    //Log the ending of the Index method execution.
            //    _logger.LogInformation("EditTimeSlotUser method call started.");
            //    @TempData["MessageClass"] = "alert-success";
            //    @TempData["SuccessMessageBatch"] = "Changes Saved Successfully";
            //    //return View(response.Data.ToList()); 
            //    //return Redirect(url);
            //    return View(response.Data.ToList());
            //}
            //else
            //{
            //    return View(response.Data.ToList());
            //    //return NotFound("Users not found for the specified time slot.");
            //}

        }                 
        private long ToUnixTimestamp(DateTime dateTime)
        {
            //_logger.LogInformation("ToUnixTimestamp method call started.");
            //_logger.LogInformation("ToUnixTimestamp method call ended.");
            //return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

            
    _logger.LogInformation("ToUnixTimestamp method call started.");
    long timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    _logger.LogInformation("ToUnixTimestamp method call ended.");
    return timestamp;

        }

       


        /// <summary>
        /// Retrieves the ID of a time slot based on the provided time range and time slot name.
        /// </summary>
        /// <param name="request">The request object containing time slot details such as StartTime, EndTime, and TimeSlotName.</param>
        /// <returns>Returns the time slot ID and the maximum number of users for the time slot if found, or an error message if not.</returns>
        [Authorize(Roles = $"{UserRoleEnum.Admin},{UserRoleEnum.User}")]
        [HttpPost("GetTimeSlotId")]
        public async Task<IActionResult> GetTimeSlotId([FromBody]TimeSlotDto request)
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


                _logger.LogInformation($"Event details: Start={startTime}, End={endTime}, Title={request.TimeSlotName}");

            }
            catch (FormatException ex)
            {
                _logger.LogCritical($"Internal server error: {ex.Message}, Stack Trace: {ex.StackTrace}");
                return BadRequest("Invalid date format.");
            }

            var timeSlotDetails = await _timeSlotService.GetTimeSlotDetailsAsync(startTime, endTime, request.TimeSlotName);


            if (timeSlotDetails != null)
            {
                _logger.LogInformation("GetTimeSlotId method call ended.");
                return Ok(new
                {
                    timeSlotId = timeSlotDetails.Id,
                    maxUsers = timeSlotDetails.MaxNumberOfUsers
                });
            }
            else
            {
                return NotFound("Time slot not found.");
            }
        }


        /// <summary>
        /// Displays the calendar page for administrators, including all events for the current tenant.
        /// </summary>
        /// <returns>Returns a view containing the calendar with events for the tenant.</returns>
        /// 
        [Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpGet("Calendar")]
        public async Task<IActionResult> Calendar()
        {
            var tenantName = HttpContext.Items["TenantName"] as string;
            _logger.LogInformation("Calendar page accessed");
            _logger.LogInformation("Calendar method call started.");
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");

            //ViewBag.TenantId = tenantId;
            ViewBag.PageName = TenantDisplayName;

            ViewBag.TenantId = TenantId;
            //ViewBag.PageName = PageName;
            var tenantId = TenantId;
            // Fetch events from the database
            // var events = await _timeSlotService.GetAllEventsAsync();
            var events = await _timeSlotService.GetAllEventsByTenantIdAsync(tenantId.Value);
            _logger.LogInformation("{@events}", events);
            // Log the number of events fetched
            _logger.LogInformation($"Fetched {events.Count()} events from the database.");

            //// Log details of each event
            foreach (var e in events)
            {
                _logger.LogInformation($"Event: Start={e.StartTime}, End={e.EndTime}, Title={e.TimeSlotName}, Category={e.EventType}");
            }

            // Map to your event model
            var calendarEvents = events.Select(e => new Event
            {
                Start = ToUnixTimestamp(e.StartTime),
                End = ToUnixTimestamp(e.EndTime),
                Title = e.TimeSlotName,
                Category = e.EventType 
            }).ToList();

            _logger.LogInformation("{@message}", calendarEvents);
 
            // Log the number of events mapped
            _logger.LogInformation($"Mapped {calendarEvents.Count} events to calendar event model.");

            var model = new CalendarViewModel
            {
                Events = calendarEvents
            };
            _logger.LogInformation("{@message}", model);


            _logger.LogInformation("Calendar method call ended.");
            return View(model);
        }


        /// <summary>
        /// Displays the user-specific calendar page, including all events for the current tenant.
        /// </summary>
        /// <returns>Returns a view containing the user calendar with events for the tenant.</returns>
        [Authorize(Roles = $"{UserRoleEnum.User}")]         
        [HttpGet("UserCalendar")]
        public async Task<IActionResult> UserCalendar()
        {
            _logger.LogInformation("Calendar page accessed");
            // Helper method to get the start of the current week (Sunday)
            var TenantDisplayName = HttpContext.Session.GetString("TenantDisplayName");
            ViewBag.PageName = TenantDisplayName;
            var tenantId = TenantId;
            // Fetch events from the database
           // var events = await _timeSlotService.GetAllEventsAsync();

            var events = await _timeSlotService.GetAllEventsByTenantIdAsync(tenantId.Value);
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
                Category = e.EventType
            }).ToList();

            // Log the number of events mapped
            _logger.LogInformation($"Mapped {calendarEvents.Count} events to calendar event model.");

            var model = new UserCalendarDto
            {
                Events = calendarEvents
            };

            return View(model);
        }




        /// <summary>
        /// Updates the details of an existing time slot event.
        /// </summary>
        /// <param name="timeSlotDto">The updated time slot event data.</param>
        /// <returns>A JSON response indicating the success or failure of the operation.</returns>
        //[Authorize(Roles = $"{UserRoleEnum.Admin}")]
        [HttpPost("UpdateEvent")]
        public async Task<IActionResult> UpdateEvent([FromBody] TimeSlotDto timeSlotDto)
        {
            _logger.LogInformation("UpdateEvent method call started.");

            if (timeSlotDto == null || timeSlotDto.Id == 0)
            {
                return BadRequest("Invalid event data.");
            }

            if (ModelState.IsValid)
            {
                // Call the service to update the event
                var success = await _timeSlotService.UpdateTimeSlotAsync(timeSlotDto);

                string responseJson = JsonConvert.SerializeObject(success, Formatting.Indented);
                // Log the serialized response
                _logger.LogInformation("Update Event Success Message: {ResponseJson}", responseJson);

                if (success)
                {
                    _logger.LogInformation("Event updated successfully.");
                    return Json(new { success = true });
                }
                else
                {
                    _logger.LogError("An error occurred while updating the event.");
                    return StatusCode(500, "An error occurred while updating the event. Please try again.");
                }
            }

            _logger.LogInformation("UpdateEvent method call ended.");
            return BadRequest(ModelState); // Return a bad request response with validation errors
        }


    }

}


    


   



using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.Services.TimeSlotServices;
using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    [Route("TimeSlot")]
    public class TimeSlotController : Controller
    {
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        // POST: TimeSlot/AddTimeSlot
        [HttpPost("AddTimeSlot")]
        public async Task<IActionResult> AddTimeSlot([FromBody] TimeSlotDto timeSlotDto)
        {
            if (timeSlotDto == null)
            {
                return BadRequest("Invalid time slot data.");
            }

            var result = await _timeSlotService.AddTimeSlotAsync(timeSlotDto);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetTimeSlot), new { id = result.Data.Id }, result.Data);
            }

            return BadRequest(result.Message);
        }

        // GET: TimeSlot/GetTimeSlots
        [HttpGet("GetTimeSlots")]
        public async Task<IActionResult> GetTimeSlots()
        {
            var result = await _timeSlotService.GetTimeSlotsAsync();

            if (result.Success)
            {
                return Ok(result.Data);
            }

            return NotFound(result.Message);
        }

        // GET: api/TimeSlot/GetTimeSlot/{id}
        [HttpGet("GetTimeSlot/{id}")]
        public async Task<IActionResult> GetTimeSlot(int id)
        {
            var result = await _timeSlotService.GetTimeSlotByIdAsync(id);

            //if (result.Success)
            //{
            //    return Ok(result.Data);
            //}

            return Ok(result);
        }

        // PUT: api/TimeSlot/EditTimeSlot
        [HttpPut("EditTimeSlot")]
        public async Task<IActionResult> EditTimeSlot([FromBody] TimeSlotDto timeSlotDto)
        {
            if (timeSlotDto == null)
            {
                return BadRequest("Invalid time slot data.");
            }

            var result = await _timeSlotService.UpdateTimeSlotAsync(timeSlotDto);

            if (result.Success)
            {
                return NoContent();
            }

            return BadRequest(result.Message);
        }

        // DELETE: api/TimeSlot/DeleteTimeSlot/{id}
        [HttpDelete("DeleteTimeSlot/{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var result = await _timeSlotService.DeleteTimeSlotAsync(id);

            if (result.Success)
            {
                return NoContent();
            }

            return BadRequest(result.Message);
        }
    }
}

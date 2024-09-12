using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogicalPantry.Services.TimeSlotServices
{
    public class TimeSlotService :ITimeSlotService
    {
        private readonly ApplicationDataContext _context;

        public TimeSlotService(ApplicationDataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all time slots.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="TimeSlotDto"/>.</returns>
        public async Task<List<TimeSlotDto>> GetTimeSlotsAsync()
        {
            //getting all time slots
            return await _context.TimeSlots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                UserId = ts.UserId,
                TenantId = ts.TenantId,
                TimeSlotName = ts.TimeSlotName,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime
            }).ToListAsync();
        }


        /// <summary>
        /// Adds a new time slot to the database.
        /// </summary>
        /// <param name="timeSlotDto">The data transfer object containing the time slot information to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating the success of the operation.</returns>
        public async Task<bool> AddTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            try
            {
                // Check for overlapping time slots
                var overlappingTimeSlot = await _context.TimeSlots
                    .Where(ts => ts.TenantId == timeSlotDto.TenantId
                                && ts.UserId == timeSlotDto.UserId
                                && ts.StartTime.Date == timeSlotDto.StartTime.Date
                                && (ts.StartTime < timeSlotDto.EndTime && ts.EndTime > timeSlotDto.StartTime))
                    .FirstOrDefaultAsync();


                if (overlappingTimeSlot != null)
                {
                    throw new InvalidOperationException("This time slot overlaps with an existing time slot on the same day.");
                }

                var timeSlot = new TimeSlot
                {
                    TimeSlotName = timeSlotDto.TimeSlotName,
                    StartTime = timeSlotDto.StartTime,
                    EndTime = timeSlotDto.EndTime,
                    UserId = timeSlotDto.UserId,
                    TenantId = timeSlotDto.TenantId,
                    EventType = timeSlotDto.EventType,
                    MaxNumberOfUsers = timeSlotDto.MaxNumberOfUsers,    
                };

                //add time slot here
                _context.TimeSlots.Add(timeSlot);
                await _context.SaveChangesAsync();

                return true; // Indicate that the operation was successful
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.Error.WriteLine(ex);

                // Return false to indicate failure
                return false;
            }
        }

        /// <summary>
        /// Updates an existing time slot with the provided data.
        /// </summary>
        /// <param name="timeSlotDto">The data transfer object containing the updated time slot information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(timeSlotDto.Id);
            if (timeSlot != null)
            {
                timeSlot.TimeSlotName = timeSlotDto.TimeSlotName;
                timeSlot.StartTime = timeSlotDto.StartTime;
                timeSlot.EndTime = timeSlotDto.EndTime;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves all events from the time slots table.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of <see cref="TimeSlot"/>.</returns>
        public async Task<IEnumerable<TimeSlot>> GetAllEventsAsync()
        {
            // Fetch events from the TimeSlot table
            return await _context.TimeSlots.ToListAsync();
        }

        /// <summary>
        /// Retrieves all events for a specific tenant by tenant ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for which to retrieve events.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of <see cref="TimeSlot"/>.</returns>
        public async Task<IEnumerable<TimeSlot>> GetAllEventsByTenantIdAsync(int tenantId)
        {
            // Fetch events from the TimeSlot table that match the given tenantId
            return await _context.TimeSlots
                .Where(ts => ts.TenantId == tenantId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves detailed information for a time slot based on the provided start and end times and title.
        /// </summary>
        /// <param name="startTime">The start time of the time slot (nullable).</param>
        /// <param name="endTime">The end time of the time slot (nullable).</param>
        /// <param name="title">The title of the time slot.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="TimeSlotDto"/> with the time slot details, or <c>null</c> if not found.</returns>
        public async Task<TimeSlotDto> GetTimeSlotDetailsAsync(DateTime? startTime, DateTime? endTime, string title)
        {
            // Check if any parameter is null or empty
            if (!startTime.HasValue || !endTime.HasValue || string.IsNullOrWhiteSpace(title))
            {           
                return null; // Or handle the error according to your needs
            }

            var timeSlot = await _context.TimeSlots
                .Where(ts => ts.StartTime == startTime.Value && ts.EndTime == endTime.Value && ts.TimeSlotName == title)
                .Select(ts => new TimeSlotDto
                {
                    Id = ts.Id,
                    MaxNumberOfUsers = ts.MaxNumberOfUsers // Assuming you have a MaxUsers property in TimeSlot
                })
                .FirstOrDefaultAsync();

            return timeSlot;
        }

    }





}


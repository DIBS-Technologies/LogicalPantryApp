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

        public async Task<List<TimeSlotDto>> GetTimeSlotsAsync()
        {
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

        public async Task AddTimeSlot1Async(TimeSlotDto timeSlotDto)
        {
            var timeSlot = new TimeSlot
            {
                UserId = timeSlotDto.UserId,
                TenantId = timeSlotDto.TenantId,
                TimeSlotName = timeSlotDto.TimeSlotName,
                StartTime = timeSlotDto.StartTime,
                EndTime = timeSlotDto.EndTime
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();
        }




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

        public async Task DeleteTimeSlotAsync(long id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot != null)
            {
                _context.TimeSlots.Remove(timeSlot);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TimeSlot>> GetAllEventsAsync()
        {
            // Fetch events from the TimeSlot table
            return await _context.TimeSlots.ToListAsync();
        }

        public async Task<IEnumerable<TimeSlot>> GetAllEventsByTenantIdAsync(int tenantId)
        {
            // Fetch events from the TimeSlot table that match the given tenantId
            return await _context.TimeSlots
                .Where(ts => ts.TenantId == tenantId)
                .ToListAsync();
        }



        //public async Task<int?> GetTimeSlotIdAsync(DateTime startTime, DateTime endTime, string title)
        //{
        //    var timeSlot = await _context.TimeSlots
        //        .Where(ts => ts.StartTime == startTime && ts.EndTime == endTime && ts.TimeSlotName == title)
        //        .FirstOrDefaultAsync();


        //    return timeSlot?.Id;
        //}

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


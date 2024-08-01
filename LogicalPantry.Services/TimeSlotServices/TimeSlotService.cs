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




        public async Task AddTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var timeSlot = new TimeSlot
            {
                TimeSlotName = timeSlotDto.TimeSlotName,
                StartTime = timeSlotDto.StartTime,
                EndTime = timeSlotDto.EndTime
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();
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
















    }





}


using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TimeSlotServices
{
    public class TimeSlotService :ITimeSlotService
    {
        private readonly ApplicationDataContext _context;

        public TimeSlotService(ApplicationDataContext context)
        {
            _context = context;
        }

        public async Task<TimeSlot> GetTimeSlotByIdAsync(int timeSlotId)
        {
            return await _context.TimeSlots
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.Id == timeSlotId);
        }

        public async Task<ServiceResponse<TimeSlotDto>> AddTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var response = new ServiceResponse<TimeSlotDto>();

            try
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

                response.Data = new TimeSlotDto
                {
                    Id = timeSlot.Id,
                    UserId = timeSlot.UserId,
                    TenantId = timeSlot.TenantId,
                    TimeSlotName = timeSlot.TimeSlotName,
                    StartTime = timeSlot.StartTime,
                    EndTime = timeSlot.EndTime
                };

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error adding time slot: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<TimeSlotDto>>> GetTimeSlotsAsync()
        {
            var response = new ServiceResponse<List<TimeSlotDto>>();

            try
            {
                var timeSlots = await _context.TimeSlots.ToListAsync();
                response.Data = timeSlots.Select(ts => new TimeSlotDto
                {
                    Id = ts.Id,
                    UserId = ts.UserId,
                    TenantId = ts.TenantId,
                    TimeSlotName = ts.TimeSlotName,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime
                }).ToList();

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving time slots: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<TimeSlotDto>> GetTimeSlotByIdAsync1(int id)
        {
            var response = new ServiceResponse<TimeSlotDto>();

            try
            {
                var timeSlot = await _context.TimeSlots.FindAsync(id);
                if (timeSlot != null)
                {
                    response.Data = new TimeSlotDto
                    {
                        Id = timeSlot.Id,
                        UserId = timeSlot.UserId,
                        TenantId = timeSlot.TenantId,
                        TimeSlotName = timeSlot.TimeSlotName,
                        StartTime = timeSlot.StartTime,
                        EndTime = timeSlot.EndTime
                    };
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Time slot not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving time slot: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> UpdateTimeSlotAsync(TimeSlotDto timeSlotDto)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var timeSlot = await _context.TimeSlots.FindAsync(timeSlotDto.Id);
                if (timeSlot != null)
                {
                    timeSlot.UserId = timeSlotDto.UserId;
                    timeSlot.TenantId = timeSlotDto.TenantId;
                    timeSlot.TimeSlotName = timeSlotDto.TimeSlotName;
                    timeSlot.StartTime = timeSlotDto.StartTime;
                    timeSlot.EndTime = timeSlotDto.EndTime;

                    _context.TimeSlots.Update(timeSlot);
                    await _context.SaveChangesAsync();

                    response.Data = true;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Time slot not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating time slot: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteTimeSlotAsync(int id)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var timeSlot = await _context.TimeSlots.FindAsync(id);
                if (timeSlot != null)
                {
                    _context.TimeSlots.Remove(timeSlot);
                    await _context.SaveChangesAsync();

                    response.Data = true;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Time slot not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting time slot: {ex.Message}";
            }

            return response;
        }
    }
}

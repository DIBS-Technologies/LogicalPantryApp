using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TimeSlotServices
{
    public interface ITimeSlotService
    {
        Task<TimeSlot> GetTimeSlotByIdAsync(int timeSlotId);
        Task<ServiceResponse<TimeSlotDto>> AddTimeSlotAsync(TimeSlotDto timeSlotDto);
        Task<ServiceResponse<List<TimeSlotDto>>> GetTimeSlotsAsync();
        Task<ServiceResponse<bool>> UpdateTimeSlotAsync(TimeSlotDto timeSlotDto);
        Task<ServiceResponse<bool>> DeleteTimeSlotAsync(int id);
    }
}

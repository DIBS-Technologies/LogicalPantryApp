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
        Task<List<TimeSlotDto>> GetTimeSlotsAsync();
        //Task AddTimeSlotAsync1(TimeSlotDto timeSlotDto);

        Task DeleteTimeSlotAsync(long id);

        Task UpdateTimeSlotAsync(TimeSlotDto timeSlotDto);

        Task<bool> AddTimeSlotAsync(TimeSlotDto timeSlotDto);

        Task<IEnumerable<TimeSlot>> GetAllEventsAsync();

        Task<int?> GetTimeSlotIdAsync(DateTime startTime, DateTime endTime, string title);
    }
}

using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicalPantry.DTOs.UserDtos;

namespace LogicalPantry.Services.TimeSlotServices
{
    public interface ITimeSlotService
    {
        /// <summary>
        /// Retrieves a list of all time slots.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="TimeSlotDto"/>.</returns>
        Task<List<TimeSlotDto>> GetTimeSlotsAsync();

        /// <summary>
        /// Updates an existing time slot with the provided data.
        /// </summary>
        /// <param name="timeSlotDto">The data transfer object containing updated time slot information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<bool> UpdateTimeSlotAsync(TimeSlotDto timeSlotDto);

        /// <summary>
        /// Adds a new time slot with the provided data.
        /// </summary>
        /// <param name="timeSlotDto">The data transfer object containing the new time slot information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> AddTimeSlotAsync(TimeSlotDto timeSlotDto);

        /// <summary>
        /// Retrieves all events.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of <see cref="TimeSlot"/>.</returns>
        Task<IEnumerable<TimeSlot>> GetAllEventsAsync();

        /// <summary>
        /// Retrieves all time slots for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for which to retrieve time slots.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of <see cref="TimeSlot"/>.</returns>
        Task<IEnumerable<TimeSlot>> GetAllEventsByTenantIdAsync(int tenantId);

        /// <summary>
        /// Retrieves detailed information for a time slot based on the provided start and end times and title.
        /// </summary>
        /// <param name="startTime">The start time of the time slot (nullable).</param>
        /// <param name="endTime">The end time of the time slot (nullable).</param>
        /// <param name="title">The title of the time slot.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="TimeSlotDto"/> with the time slot details.</returns>
        Task<TimeSlotDto> GetTimeSlotDetailsAsync(DateTime? startTime, DateTime? endTime, string title);



        /// <summary>
        /// Retrieves detailed information for a time slot based on the provided ID.
        /// </summary>
        /// <param name="Id">The Id of the time slot.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="TimeSlotDto"/> with the time slot details.</returns>
        Task<TimeSlotDto> GetTimeSlotById(int id);


    }
}

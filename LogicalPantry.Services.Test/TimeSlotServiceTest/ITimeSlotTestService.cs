using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TimeSlotDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TimeSlotServiceTest
{
    public interface ITimeSlotTestService
    {
        /// <summary>
        /// Retrieves a time slot event based on the provided time slot data transfer object (DTO).
        /// </summary>
        /// <param name="timeSlotDto">The time slot data transfer object containing event details.</param>
        /// <returns>A service response containing the time slot DTO and the result of the operation.</returns>
        Task<ServiceResponse<TimeSlotDto>> GetEvent(TimeSlotDto timeSlotDto);
    }
}

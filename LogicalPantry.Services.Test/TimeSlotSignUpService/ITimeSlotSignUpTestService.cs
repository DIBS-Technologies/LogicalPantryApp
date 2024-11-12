
using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TimeSlotSignupDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TimeSlotSignUpService
{
    public interface ITimeSlotSignUpTestService
    {
        /// <summary>
        /// Retrieves information about a specific time slot sign-up based on the provided time slot sign-up data transfer object (DTO).
        /// </summary>
        /// <param name="timeSlotSignupDto">The time slot sign-up data transfer object containing sign-up details.</param>
        /// <returns>A service response containing the time slot sign-up DTO and the result of the operation.</returns>
        Task<ServiceResponse<TimeSlotSignupDto>> GetTimeSlot(TimeSlotSignupDto timeSlotSignupDto);
    }
}

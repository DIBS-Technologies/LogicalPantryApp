using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TimeSlotSignupService
{
    public interface ITimeSlotSignupService
    {
        /// <summary>
        /// Retrieves a list of users who have signed up for a specific time slot.
        /// </summary>
        /// <param name="timeslot">The date and time of the time slot.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of users signed up for the specified time slot.</returns>
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUserbyTimeSlot(DateTime timeslot);

        /// <summary>
        /// Posts a list of time slot sign-ups.
        /// </summary>
        /// <param name="users">A list of <see cref="TimeSlotSignupDto"/> objects representing the sign-ups to be posted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ServiceResponse{string}"/> with a message indicating the result of the operation.</returns>
        Task<ServiceResponse<string>> PostTimeSlotSignup(List<TimeSlotSignupDto> users);

        /// <summary>
        /// Adds a time slot sign-up entry.
        /// </summary>
        /// <param name="dto">The <see cref="TimeSlotSignupDto"/> object containing the details of the sign-up to be added.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a tuple containing a boolean indicating success and a string message with the result of the operation.</returns>
        Task<(bool success, string message)> AddTimeSlotSignUp(TimeSlotSignupDto dto);
    }
}

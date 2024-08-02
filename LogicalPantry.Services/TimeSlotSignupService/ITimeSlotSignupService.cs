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
        Task<ServiceResponse< IEnumerable<UserDto>>> GetUserbyTimeSlot(DateTime timeslot);
        Task<ServiceResponse<string>> PostTimeSlotSignup(List<TimeSlotSignupDto> users);
    }
}

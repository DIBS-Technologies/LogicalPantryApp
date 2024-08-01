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
        List<UserDto> GetUserbyTimeSlot(DateTime timeslot);
        string PostTimeSlotSignup(List<TimeSlotSignupDto> users);
    }
}

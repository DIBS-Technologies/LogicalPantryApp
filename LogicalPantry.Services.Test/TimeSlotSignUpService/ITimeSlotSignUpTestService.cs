using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TimeSlotSignUpService
{
    public interface ITimeSlotSignUpTestService
    {
        Task<ServiceResponse<TimeSlotSignupDto>> GetTimeSlot(TimeSlotSignupDto timeSlotSignupDto);
    }
}

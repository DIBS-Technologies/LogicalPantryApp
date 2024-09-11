
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
        Task<ServiceResponse<TimeSlotSignupDto>> GetTimeSlot(TimeSlotSignupDto timeSlotSignupDto);
    }
}

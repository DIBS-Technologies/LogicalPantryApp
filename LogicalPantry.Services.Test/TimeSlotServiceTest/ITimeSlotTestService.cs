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

        Task<ServiceResponse<TimeSlotDto>> GetEvent(TimeSlotDto timeSlotDto);
    }
}

using LogicalPantry.DTOs.TenantDtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.TimeSlotDtos
{
    //public class CalendarViewModel
    //{

    //        public List<TimeSlotDto> TimeSlotDto { get; set; }

    //}

    public class CalendarViewModel
    {
        public List<Event> Events { get; set; }        
    }
   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.TimeSlotSignupDtos
{
    public class TimeSlotSignupDto
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public int UserId { get; set; }
        public bool Attended { get; set; }


    }
}

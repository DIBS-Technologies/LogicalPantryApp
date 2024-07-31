using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.TimeSlotSignupDtos
{
    public class TimeSlotSignupViewModel
    {
        public int TimeSlotId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public List<UserDto> Users { get; set; }
        public List<TimeSlotSignupDto> TimeSlotSignups { get; set; }
        public string Email { get; set; } // Add this property to hold the email address
        public bool Attended { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.Test.UserDtos
{
    public class UserAttendedDto
    {

        public int Id { get; set; }
        public bool IsAttended { get; set; }
        public int TimeSlotId { get; set; } // TimeSlotId
    }
}

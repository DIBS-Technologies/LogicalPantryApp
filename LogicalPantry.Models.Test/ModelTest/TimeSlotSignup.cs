using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Models.Test.ModelTest
{
    public class TimeSlotSignup
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool Attended { get; set; }

    }
}

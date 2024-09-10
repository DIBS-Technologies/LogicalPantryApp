using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Models.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string TimeSlotName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string EventType { get; set; } 
        public int MaxNumberOfUsers { get; set; } 

        public ICollection<TimeSlotSignup> TimeSlotSignups { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.DTOs.TimeSlotDtos
{
    public class TimeSlotDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string TimeSlotName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string EventType { get; set; }
        public int MaxNumberOfUsers { get; set; }
    }
}

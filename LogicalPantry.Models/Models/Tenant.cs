using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Models.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string TenantName { get; set; }
        public string AdminEmail { get; set; }
        public string? TenantDisplayName { get; set; }
        public string? PaypalId { get; set; }
        public string? PageName { get; set; }
        public string? Logo { get; set; }
        public string? Timezone { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<TimeSlot> TimeSlots { get; set; }
    }
}

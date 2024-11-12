using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Models.Test.ModelTest
{
    public class User
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsAllow { get; set; }
        public bool IsRegistered { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<TimeSlot> TimeSlots { get; set; }
        public ICollection<TimeSlotSignup> TimeSlotSignups { get; set; }
    }
}

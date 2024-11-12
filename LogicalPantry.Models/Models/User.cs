using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Models.Models
{
    public class User
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsAllow { get; set; }
        public bool IsRegistered { get; set; }

        public string? ZipCode { get; set; }    // Zip Code
        public bool? IsMarried { get; set; }    // Married (Yes/No)
        public int? HouseholdSize { get; set; } // Number in Household
        public bool? HasSchoolAgedChildren { get; set; } // School-aged Children (Yes/No)
        public bool? IsVeteran { get; set; }    // Veteran (Yes/No)
        public bool? IsDisabled { get; set; }    // IsDisabled (Yes/No)
        public DateTime? DateOfBirth { get; set; } // Date of Birth (Month/Year)
        public string? EmploymentStatus { get; set; } // Employment Status Dropdown
        public string? ProfilePictureUrl { get; set; } // URL for the uploaded profile picture

        

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<TimeSlot> TimeSlots { get; set; }
        public ICollection<TimeSlotSignup> TimeSlotSignups { get; set; }
    }
}

using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TimeSlotSignupService
{
    public class TimeSlotSignupService: ITimeSlotSignupService
    {
        private readonly ApplicationDataContext _context;
        public TimeSlotSignupService(ApplicationDataContext context)
        {
            _context = context;
        }
        public List<UserDto> GetUserbyTimeSlot(DateTime timeslot)
        {
            try
            {
                List<UserDto> userDtos = new List<UserDto>();
                 userDtos = (from ts in _context.TimeSlots
                              join u in _context.Users on ts.UserId equals u.Id
                              join t in _context.TimeSlotSignups
                                  on new { ts.UserId } equals new { t.UserId } into tsSignup
                              from t in tsSignup.DefaultIfEmpty()
                              where ts.StartTime == timeslot
                              select new UserDto
                              {
                                  PhoneNumber = u.PhoneNumber,
                                  Email = u.Email,
                                  FullName = u.FullName,
                                  Attended = t != null ? t.Attended : false // Set Attendies based on presence in TimeSlotSignups
                              }).ToList();

                return userDtos;

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public string PostTimeSlotSignup(List<TimeSlotSignupDto> users)
        {
            if (users == null) return string.Empty;
            try
            {
                var timeSlotSignups = users.Select(dto => new TimeSlotSignup
                {
                    TimeSlotId = dto.TimeSlotId,
                    UserId = dto.UserId,
                    Attended = dto.Attended
                }).ToList();

                // Add entities to the context
                _context.TimeSlotSignups.AddRange(timeSlotSignups);

                // Save changes to the database asynchronously
                _context.SaveChangesAsync();

                return "Success"; // Return a success message or result
            }
            catch (Exception)
            {

                throw;
            }
            
        }
    }
}

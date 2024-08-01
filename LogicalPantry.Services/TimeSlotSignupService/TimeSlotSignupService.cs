using AutoMapper;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TimeSlotSignupService
{
    public class TimeSlotSignupService : ITimeSlotSignupService
    {
        private readonly ILogger<TimeSlotSignupService> logger; // Dependency injection for ILogger
        private readonly IMapper mapper; // Dependency injection for IMapper
        private readonly ApplicationDataContext dataContext; // Dependency injection for DataContext

        // Constructor with dependency injection
        public TimeSlotSignupService(ILogger<TimeSlotSignupService> logger, IMapper mapper, ApplicationDataContext dataContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUserbyTimeSlot(DateTime timeslot)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();
            try
            {
                var userDtos = await (from ts in dataContext.TimeSlots
                                      join u in dataContext.Users on ts.UserId equals u.Id
                                      join t in dataContext.TimeSlotSignups
                                          on new { ts.UserId } equals new { t.UserId } into tsSignup
                                      from t in tsSignup.DefaultIfEmpty()
                                      where ts.StartTime == timeslot
                                      select new UserDto
                                      {
                                          PhoneNumber = u.PhoneNumber,
                                          Email = u.Email,
                                          FullName = u.FullName,
                                          Attended = t != null ? t.Attended : false // Set Attended based on presence in TimeSlotSignups
                                      }).ToListAsync();

                response.Data = userDtos;
                response.Success = true;
                response.Message = "Users fetched successfully.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching users by time slot");
                response.Success = false;
                response.Message = $"Error fetching users: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<string>> PostTimeSlotSignup(List<TimeSlotSignupDto> users)
        {
            var response = new ServiceResponse<string>();
            if (users == null || !users.Any())
            {
                response.Success = false;
                response.Message = "No users to update.";
                return response;
            }

            try
            {
                var timeSlotSignups = users.Select(dto => new TimeSlotSignup
                {
                    TimeSlotId = dto.TimeSlotId,
                    UserId = dto.UserId,
                    Attended = dto.Attended
                }).ToList();

                // Add entities to the context
                await dataContext.TimeSlotSignups.AddRangeAsync(timeSlotSignups);

                // Save changes to the database asynchronously
                await dataContext.SaveChangesAsync();

                response.Success = true;
                response.Message = "Time Slot Signup updated successfully.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error posting time slot signups");
                response.Success = false;
                response.Message = $"Error posting time slot signups: {ex.Message}";
            }

            return response;
        }
    }
}

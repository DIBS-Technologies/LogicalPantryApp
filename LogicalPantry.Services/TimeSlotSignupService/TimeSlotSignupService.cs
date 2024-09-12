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


        /// <summary>
        /// Retrieves a list of users who have signed up for a specific time slot.
        /// </summary>
        /// <param name="timeslot">The date and time of the time slot for which to retrieve the list of users.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the following properties:
        /// </returns>
        /// <remarks>
        /// The method performs a left join between time slots, users, and time slot sign-ups to get the relevant user information for the specified time slot.
        /// If a user is not found in the time slot sign-ups, the <see cref="UserDto.Attended"/> property is set to false by default.
        /// </remarks>
        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUserbyTimeSlot(DateTime timeslot)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();
            try
            {
                // Query to get users signed up for the specified time slot
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

                // Set successful response
                response.Data = userDtos;
                response.Success = true;
                response.Message = "Users fetched successfully.";
            }
            catch (Exception ex)
            {
                // Log the exception and set failure response
                logger.LogError(ex, "Error fetching users by time slot");
                response.Success = false;
                response.Message = $"Error fetching users: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Posts a list of time slot sign-ups for users and saves them to the database.
        /// </summary>
        /// <param name="users">A list of <see cref="TimeSlotSignupDto"/> representing the time slot sign-ups.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{string}"/> that indicates the result of the operation.</returns>
        public async Task<ServiceResponse<string>> PostTimeSlotSignup(List<TimeSlotSignupDto> users)
        {
            var response = new ServiceResponse<string>();

            // Validate input
            if (users == null || !users.Any())
            {
                response.Success = false;
                response.Message = "No users to update.";
                return response;
            }

            try
            {
                // Check if time slots and users are valid (optional, based on your requirements)
                // For example: Ensure all TimeSlotIds and UserIds are valid and exist in the database

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
                // Log the error with details
                logger.LogError(ex, "Error posting time slot signups. Data: {TimeSlotSignups}", users);
                response.Success = false;
                response.Message = $"Error posting time slot signups: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Adds a time slot sign-up for a user.
        /// </summary>
        /// <param name="dto">The time slot sign-up data.</param>
        /// <returns>A tuple with a boolean indicating success and a string message.</returns>
        public async Task<(bool success, string message)> AddTimeSlotSignUp(TimeSlotSignupDto dto)
        {
            try
            {
                // Validate input
                if (dto == null)
                {
                    return (false, "Invalid time slot signup data.");
                }

                // Check if the user has already signed up for the same time slot
                var existingSignup = await dataContext.TimeSlotSignups
                    .FirstOrDefaultAsync(s => s.TimeSlotId == dto.TimeSlotId && s.UserId == dto.UserId);

                if (existingSignup != null)
                {
                    return (true, "You have already signed up for this time slot.");
                }

                // Count the number of users signed up for this specific time slot
                var signupCount = await dataContext.TimeSlotSignups
                    .CountAsync(s => s.TimeSlotId == dto.TimeSlotId);

                // Check if the signup limit of 100 users has been reached for this time slot
                if (signupCount >= dto.MaxNumberOfUsers)
                {
                    return (false, $"The signup limit of {dto.MaxNumberOfUsers} users for this time slot has been reached.");
                }

                // Create a new TimeSlotSignup entity
                var timeSlotSignup = new TimeSlotSignup
                {
                    TimeSlotId = dto.TimeSlotId,
                    UserId = dto.UserId,
                    Attended = dto.Attended,
                    
                };

                // Add the new sign-up to the database
                dataContext.TimeSlotSignups.Add(timeSlotSignup);
                await dataContext.SaveChangesAsync();

                return (true, "Successfully signed up for the time slot.");
            }
            catch (Exception ex)
            {
                // Log the exception
                // Example: logger.LogError(ex, "Error adding time slot signup.");

                return (false, "Error adding time slot signup: " + ex.Message);
            }
        }


    }
}

﻿using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.UserDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.UserServiceTest
{
    public class UserServicesTest : IUserServiceTest
    {

        private readonly TestApplicationDataContext _context;
        private readonly IConfiguration _configuration;

        public UserServicesTest(TestApplicationDataContext context)
        {
            var builder = new DbContextOptionsBuilder<TestApplicationDataContext>();


            //Set up configuration to load appsettings json 
            var builderConnectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builderConnectionString.Build();

            var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

            builder.UseSqlServer(connectionString);

            // Initialize dataContext with the configured options
            _context = new TestApplicationDataContext(builder.Options);
        }

        public Task<bool> AddUserAsync(UserDto userDto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check the UpdateUserBatch Successfully
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns>List<TimeSlotSignupDto></returns>

        public async Task<ServiceResponse<List<TimeSlotSignupDto>>> CheckUpdateUserBatch(List<UserDto> userDto)
        {
            var response = new ServiceResponse<List<TimeSlotSignupDto>>();

            try
            {
                if (userDto == null || !userDto.Any())
                {
                    response.Success = false;
                    response.Message = "User list is empty.";
                    return response;
                }

                // Extract unique IDs from the userDto list
                var userIds = userDto.Select(u => u.Id).Distinct().ToList();
                var attended = userDto.Select(u => u.Attended).ToList();

                // Query the database for matching time slot signups
                var timeSlotSignups = await _context.TimeSlotSignups
                    .Where(ts => userIds.Contains(ts.UserId) && attended.Contains(ts.Attended))
                    .ToListAsync();

                if (!timeSlotSignups.Any())
                {
                    response.Success = false;
                    response.Message = "No signups found for the provided user IDs.";
                }
                else
                {
                    response.Success = true;
                    response.Data = timeSlotSignups.Select(ts => new TimeSlotSignupDto
                    {
                        UserId = ts.UserId,
                        TimeSlotId = ts.TimeSlotId,
                        Attended = ts.Attended
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the exception details here
                response.Success = false;
                response.Message = "An error occurred while processing the request.";
                // Example: _logger.LogError(ex, "Error occurred in CheckUpdateUserBatch.");
            }

            return response;
        }

        /// <summary>
        /// Check User Delete Response is sucessfull 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<bool>> CheckUserDeleteResponse(int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    response.Success = true;
                    response.Message = "User deleted Successfully";
                    return response;

                }
                else
                {
                    response.Success = false;
                    response.Message = "User not deleted";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occured while delete user: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Check User Post Response
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<UserDto>> CheckUserPostResponse(UserDto userDto)
        {
            var response = new ServiceResponse<UserDto>();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id && u.IsAllow == userDto.IsAllow);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";

                }
                else
                {
                    response.Success = true;
                    response.Message = "User found in database";
                    response.Data = new UserDto
                    {
                        Id = userDto.Id,
                        IsAllow = userDto.IsAllow,
                    };
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occured while Update user: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// sservice for delete user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// service for profile
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<UserDto>> ProfileAsync(string userEmail)
        {
            var response = new ServiceResponse<UserDto>();

            if(userEmail == null)
            {
                response.Success = false;
                response.Message = "User email is found";
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);

                if(user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                   

                }
                else
                {
                    response.Success = true;
                    response.Message = "User found Successfully";
                    response.Data = new UserDto
                    {
                        FullName = user.FullName,
                        Address = user.Address,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        IsAllow = user.IsAllow,
                        IsRegistered = user.IsRegistered,
                    };
                    return response;
                }
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }



        /// <summary>
        /// check for Get User Details By Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<UserDto>> GetUserDetailsByEmail(string email)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                // Retrieve the user from the database based on the provided email
                var user = await _context.Users
                    .Where(u => u.Email == email)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Map the User entity to UserDto
                    var userDto = new UserDto
                    {
                        Id = user.Id,
                        TenantId = user.TenantId,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        IsRegistered = user.IsRegistered,
                     
                    };

                    response.Data = userDto;
                    response.Success = true;
                }
                else
                {
                    // User not found, set response message and success status
                    response.Success = false;
                    response.Message = "User not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and set response message and success status
                response.Success = false;
                response.Message = $"Error retrieving user details: {ex.Message}";
            }

            return response;
        }


        public Task CheckUserDeleteResponse(UserDto user)
        {
            throw new NotImplementedException();
        }
    }
}




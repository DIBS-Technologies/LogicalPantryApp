using AutoMapper;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.RegistrationService
{
    public class RegistrationService : IRegistrationService
    {
        
        private readonly ILogger<RegistrationService> logger;// Dependency injection for ILogger
        private readonly IMapper mapper;// Dependency injection for IMapper
        private readonly ApplicationDataContext dataContext; // Dependency injection for DataContext

        // Constructor with dependency injection
        public RegistrationService(ILogger<RegistrationService> logger, IMapper mapper, ApplicationDataContext dataContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }

        /// <summary>
        /// Checks whether a given email exists in the system.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{T}"/> containing a boolean indicating whether the email exists:
        /// <c>true</c> if the email exists, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method checks if an email exists in the Users table, performs case-insensitive comparison,
        /// and returns the appropriate response.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the database query.</exception>
        public async Task<ServiceResponse<bool>> CheckEmailIsExist(string email)
        {
            var response = new ServiceResponse<bool>();

            // Validate input
            if (string.IsNullOrEmpty(email))
            {
                response.Success = false;
                response.Message = "Email cannot be null or empty.";
                response.Data = false;
                return response;
            }

            try
            {
                // Check if the email exists in the Users table
                bool emailExists = await dataContext.Users
                    .AnyAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                // Set response based on the existence of the email
                response.Success = true;
                response.Data = emailExists;
                response.Message = emailExists ? "Email exists." : "Email does not exist.";
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                logger.LogError(ex, "Error checking if email exists");

                // Return a generic error response
                response.Success = false;
                response.Message = $"An error occurred while checking the email: {ex.Message}";
                response.Data = false;
            }

            return response;
        }


        /// <summary>
        /// Registers a new user or updates an existing user if their email is already in the system.
        /// </summary>
        /// <param name="userDto">A <see cref="UserDto"/> object containing the user data to register.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{T}"/> containing a boolean indicating whether the user was successfully registered:
        /// <c>true</c> if the registration was successful, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If the user's email already exists in the database and the user is registered, the method returns a failure response.
        /// If the email exists but the user is not registered, the existing user's details are updated. 
        /// Otherwise, a new user is created and added to the database.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during the registration process.</exception>
        public async Task<ServiceResponse<bool>> RegisterUser(UserDto userDto)
        {
            var response = new ServiceResponse<bool>();

            if (userDto == null)
            {
                response.Success = false;
                response.Message = "User data is null.";
                response.Data = false; // Indicating failure
                return response;
            }      
            try
            {
                // Check if the email already exists in the database
                var existingUser = await dataContext.Users
                    .FirstOrDefaultAsync(u => u.Email == userDto.Email);

                if (existingUser != null)
                {
                    if (existingUser.IsRegistered)
                    {
                        // If the user is already registered, return a response indicating failure
                        response.Success = false;
                        response.Message = "Email is already registered.";
                        response.Data = false; // Indicating failure
                        return response;
                    }

                    // If the user exists but is not registered, update the existing user
                    existingUser.FullName = userDto.FullName;
                    existingUser.Address = userDto.Address;
                    existingUser.PhoneNumber = userDto.PhoneNumber;
                    existingUser.IsAllow = false; // Default value or update based on your logic
                    existingUser.IsRegistered = true; // Ensure the registration status remains false or set as needed

                    // Update the user in the database
                    dataContext.Users.Update(existingUser);
                }
                else
                {
                    // Create a new user entity
                    var newUser = new User
                    {
                        TenantId = userDto.TenantId,
                        FullName = userDto.FullName,
                        Address = userDto.Address,
                        Email = userDto.Email,
                        PhoneNumber = userDto.PhoneNumber,
                        IsAllow = false, // Default value
                        IsRegistered = false // Default value
                    };

                    // Add the new user to the database
                    await dataContext.Users.AddAsync(newUser);
                }

                // Save changes to the database
                await dataContext.SaveChangesAsync();

                response.Data = true; // Indicating success
                response.Success = true;
                response.Message = "User registered successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error registering user: {ex.Message}";
                response.Data = false; // Indicating failure
            }

            return response;
        }



      
    }
}

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
                    response.Success = false;
                    response.Message = "Email is already registered.";
                    response.Data = false; // Indicating failure
                    return response;
                }

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
                dataContext.Users.Add(newUser);

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

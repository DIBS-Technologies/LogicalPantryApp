using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LogicalPantry.DTOs;

using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models.Enums;
using LogicalPantry.DTOs.Roledtos;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Azure.Core;
using Azure;

namespace LogicalPantry.Services.UserServices
{
    
    public class UserService : IUserService
    {
        private readonly ApplicationDataContext _context;
        public UserService(ApplicationDataContext context)
        {
            _context = context;
        }

        private readonly ILogger<UserService> logger;// Dependency injection for ILogger
        private readonly IMapper mapper;// Dependency injection for IMapper
        private readonly ApplicationDataContext dataContext; // Dependency injection for DataContext

        // Constructor with dependency injection
        public UserService(ILogger<UserService> logger, IMapper mapper, ApplicationDataContext dataContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }


        /// <summary>
        /// Retrieves all registered users for a specific tenant who have the role of 'User'.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant for which registered users are being retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of registered users.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while fetching users.</exception>
        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllRegisteredUsersAsync(int tenantId)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();

            try
            {
                var userRoleId = (int)UserRoleEnum.User; // Get the enum value for 'User'

                            var users = await dataContext.Users
                       .Join(
                           dataContext.UserRoles,
                           u => u.Id,
                           ur => ur.UserId,
                           (u, ur) => new { User = u, UserRole = ur }
                       )
                       .Join(
                           dataContext.Roles,
                           ur => ur.UserRole.RoleId,
                           r => r.Id,
                           (ur, r) => new { ur.User, Role = r }
                       )
                       .Where(x => x.User.IsRegistered && x.User.TenantId == tenantId &&  x.Role.Id == userRoleId)
                       .Select(x => new UserDto
                       {
                           Id = x.User.Id,
                           FullName = x.User.FullName,
                           Email = x.User.Email,
                           PhoneNumber = x.User.PhoneNumber,
                           Address = x.User.Address,
                           IsAllow = x.User.IsAllow,
                           IsRegistered = x.User.IsRegistered
                       }).ToListAsync();

                response.Data = users;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching users: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the user details if found, otherwise an error message.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while fetching the user.</exception>
        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(int id)
        {
            var response = new ServiceResponse<UserDto>();

            try
            {
                var user = await dataContext.Users
                    .Where(u => u.Id == id)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        IsAllow = u.IsAllow,
                        IsRegistered = u.IsRegistered
                    }).FirstOrDefaultAsync();

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                }
                else
                {
                    response.Data = user;
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching user: {ex.Message}";
            }

            return response;
        }



        /// <summary>
        /// Deletes a user and all associated data from the database.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{bool}"/> indicating success or failure of the operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the deletion process.</exception>
        public async Task<ServiceResponse<bool>> DeleteUserAsync(int id)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Begin a transaction to ensure atomicity
                using (var transaction = await dataContext.Database.BeginTransactionAsync())
                {
                    // Retrieve the user and include related entities
                    var user = await dataContext.Users
                        .Include(u => u.UserRoles)         // Include user roles to remove them
                        .Include(u => u.TimeSlotSignups)   // Include time slot sign-ups to remove them
                        .Include(u => u.TimeSlots)         // Include time slots to remove them
                        .FirstOrDefaultAsync(u => u.Id == id);
                    

                    if (user == null)
                    {
                        response.Success = false;
                        response.Message = "User not found.";
                        return response;
                    }

                    // Remove related entities
                    dataContext.UserRoles.RemoveRange(user.UserRoles);        // Remove user roles
                    dataContext.TimeSlotSignups.RemoveRange(user.TimeSlotSignups); // Remove time slot sign-ups
                    dataContext.TimeSlots.RemoveRange(user.TimeSlots);        // Remove time slots

                    // Remove the user
                    dataContext.Users.Remove(user);

                    // Save changes and commit transaction
                    await dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response.Data = true;
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting user: {ex.Message}";
            }

            return response;
        }


        /// <summary>
        /// Updates the details of an existing user in the database.
        /// </summary>
        /// <param name="userDto">The <see cref="UserDto"/> object containing the updated user information.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the updated user information or an error message.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the update process.</exception>
        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto userDto)
        {
            var response = new ServiceResponse<UserDto>();

            try
            {
                // Find the user by ID
                var user = await dataContext.Users.FindAsync(userDto.Id);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                user.IsAllow = userDto.IsAllow;
                dataContext.Users.Update(user);
                await dataContext.SaveChangesAsync();

                // Set response data and success status
                response.Data = userDto;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating user: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Updates the attendance status for a list of users and time slots.
        /// </summary>
        /// <param name="userAttendedDtos">A list of <see cref="UserDto"/> objects containing user IDs, time slot IDs, and their attendance status.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{bool}"/> indicating the result of the operation.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the update process.</exception>
        public async Task<ServiceResponse<bool>> UpdateUserAttendanceStatusAsync(List<UserDto> userAttendedDtos)
        {
            var response = new ServiceResponse<bool>();

            if (userAttendedDtos == null || !userAttendedDtos.Any())
            {
                response.Success = false;
                response.Message = "No users to update.";
                return response;
            }

            try
            {
                // Extract user IDs and corresponding time slot IDs from the list of DTOs
                var userTimeSlotPairs = userAttendedDtos
                    .Select(dto => new { dto.Id, dto.TimeSlotId })
                    .ToList();

                if (!userTimeSlotPairs.Any())
                {
                    response.Success = false;
                    response.Message = "No valid users provided.";
                    return response;
                }

                // Fetch all relevant TimeSlotSignups from the database
                var allTimeSlotSignups = await dataContext.TimeSlotSignups
                    .ToListAsync();

                // Filter the data in-memory
                var timeSlotSignupsToUpdate = allTimeSlotSignups
                    .Where(ts => userTimeSlotPairs.Any(uts => uts.Id == ts.UserId && uts.TimeSlotId == ts.TimeSlotId))
                    .ToList();

                // Update the 'Attended' status for matching records
                foreach (var userDto in userAttendedDtos)
                {
                    var timeSlotSignup = timeSlotSignupsToUpdate
                        .FirstOrDefault(ts => ts.UserId == userDto.Id && ts.TimeSlotId == userDto.TimeSlotId);

                    if (timeSlotSignup != null)
                    {
                        timeSlotSignup.Attended = userDto.Attended;
                    }
                }

                // Save changes to the database
                await dataContext.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "User attendance status updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating user attendance status: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Retrieves a list of registered users for a specific tenant and time slot.
        /// </summary>
        /// <param name="timeSlot">The date and time slot to filter the users by.</param>
        /// <param name="tenantId">The ID of the tenant to which the users belong.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of users or an error message.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during data retrieval.</exception>
        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlot(DateTime timeSlot, int tenantId)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();

            try
            {
                // Retrieve users matching the tenantId and where IsRegistered is true
                var users = await dataContext.Users
                    .Where(u => u.TenantId == tenantId && u.IsRegistered)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsAllow = u.IsAllow
                    })
                    .ToListAsync();

                // Check if any users are found
                if (!users.Any())
                {
                    response.Success = false;
                    response.Message = "No registered users found for the specified tenant.";
                    response.Data = Enumerable.Empty<UserDto>();
                }
                else
                {
                    response.Data = users;
                    response.Success = true;
                    response.Message = "Changes retrieved successfully.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching users: {ex.Message}";
                response.Data = Enumerable.Empty<UserDto>();
            }

            return response;
        }


        /// <summary>
        /// Retrieves a user by their email address and tenant ID. If the user does not exist, creates a new user.
        /// If the email is found in the Tenant table, assigns the role accordingly.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve or create.</param>
        /// <param name="tenantId">The ID of the tenant associated with the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> 
        /// with the user information or an error message.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the operation.</exception>
        public async Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email,int tenantId)
        {
            var response = new ServiceResponse<UserDto>();

            try
            {
                if(email != null)
                {
                    
                
                
                // Check if the email is present in the Tenant table
                var tenant = dataContext.Tenants
                    .Where(t => t.AdminEmail == email)
                    .FirstOrDefault();

                    if (tenant != null)
                    {
                        // Email is present in the Tenant table, check if it's also present in the User table
                        var existingUser = dataContext.Users
                            .Where(u => u.Email == email)
                            .FirstOrDefault();

                        if (existingUser != null)
                        {
                            // User is already added in the User table
                            var existingUserRole = dataContext.UserRoles
                                .Where(ur => ur.UserId == existingUser.Id)
                                .Select(ur => ur.RoleId)
                                .FirstOrDefault();
                        
                            if (existingUserRole == (int)UserRoleEnum.Admin)
                            {
                                response.Message = "User already exists with Admin role.";                              
                            }
                            else if (existingUserRole == (int)UserRoleEnum.User)
                            {
                                response.Message = "User already exists with User role.";
                            }
                            else
                            {
                                response.Message = "User already exists with an unknown role.";
                            }

                            response.Data = new UserDto
                            {
                                Id = existingUser.Id,
                                FullName = existingUser.FullName,
                                Email = existingUser.Email,
                                PhoneNumber = existingUser.PhoneNumber,
                                IsAllow = existingUser.IsAllow,
                                IsRegistered = false,
                                ZipCode = existingUser.ZipCode,
                                HouseholdSize = existingUser.HouseholdSize,
                                DateOfBirth = existingUser.DateOfBirth,
                                HasSchoolAgedChildren = existingUser.HasSchoolAgedChildren,
                                IsMarried = existingUser.IsMarried,
                                ProfilePictureUrl = existingUser.ProfilePictureUrl,
                                EmploymentStatus = existingUser.EmploymentStatus,
                                IsVeteran = existingUser.IsVeteran
                            };
                            response.Success = true;
                        }
                        else
                        {
                            // Email is in the Tenant table but not in the User table, add as Admin
                            var adminUser = new User
                            {
                                TenantId = tenant.Id,
                                FullName = string.Empty,
                                Address = string.Empty,
                                Email = email,
                                PhoneNumber = string.Empty,
                                IsAllow = false,
                                IsRegistered = false
                            };

                            dataContext.Users.Add(adminUser);
                            await dataContext.SaveChangesAsync();

                            // Assign Admin role to the user
                            var adminRole = new UserRole
                            {
                                UserId = adminUser.Id,
                                RoleId = (int)UserRoleEnum.Admin
                            };
                            dataContext.UserRoles.Add(adminRole);
                            await dataContext.SaveChangesAsync();

                            response.Data = new UserDto
                            {
                                Id = adminUser.Id,
                                FullName = adminUser.FullName,
                                Email = adminUser.Email,
                                PhoneNumber = adminUser.PhoneNumber,
                                IsAllow = adminUser.IsAllow
                            };
                            response.Success = true;
                            response.Message = "User registered as Admin successfully.";
                        }
                    }
                    else
                    {
                        // Email is not present in the Tenant table, check the User table
                        var user = dataContext.Users
                            .Where(u => u.Email == email)
                            .FirstOrDefault();

                        if (user != null)
                        {
                            // User already exists in the User table
                            var existingUserRole = dataContext.UserRoles
                                .Where(ur => ur.UserId == user.Id)
                                .Select(ur => ur.RoleId)
                                .FirstOrDefault();

                            if (existingUserRole == (int)UserRoleEnum.Admin)
                            {
                                response.Message = "User already exists with Admin role.";
                            }
                            else if (existingUserRole == (int)UserRoleEnum.User)
                            {
                                response.Message = "User already exists with User role.";
                            }
                            else
                            {
                                response.Message = "User already exists with an unknown role.";
                            }

                            response.Data = new UserDto
                            {
                                Id = user.Id,
                                FullName = user.FullName,
                                Email = user.Email,
                                PhoneNumber = user.PhoneNumber,
                                IsAllow = user.IsAllow
                            };
                            response.Success = true;
                        }
                        else
                        {
                            // User does not exist, create the user with the User role
                            var newUser = new User
                            {
                                TenantId = tenantId,
                                FullName = string.Empty,
                                Address = string.Empty,
                                Email = email,
                                PhoneNumber = string.Empty,
                                IsAllow = false,
                                IsRegistered = false
                            };

                            dataContext.Users.Add(newUser);
                            await dataContext.SaveChangesAsync(); // Save the new user

                            // Assign User role to the user
                            var userRole = new UserRole
                            {
                                UserId = newUser.Id,
                                RoleId = (int)UserRoleEnum.User
                            };
                            dataContext.UserRoles.Add(userRole);
                            await dataContext.SaveChangesAsync(); // Save the role

                            response.Data = new UserDto
                            {
                                Id = newUser.Id,
                                FullName = newUser.FullName,
                                Email = newUser.Email,
                                PhoneNumber = newUser.PhoneNumber,
                                IsAllow = newUser.IsAllow
                            };
                            response.Success = true;
                            response.Message = "User registered as User successfully.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error registering user: {ex.Message}";
                response.Data = null; // Indicating failure
            }

            return response;
        }






        /// <summary>
        /// Retrieves users associated with a specified time slot and have the role 'User'.
        /// Includes attendance status for each user.
        /// </summary>
        /// <param name="timeSlotId">The ID of the time slot for which to retrieve users.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> 
        /// with the list of users or an error message.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the operation.</exception>
        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlotId(int timeSlotId)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();

            try
            {

                // Retrieve users associated with the specified time slot with role 'User'
                var users = await dataContext.TimeSlotSignups
                    .Where(tsu => tsu.TimeSlotId == timeSlotId) // Filter by TimeSlotId
                    .Join(dataContext.Users,
                        tsu => tsu.UserId,
                        u => u.Id,
                        (tsu, u) => new { tsu, User = u })
                    .Join(dataContext.UserRoles,
                        combined => combined.User.Id,
                        ur => ur.UserId,
                        (combined, ur) => new { combined.User, combined.tsu, UserRole = ur })
                    .Join(dataContext.Roles,
                        combined => combined.UserRole.RoleId,
                        r => r.Id,
                        (combined, r) => new
                        {
                            User = combined.User,
                            Attended = combined.tsu.Attended, // Include Attended field from TimeSlotSignups
                            RoleName = r.RoleName // Include RoleName for filtering
                        })
                    .Where(result => result.RoleName == "User") // Filter by RoleName
                    .Select(result => new UserDto
                    {
                        Id = result.User.Id,
                        FullName = result.User.FullName,
                        Email = result.User.Email,
                        PhoneNumber = result.User.PhoneNumber,
                        Address = result.User.Address,
                        IsAllow = result.User.IsAllow,
                        IsRegistered = result.User.IsRegistered,
                        Attended = result.Attended // Include Attended field
                    })
                    .ToListAsync();





                if (!users.Any())
                {
                    response.Success = false;
                    response.Message = "No registered users found for the specified time slot.";
                    response.Data = Enumerable.Empty<UserDto>();
                }
                else
                {
                    response.Data = users;
                    response.Success = true;
                    response.Message = "Users retrieved successfully.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error fetching users: {ex.Message}";
                response.Data = Enumerable.Empty<UserDto>();
            }

            return response;
        }


        /// <summary>
        /// Retrieves the ID of a user based on their email address.
        /// </summary>
        /// <param name="email">The email address of the user whose ID is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{int}"/> 
        /// with the user ID if found, or an error message if the user is not found or an error occurs.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the operation.</exception>
        public async Task<ServiceResponse<int>> GetUserIdByEmail(string email)
        {
            var response = new ServiceResponse<int>();
            try
            {
                // Retrieve the user with the specified email address
                var user = await dataContext.Users
                    .Where(u => u.Email == email)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    response.Data = user.Id;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "User not found.";
                }
            }
            catch (Exception ex)
            {
                
                response.Success = false;
                response.Message = $"Error retrieving user ID: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Retrieves the role of a user based on their user ID.
        /// </summary>
        /// <param name="id">The ID of the user whose role is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a <see cref="RoleDto"/> 
        /// containing the role ID and role name of the user, or null if the user is not found or an error occurs.</returns>
        public async  Task<RoleDto> GetUserRoleAsync(int id)
        {
            var response = new ServiceResponse<RoleDto>();
            var role = new RoleDto();
            try
            {
                // Retrieve the role associated with the user ID
                var user = await dataContext.UserRoles
                    .Where(u => u.UserId == id)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Map the role details to the RoleDto
                    role = new RoleDto
                    {
                        Id = user.RoleId,
                        RoleName = user.RoleId == 1 ? UserRoleEnum.Admin.ToString() : UserRoleEnum.User.ToString(),
                    };
                }                
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error registering user: {ex.Message}";
                response.Data = null; // Indicating failure
            }
            return role;
        }


        /// <summary>
        /// Registers or updates a user profile based on the provided <see cref="UserDto"/>.
        /// </summary>
        /// <param name="userDto">The user data transfer object containing user details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a <see cref="ServiceResponse{UserDto}"/> 
        /// containing the updated or newly created user profile data and the status of the operation.</returns>
        public async Task<ServiceResponse<UserDto>> ProfileRagistration(UserDto userDto)
        {
            var response = new ServiceResponse<UserDto>();

            if (userDto == null)
            {
                response.Success = false;
                response.Message = "User data is null.";
                response.Data = null; // Indicating failure
                return response;
            }

            try
            {
                // Check if the email already exists in the database
                var existingUser = await dataContext.Users
                    .FirstOrDefaultAsync(u => u.Email == userDto.Email);

                if (existingUser != null)
                {
                    // Update the existing user
                    existingUser.FullName = userDto.FullName;
                    existingUser.Address = userDto.Address;
                    existingUser.PhoneNumber = userDto.PhoneNumber;
                    existingUser.IsAllow = false; // Default value or update based on your logic
                    existingUser.IsRegistered = true; // Ensure the registration status remains false or set as needed
                    existingUser.DateOfBirth = userDto.DateOfBirth;
                    existingUser.ZipCode = userDto.ZipCode;
                    existingUser.HouseholdSize = userDto.HouseholdSize;
                    existingUser.HasSchoolAgedChildren = userDto.HasSchoolAgedChildren;
                    existingUser.IsMarried = userDto.IsMarried;             
                    existingUser.EmploymentStatus = userDto.EmploymentStatus;
                    existingUser.IsVeteran = userDto.IsVeteran;
                    if(userDto.ProfilePictureUrl != null )
                    {
                        existingUser.ProfilePictureUrl = userDto.ProfilePictureUrl;
                    }
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
                        IsAllow = false,
                        IsRegistered = false,
                        ZipCode = userDto.ZipCode,
                        HouseholdSize = userDto.HouseholdSize,
                        DateOfBirth = userDto.DateOfBirth,
                        HasSchoolAgedChildren = userDto.HasSchoolAgedChildren,
                        IsMarried = userDto.IsMarried,
                        ProfilePictureUrl = userDto.ProfilePictureUrl,
                        EmploymentStatus = userDto.EmploymentStatus,
                        IsVeteran = userDto.IsVeteran
                    };

                    // Add the new user to the database
                    await dataContext.Users.AddAsync(newUser);
                    await dataContext.SaveChangesAsync();

                    // Map updated/added user to DTO
                    userDto.Id = newUser.Id;
                }

                // Save changes to the database if there are updates
                if (existingUser != null)
                {
                    await dataContext.SaveChangesAsync();
                }
                
                response.Data = userDto; // Pass the updated user DTO
                response.Success = true;
                response.Message = "User Profile updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error registering user: {ex.Message}";
                response.Data = null; // Indicating failure
            }

            return response;
        }



        /// <summary>
        /// Retrieves user details based on the provided email address.
        /// </summary>
        /// <param name="email">The email address of the user whose details are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a <see cref="ServiceResponse{UserDto}"/> 
        /// containing the user details and the status of the operation.</returns>
        public async Task<ServiceResponse<UserDto>> GetUserDetailsByEmail(string email)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                // Retrieve the user from the database based on the provided email
                var user = await dataContext.Users
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
                        IsAllow = user.IsAllow,
                        IsRegistered = user.IsRegistered,
                        ZipCode = user.ZipCode,
                        IsMarried = user.IsMarried,
                        HouseholdSize = user.HouseholdSize,
                        HasSchoolAgedChildren = user.HasSchoolAgedChildren,
                        IsVeteran = user.IsVeteran,
                        DateOfBirth = user.DateOfBirth,
                        EmploymentStatus = user.EmploymentStatus,
                        ProfilePictureUrl = user.ProfilePictureUrl,
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


    }
}

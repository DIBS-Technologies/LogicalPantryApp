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

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllRegisteredUsersAsync()
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();

            try
            {
                var users = await dataContext.Users
                    .Where(u => u.IsRegistered)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        IsAllow = u.IsAllow,
                        IsRegistered = u.IsRegistered
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

        public async Task<ServiceResponse<bool>> DeleteUserAsync(int id)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var user = await dataContext.Users.FindAsync(id);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found.";
                    return response;
                }

                dataContext.Users.Remove(user);
                await dataContext.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting user: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto userDto)
        {
            var response = new ServiceResponse<UserDto>();

            try
            {
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


     

        public async Task<ServiceResponse<bool>> UpdateUserAllowStatusAsync(List<UserAttendedDto> userAllowStatusDtos)
        {
            var response = new ServiceResponse<bool>();

            if (userAllowStatusDtos == null || !userAllowStatusDtos.Any())
            {
                response.Success = false;
                response.Message = "No users to update.";
                return response;
            }

            try
            {
                // Extract user IDs from the list of DTOs
                var userIds = userAllowStatusDtos
                    .Where(dto => dto.IsAttended)
                    .Select(dto => dto.Id)
                    .ToList();

                if (!userIds.Any())
                {
                    response.Success = false;
                    response.Message = "No users with AllowStatus set to true.";
                    return response;
                }

                // Fetch users matching the IDs from the database
                var usersToUpdate = await dataContext.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                // Update the 'IsAllow' status for matching users
                foreach (var userDto in userAllowStatusDtos)
                {
                    if (userDto.IsAttended) // Only update users with AllowStatus true
                    {
                        var user = usersToUpdate.FirstOrDefault(u => u.Id == userDto.Id);
                        if (user != null)
                        {
                            user.IsAllow = userDto.IsAttended;
                        }
                    }
                }

                // Save changes to the database
                await dataContext.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "User allow status updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating user allow status: {ex.Message}";
            }

            return response;
        }

        //Commented By Swapnil
        //public async Task<UserDto> CheckUserExisist(string email)
        //{
        //    var response = new ServiceResponse<Task<UserDto>>();

        //    //if (email == null)
        //    //{
        //    //    response.Success = false;
        //    //    response.Message = "No users to update.";
        //    //    return response;
        //    //}



        //    //    // Save changes to the database asynchronously
        //    //    await dataContext.SaveChangesAsync();

        //    //    response.Success = true;
        //    //    response.Message = "Time Slot Signup updated successfully.";
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    logger.LogError(ex, "Error posting time slot signups");
        //    //    response.Success = false;
        //    //    response.Message = $"Error posting time slot signups: {ex.Message}";
        //    //}

        //    return response;
        //}
        /// <summary>
        /// Changes in TimeSlot Service : Gte Users by Time Slot Id
        /// </summary>
        /// <param name="timeSlot"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>

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

        public async Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            var response = new ServiceResponse<UserDto>();

            var message = string.Empty;
            try
            {

                var userEmails = dataContext.Users
                    .Where(u => u.Email == email).Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsAllow = u.IsAllow,
                        TenantId = u.TenantId,
                    }).FirstOrDefault();
                // Retrieve users matching the tenantId and where IsRegistered is true
                var users = dataContext.Users
                    .Where(u => u.Email == email)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        IsAllow = u.IsAllow
                    })
                    .FirstOrDefault();


                    //getUserRole = dataContext.UserRoles
                    //    .Where(u => u.UserId != users.Id).Select(x => new { x.UserId, x.RoleId }).ToListAsync().Result;
                
                    if (users == null)
                    {
                        // Create a new user entity
                        var newUser = new User
                        {
                            TenantId = 1,
                            FullName = string.Empty,
                            Address = string.Empty,
                            Email = email,
                            PhoneNumber = string.Empty,
                            IsAllow = false,
                            IsRegistered = false
                        };

                        // Add the new user to the database
                        dataContext.Users.Add(newUser);
                        await dataContext.SaveChangesAsync(); // Use SaveChangesAsync for consistency

                        // Retrieve the newly created user
                        var usersUpdate = dataContext.Users
                            .Where(u => u.Email == email)
                            .Select(u => new UserDto
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                Email = u.Email,
                                PhoneNumber = u.PhoneNumber,
                                IsAllow = u.IsAllow
                            }).FirstOrDefault();

                        // Add a role for the new user
                        var role = new UserRole
                        {
                            UserId = usersUpdate.Id,
                            RoleId = (int)UserRoleEnum.User
                        };
                        dataContext.UserRoles.Add(role);
                        await dataContext.SaveChangesAsync(); // Use SaveChangesAsync for consistency

                        // Prepare the user DTO to return
                        var user = new UserDto
                        {
                            Id = newUser.Id,
                            FullName = newUser.FullName,
                            Email = newUser.Email,
                            PhoneNumber = newUser.PhoneNumber,
                            IsAllow = newUser.IsAllow
                        };

                        // Set the response message
                        response.Data = user; // Indicating success
                        response.Success = true;
                        response.Message = "User registered successfully.";
                    }
                
                else
                {
                    // User already exists, retrieve their role
                    var existingUser = dataContext.Users
                        .Where(u => u.Email == email)
                        .Select(u => new
                        {
                            u.Id,
                            RoleId = dataContext.UserRoles
                                .Where(ur => ur.UserId == u.Id)
                                .Select(ur => ur.RoleId)
                                .FirstOrDefault()
                        }).FirstOrDefault();

                    if (existingUser != null)
                    {
                        // Set message based on the existing user's role
                        if (existingUser.RoleId == (int)UserRoleEnum.Admin)
                        {
                            response.Message = "User already exists with Admin role.";
                        }
                        else if (existingUser.RoleId == (int)UserRoleEnum.User)
                        {
                            response.Message = "User already exists with User role.";
                        }
                        else
                        {
                            response.Message = "User already exists with an unknown role.";
                        }

                        // Optionally, include user details in the response
                        response.Data = new UserDto
                        {
                            Id = existingUser.Id,
                            // Include other relevant user details if needed
                        };
                        response.Success = true;
                    }
                    else
                    {
                        response.Message = "User already available.";
                        response.Success = false;
                        response.Data = userEmails; // Indicating success
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


        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlotId( int timeSlotId)
        {
            var response = new ServiceResponse<IEnumerable<UserDto>>();

            try
            {

                // select user id from timeSlots with timeSlotId join user table and return these user info 
                var users = await dataContext.TimeSlots
                 .Where(ts => ts.Id == timeSlotId)
                 .Join(dataContext.Users,
                       ts => ts.UserId,
                       u => u.Id,
                       (ts, u) => new UserDto
                       {
                           Id = u.Id,
                           FullName = u.FullName,
                           Email = u.Email,
                           PhoneNumber = u.PhoneNumber,
                           IsAllow = u.IsAllow,
                           TenantId = u.TenantId,
                       })
                     .ToListAsync();

          
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



        public async Task<ServiceResponse<int>> GetUserIdByEmail(string email)
        {
            var response = new ServiceResponse<int>();
            try
            {
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

        public  async  Task<RoleDto> GetUserRoleAsync(int id)
        {
           
            var role = new RoleDto();
            try
            {
                
                var user = await dataContext.UserRoles
                    .Where(u => u.UserId == id)
                    .FirstOrDefaultAsync();

                if (user != null)
                {


                    role = new RoleDto
                    {
                        Id = user.RoleId,
                        RoleName = user.RoleId == 1 ? UserRoleEnum.Admin.ToString() : UserRoleEnum.User.ToString(),
                    };
                }
                
            }
            catch (Exception ex)
            {

            }

            return role;
        }
    }
}

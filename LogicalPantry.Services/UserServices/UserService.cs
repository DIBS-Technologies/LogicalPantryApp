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

                //dataContext.Users.Remove(user);
                //await dataContext.SaveChangesAsync();

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

        public Task<UserDto> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResponse<bool>> UpdateUserAllowStatusAsync(List<UserAllowStatusDto> userAllowStatusDtos)
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
                    .Where(dto => dto.IsAllow)
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
                    if (userDto.IsAllow) // Only update users with AllowStatus true
                    {
                        var user = usersToUpdate.FirstOrDefault(u => u.Id == userDto.Id);
                        if (user != null)
                        {
                            user.IsAllow = userDto.IsAllow;
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

        
    }
}

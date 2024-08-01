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

      
        
    }
}

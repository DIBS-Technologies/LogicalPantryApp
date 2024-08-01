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
        public List<UserDto> Get(int tenentId)
        {
            if (tenentId == 0) return null;
            try
            {
                var result = (from u in _context.Users
                              where u.TenantId == tenentId
                              select new UserDto
                              {
                                  Id = u.Id,
                                  FullName = u.FullName,
                                  Email = u.Email,
                                  PhoneNumber = u.PhoneNumber,
                                  IsAllow = u.IsAllow
                              }).ToList();

                return result;
            }
            catch (Exception ex) { throw; }
        }

        public List<UserDto> GetUsersbyTimeSlot(DateTime timeSlot, int tenentId)
        {
            throw new NotImplementedException();
        }

        public string Post(List<UserDto> users)
        {
            if (users == null || !users.Any())
            {
                return "No users to update.";
            }

            try
            {
                // Extract user IDs from the list of DTOs
                var userIds = users.Select(u => u.Id).ToList();

                // Retrieve the existing users from the database synchronously
                var existingUsers = _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToList();

                // Update the properties of existing users
                foreach (var userDto in users)
                {
                    var existingUser = existingUsers.FirstOrDefault(u => u.Id == userDto.Id);
                    if (existingUser != null)
                    {
                        existingUser.FullName = userDto.FullName;
                        existingUser.Email = userDto.Email;
                        existingUser.PhoneNumber = userDto.PhoneNumber;
                        existingUser.IsAllow = userDto.IsAllow;
                    }
                }

                // Save changes to the database synchronously
                _context.SaveChanges();

                return "Users updated successfully.";
            }
            catch (Exception ex)
            {
                // Optional: Log the exception (ex) if needed
                throw new ApplicationException("An error occurred while updating users.", ex);
            }
        }
    }
}

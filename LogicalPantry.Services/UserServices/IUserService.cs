using LogicalPantry.DTOs;
using LogicalPantry.DTOs.Roledtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.UserServices
{
    public interface IUserService
    {
        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllRegisteredUsersAsync(int tenantId);
        Task<ServiceResponse<UserDto>> GetUserByIdAsync(int id);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto userDto);
        Task<ServiceResponse<bool>> DeleteUserAsync(int id);
        //Task<UserDto> GetUserByEmailAsync(string email);

        // Task<User> GetOrCreateUserAsync(string email, string name, int tenantId);
        //Task<ServiceResponse<bool>> UpdateUserAllowStatusAsync(List<UserAttendedDto> userAllowStatusDto);
        //Task<ServiceResponse<bool>> UpdateUserAttendanceStatusAsync(List<UserAttendedDto> userAllowStatusDto);
        Task<ServiceResponse<bool>> UpdateUserAttendanceStatusAsync(List<UserDto> userAttendedDtos);

        Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email,int tenantId);

        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlot(DateTime timeSlot, int tenentId);

        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlotId(int timeSlotId);

        Task<ServiceResponse<int>> GetUserIdByEmail(string email);
        Task<RoleDto> GetUserRoleAsync(int id);

        Task<ServiceResponse<UserDto>> ProfileRagistration(UserDto userDto);

        Task<ServiceResponse<UserDto>> GetUserDetailsByEmail(string email);
    }
}

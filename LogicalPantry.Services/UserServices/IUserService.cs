using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.UserServices
{
    public interface IUserService
    {
        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllRegisteredUsersAsync();
        Task<ServiceResponse<UserDto>> GetUserByIdAsync(int id);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto userDto);
        Task<ServiceResponse<bool>> DeleteUserAsync(int id);
        //Task<UserDto> GetUserByEmailAsync(string email);

       // Task<User> GetOrCreateUserAsync(string email, string name, int tenantId);
        Task<ServiceResponse<bool>> UpdateUserAllowStatusAsync(List<UserAllowStatusDto> userAllowStatusDto);


        Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email);

        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlot(DateTime timeSlot, int tenentId);

        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlot(int timeSlotId);



    }
}

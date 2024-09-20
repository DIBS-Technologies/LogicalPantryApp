
using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.UserDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.UserServiceTest
{
    public interface IUserServiceTest
    {
        Task<ServiceResponse<List<TimeSlotSignupDto>>> CheckUpdateUserBatch(List<UserDto> userDto);
        Task<ServiceResponse<bool>> CheckUserDeleteResponse(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<ServiceResponse<UserDto>> CheckUserPostResponse(UserDto userDto);

        Task<ServiceResponse<UserDto>> ProfileAsync(string userEmail);


    }
}

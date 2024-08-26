﻿using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.UserServiceTest
{
    public interface IUserServiceTest
    {
        /// <summary>
        ///  Check user data when user saved in database 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ServiceResponse<UserDto>> CheckUserPostResponse(UserDto user);

        /// <summary>
        ///  Check user data when user updated
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task CheckUserPutResponse(UserDto user);

        /// <summary>
        ///  Check user deleted  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task CheckUserDeleteResponse(UserDto user);


        Task<UserDto> GetUserByIdAsync(int userId);
        Task<bool> AddUserAsync(UserDto userDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<ServiceResponse<bool>> CheckUserDeleteResponse(int userId);

        Task<ServiceResponse<List<TimeSlotSignupDto>>> CheckUpdateUserBatch(List<UserDto> userDto);
    }
}

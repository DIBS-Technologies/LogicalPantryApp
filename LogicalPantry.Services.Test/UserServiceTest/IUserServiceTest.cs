
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
        /// <summary>
        /// Checks and updates a batch of users.
        /// </summary>
        /// <param name="userDto">List of users to check and update.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing a list of <see cref="TimeSlotSignupDto"/> indicating the update result for each user.</returns>
        Task<ServiceResponse<List<TimeSlotSignupDto>>> CheckUpdateUserBatch(List<UserDto> userDto);

        /// <summary>
        /// Checks the response for deleting a user.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing a boolean indicating whether the delete operation was successful.</returns>
        Task<ServiceResponse<bool>> CheckUserDeleteResponse(int userId);

        /// <summary>
        /// Deletes a user asynchronously by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>A boolean value indicating whether the user was successfully deleted.</returns>
        Task<bool> DeleteUserAsync(int userId);

        /// <summary>
        /// Checks the response for posting a new user.
        /// </summary>
        /// <param name="userDto">The user data to be posted.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the posted <see cref="UserDto"/>.</returns>
        Task<ServiceResponse<UserDto>> CheckUserPostResponse(UserDto userDto);

        /// <summary>
        /// Retrieves the profile details of a user by their email address.
        /// </summary>
        /// <param name="userEmail">The email address of the user.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the user's profile information as a <see cref="UserDto"/>.</returns>
        Task<ServiceResponse<UserDto>> ProfileAsync(string userEmail);

        /// <summary>
        /// Gets the user details by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the user details as a <see cref="UserDto"/>.</returns>
        Task<ServiceResponse<UserDto>> GetUserDetailsByEmail(string email);


    }
}

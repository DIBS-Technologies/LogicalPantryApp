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
        /// <summary>
        /// Retrieves all registered users for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant whose users are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of users.</returns>
        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllRegisteredUsersAsync(int tenantId);
        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the user details.</returns>
        Task<ServiceResponse<UserDto>> GetUserByIdAsync(int id);
        /// <summary>
        /// Updates the details of a user.
        /// </summary>
        /// <param name="userDto">The <see cref="UserDto"/> containing updated user details.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the updated user information.</returns>
        Task<ServiceResponse<UserDto>> UpdateUserAsync(UserDto userDto);
        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{bool}"/> indicating the success of the operation.</returns>
        Task<ServiceResponse<bool>> DeleteUserAsync(int id);
        /// <summary>
        /// Updates the attendance status of a list of users.
        /// </summary>
        /// <param name="userAttendedDtos">A list of <see cref="UserDto"/> representing users with updated attendance status.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{bool}"/> indicating the success of the operation.</returns>
        Task<ServiceResponse<bool>> UpdateUserAttendanceStatusAsync(List<UserDto> userAttendedDtos);
        /// <summary>
        /// Retrieves a user by their email address and tenant ID.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <param name="tenantId">The ID of the tenant associated with the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the user details.</returns>
        Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email,int tenantId);

        /// <summary>
        /// Retrieves users who are signed up for a specific time slot.
        /// </summary>
        /// <param name="timeSlot">The date and time of the time slot.</param>
        /// <param name="tenantId">The ID of the tenant associated with the time slot.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of users signed up for the time slot.</returns>
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlot(DateTime timeSlot, int tenentId);

        /// <summary>
        /// Retrieves users who are signed up for a specific time slot identified by its ID.
        /// </summary>
        /// <param name="timeSlotId">The ID of the time slot.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{IEnumerable{UserDto}}"/> with the list of users signed up for the time slot.</returns>
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersbyTimeSlotId(int timeSlotId);

        /// <summary>
        /// Retrieves the user ID based on the provided email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{int}"/> with the user ID.</returns>
        Task<ServiceResponse<int>> GetUserIdByEmail(string email);

        /// <summary>
        /// Retrieves the role of a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="RoleDto"/> with the user's role.</returns>
        Task<RoleDto> GetUserRoleAsync(int id);

        /// <summary>
        /// Registers a user profile with the provided details.
        /// </summary>
        /// <param name="userDto">The <see cref="UserDto"/> containing user profile details.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the registered user information.</returns>
        Task<ServiceResponse<UserDto>> ProfileRagistration(UserDto userDto);

        /// <summary>
        /// Retrieves user details by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing a <see cref="ServiceResponse{UserDto}"/> with the user details.</returns>
        Task<ServiceResponse<UserDto>> GetUserDetailsByEmail(string email);
    }
}

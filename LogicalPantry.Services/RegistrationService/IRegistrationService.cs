using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.RegistrationService
{
    /// <summary>
    /// Interface for managing user registration services.
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">The <see cref="UserDto"/> object containing user details to be registered.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> indicating success or failure of the registration process.</returns>
        Task<ServiceResponse<bool>> RegisterUser(UserDto user);
        /// <summary>
        /// Checks whether the provided email already exists in the system.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing a boolean indicating whether the email exists.</returns>
        Task<ServiceResponse<bool>> CheckEmailIsExist(string email);
    }
}

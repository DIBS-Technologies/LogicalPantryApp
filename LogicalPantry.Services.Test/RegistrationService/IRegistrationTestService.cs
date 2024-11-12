
using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.RegistrationService
{
    public interface IRegistrationTestService
    {
        /// <summary>
        /// Retrieves the user information and returns a service response indicating success or failure.
        /// </summary>
        /// <param name="user">The user data transfer object containing user information.</param>
        /// <returns>A service response containing a boolean value indicating the success or failure of the operation.</returns>
        ServiceResponse<bool> GetUser(UserDto user);
    }
}


using LogicalPantry.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.RoleServices
{
    /// <summary>
    /// Provides functionality for managing and assigning roles to users.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Assigns a specified role to a user asynchronously.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to whom the role will be assigned.</param>
        /// <param name="role">The name of the role to be assigned to the user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method assigns a role to a user based on the user's ID and the provided role name.
        /// </remarks>
        Task AssignRoleToUserAsync(int userId, string role);

    }
}

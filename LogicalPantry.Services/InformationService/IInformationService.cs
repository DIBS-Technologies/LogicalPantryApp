using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.InformationService
{
    /// <summary>
    /// Service interface for handling tenant-related information.
    /// </summary>
    public interface IInformationService
    {
        /// <summary>
        /// Retrieves the tenant information by tenant ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A service response containing the tenant details.</returns>
        Task<ServiceResponse<TenantDto>> GetTenant(int tenantId);

        /// <summary>
        /// Adds or updates tenant information in the database.
        /// </summary>
        /// <param name="tenantDto">The DTO containing tenant details.</param>
        /// <returns>A service response indicating success or failure.</returns>
        Task<ServiceResponse<bool>> PostTenant(TenantDto tenantDto);

        /// <summary>
        /// Retrieves tenant details by a unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier for the tenant (e.g., URL slug).</param>
        /// <returns>The tenant entity that matches the identifier.</returns>
        Task<Tenant> GetTenantByIdentifierAsync(string identifier);

        /// <summary>
        /// Retrieves tenant details from the database by tenant ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The tenant DTO with the corresponding details.</returns>
        Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId);

        /// <summary>
        /// Retrieves the tenant ID associated with a user's email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <returns>The tenant ID corresponding to the user's email.</returns>
        Task<int> GetTenantIdForUserAsync(string email);

        /// <summary>
        /// Retrieves tenant details by tenant ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the tenant.</param>
        /// <returns>A service response containing the tenant details.</returns>
        Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id);

        /// <summary>
        /// Updates tenant information asynchronously.
        /// </summary>
        /// <param name="tenantDto">The DTO containing the updated tenant details.</param>
        /// <returns>A service response containing the updated tenant details.</returns>
        Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto);

        /// <summary>
        /// Retrieves the tenant details based on a page name for a user.
        /// </summary>
        /// <param name="PageName">The name of the tenant's page.</param>
        /// <returns>A service response containing the tenant details.</returns>
        Task<ServiceResponse<TenantDto>> GetTenantPageNameForUserAsync(string PageName);

        /// <summary>
        /// Retrieves tenant details by tenant name.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <returns>A service response containing the tenant details.</returns>
        Task<ServiceResponse<TenantDto>> GetTenantByNameAsync(string tenantName);

        /// <summary>
        /// Retrieves tenant ID based on user email and tenant name.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <returns>A service response containing the tenant details.</returns>
        Task<ServiceResponse<TenantDto>> GetTenantIdByEmail(string userEmail, string tenantName);
    }
}

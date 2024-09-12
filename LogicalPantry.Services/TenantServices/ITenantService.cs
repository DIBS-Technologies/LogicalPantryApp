using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TenantServices
{
    public interface ITenantService
    {
        /// <summary>
        /// Retrieves the current tenant based on the active context or user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, returning a <see cref="TenantDto"/>.</returns>
        Task<TenantDto> GetTenantAsync();

        /// <summary>
        /// Retrieves a tenant from the database based on its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier of the tenant (e.g., tenant name or URL).</param>
        /// <returns>A task representing the asynchronous operation, returning the corresponding <see cref="Tenant"/>.</returns>
        Task<Tenant> GetTenantByIdentifierAsync(string identifier);

        /// <summary>
        /// Retrieves a tenant's data from the database using the tenant ID.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <returns>A task that represents the asynchronous operation, returning a <see cref="TenantDto"/>.</returns>
        Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId);

        /// <summary>
        /// Retrieves the tenant ID associated with a user's email.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>A task representing the asynchronous operation, returning the tenant ID as an integer.</returns>
        Task<int> GetTenantIdForUserAsync(string email);

        /// <summary>
        /// Retrieves a tenant's data by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the tenant.</param>
        /// <returns>A task that represents the asynchronous operation, returning a <see cref="ServiceResponse{TenantDto}"/> containing the tenant data.</returns>
        Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id);

        /// <summary>
        /// Updates an existing tenant's data based on the provided tenant information.
        /// </summary>
        /// <param name="tenantDto">The <see cref="TenantDto"/> containing the updated tenant information.</param>
        /// <returns>A task that represents the asynchronous operation, returning a <see cref="ServiceResponse{TenantDto}"/> with the result of the update operation.</returns>
        Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto);

    }
}

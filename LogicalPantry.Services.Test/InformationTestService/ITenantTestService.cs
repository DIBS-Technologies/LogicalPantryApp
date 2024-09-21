using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TenantDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TenantTestService
{
    public interface ITenantTestService
    {
        /// <summary>
        /// Adds a new tenant and returns a service response indicating if the addition was successful.
        /// </summary>
        /// <param name="tenantDto">The tenant data transfer object containing the tenant's information.</param>
        /// <returns>A service response containing the tenant DTO and status of the operation.</returns>
        Task<ServiceResponse<TenantDto>> IsAddSuccessful(TenantDto tenantDto);

        /// <summary>
        /// Updates an existing tenant and returns a boolean indicating if the update was successful.
        /// </summary>
        /// <param name="tenantDto">The tenant data transfer object containing the updated tenant's information.</param>
        /// <returns>A boolean indicating whether the update was successful.</returns>
        Task<bool> IsUpdateSuccessful(TenantDto tenantDto);
    }
}

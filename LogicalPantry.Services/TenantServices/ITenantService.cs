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
        Task<TenantDto> GetTenantAsync();
        Task<Tenant> GetTenantByIdentifierAsync(string identifier);

        Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId);

        Task<int> GetTenantIdForUserAsync(string email);

        Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id);
        Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto);


    }
}

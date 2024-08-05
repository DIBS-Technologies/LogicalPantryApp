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
    public interface IInformationService
    {
        Task<ServiceResponse<TenantDto>> GetTenant(int tenantId);
        Task<ServiceResponse<bool>> PostTenant(TenantDto tenantDto);

        Task<Tenant> GetTenantByIdentifierAsync(string identifier);

        Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId);

        Task<int> GetTenantIdForUserAsync(string email);

        Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id);
        Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto);

    }
}

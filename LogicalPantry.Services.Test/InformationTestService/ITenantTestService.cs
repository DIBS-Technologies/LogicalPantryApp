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
        Task<ServiceResponse<TenantDto>> IsAddSuccessful(TenantDto tenantDto);
        Task<bool> IsUpdateSuccessful(TenantDto tenantDto);
    }
}

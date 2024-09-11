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
        Task<bool> IsAddSuccessful(TenantDto tenantDto);
        Task<bool> IsUpdateSuccessful(TenantDto tenantDto);
    }
}

using AutoMapper;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.InformationService
{
    public class InformationService : IInformationService
    {
        private readonly ILogger<InformationService> logger;// Dependency injection for ILogger
        private readonly IMapper mapper;// Dependency injection for IMapper
        private readonly ApplicationDataContext dataContext; // Dependency injection for DataContext

        // Constructor with dependency injection
        public InformationService(ILogger<InformationService> logger, IMapper mapper, ApplicationDataContext dataContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }
        public async Task<ServiceResponse<TenantDto>> GetTenant(int tenantId)
        {
            var response = new ServiceResponse<TenantDto>();

            if (string.IsNullOrWhiteSpace(tenantId.ToString()))
            {
                response.Success = false;
                response.Message = "Tenant ID is required.";
                response.Data = null; // No data to return
                return response;
            }

            try
            {
                // Fetch the tenant from the database based on tenantId
                var tenant = await dataContext.Tenants
                    .Where(t => t.Id == tenantId)
                    .Select(t => new TenantDto
                    {
                        Id = t.Id,
                        PaypalId = t.PaypalId,
                        PageName = t.PageName,
                        Logo = t.Logo,
                        Timezone = t.Timezone
                        // Add any other fields you want to include in TenantDto
                    })
                    .FirstOrDefaultAsync();

                if (tenant == null)
                {
                    response.Success = false;
                    response.Message = "Tenant not found.";
                    response.Data = null; // No data found
                }
                else
                {
                    response.Data = tenant;
                    response.Success = true;
                    response.Message = "Tenant retrieved successfully.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving tenant: {ex.Message}";
                response.Data = null; // Error occurred, no data
            }

            return response;
        }

    }
}

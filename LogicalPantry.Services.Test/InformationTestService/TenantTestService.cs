using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TenantDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TenantTestService
{
    public class TenantTestService : ITenantTestService
    {
        private readonly ApplicationDataContext _context;
        private readonly IConfiguration _configuration;

        public TenantTestService()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();


            //Set up configuration to load appsettings json 
            var builderConnectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

               _configuration = builderConnectionString.Build();

            var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

            builder.UseSqlServer(connectionString);

            // Initialize dataContext with the configured options
            _context = new ApplicationDataContext(builder.Options);
        }

        public async Task<ServiceResponse<TenantDto>> IsAddSuccessful(TenantDto tenantDto)
        {
            var response = new ServiceResponse<TenantDto>();

            // Retrieve tenant matching the specified details
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == tenantDto.TenantName
                                          && t.AdminEmail == tenantDto.AdminEmail
                                          && t.PaypalId == tenantDto.PaypalId
                                          && t.PageName == tenantDto.PageName);

            if (tenant != null)
            {
                // Map the found tenant to a TenantDto
                var tenantDetails = new TenantDto
                {
                    Id = tenant.Id,
                    TenantName = tenant.TenantName,
                    AdminEmail = tenant.AdminEmail,
                    PaypalId = tenant.PaypalId,
                    PageName = tenant.PageName,
                    Logo = tenant.Logo,
                    Timezone = tenant.Timezone
                };

                // Set response data and success status
                response.Data = tenantDetails;
                response.Success = true;
                response.Message = "Tenant found successfully.";
            }
            else
            {
                // Set failure response if tenant is not found
                response.Data = null;
                response.Success = false;
                response.Message = "Tenant not found.";
            }

            return response;
        }


        public async Task<bool> IsUpdateSuccessful(TenantDto tenantDto)
        {
            var tenant = _context.Tenants
                .FirstOrDefault(t => t.Id == tenantDto.Id);

            if (tenant == null)
                return false;

            return tenant.TenantName == tenantDto.TenantName &&
                   tenant.AdminEmail == tenantDto.AdminEmail &&
                   tenant.PaypalId == tenantDto.PaypalId &&
                   tenant.PageName == tenantDto.PageName &&
                   tenant.Logo == tenantDto.Logo &&
                   tenant.Timezone == tenantDto.Timezone;
        }
    }

}

using LogicalPantry.DTOs.TenantDtos;
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

        public async Task<bool> IsAddSuccessful(TenantDto tenantDto)
        {
            var tenant = _context.Tenants
                .FirstOrDefault(t => t.TenantName == tenantDto.TenantName && t.AdminEmail == tenantDto.AdminEmail && t.PaypalId == tenantDto.PaypalId && t.PageName == tenantDto.PageName);
            return tenant != null;
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

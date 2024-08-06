using AutoMapper;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Models.Models;
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

        public async Task<Tenant> GetTenantByIdentifierAsync(string identifier)
        {
            return await dataContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == identifier); // Adjust according to your identifier
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
                    .Where(t => t.Id == 1)
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

        public async Task<ServiceResponse<bool>> PostTenant(TenantDto tenantDto)
        {
            var response = new ServiceResponse<bool>();

            tenantDto.Id = 1;
            // Validate input
            if (tenantDto == null || tenantDto.Id <= 0)
            {
                response.Success = false;
                response.Message = "Invalid tenant data provided.";
                return response;
            }

            try
            {
                // Fetch the existing tenant from the database
                var tenant = await dataContext.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantDto.Id);

                // Check if the tenant exists
                if (tenant == null)
                {
                    response.Success = false;
                    response.Message = "Tenant not found.";
                    return response;
                }

                // Update the tenant information
                tenant.PaypalId = tenantDto.PaypalId;
                tenant.PageName = tenantDto.PageName;
                tenant.Logo = tenantDto.Logo;
                tenant.Timezone = tenantDto.Timezone;

                dataContext.Tenants.Update(tenant);

                // Save changes to the database
                await dataContext.SaveChangesAsync();

                // Return a successful response
                response.Success = true;
                response.Data = true; // Indicating success
                response.Message = "Tenant updated successfully.";
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                logger.LogError(ex, "Error updating tenant");

                // Return a generic error response
                response.Success = false;
                response.Message = $"An error occurred while updating the tenant: {ex.Message}";
            }

            return response;
        }
        public async Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId)
        {
            var tenant = await dataContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                // Handle the case where the tenant is not found
                return null; // or throw an exception if you prefer
            }

            // Map the Tenant entity to TenantDto
            var tenantDto = new TenantDto
            {
                Id = tenant.Id,
                TenantName = tenant.TenantName,
                AdminEmail = tenant.AdminEmail,
                PaypalId = tenant.PaypalId,
                PageName = tenant.PageName,
                Logo = tenant.Logo,
                Timezone = tenant.Timezone
            };

            return tenantDto;
        }

        public async Task<int> GetTenantIdForUserAsync(string email)
        {
            // Assuming you have a `Tenant` table with `AdminEmail` field
            var tenant = await dataContext.Tenants
                                       .Where(t => t.AdminEmail == email)
                                       .FirstOrDefaultAsync();

            if (tenant == null)
            {
                throw new Exception("Tenant not found for the user.");
            }

            return tenant.Id;
        }





        public async Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id)
        {
            var response = new ServiceResponse<TenantDto>();

            try
            {
                var tenant = await dataContext.Tenants.FindAsync(id);
                if (tenant != null)
                {
                    response.Data = new TenantDto
                    {
                        Id = tenant.Id,
                        TenantName = tenant.TenantName,
                        AdminEmail = tenant.AdminEmail,
                        PaypalId = tenant.PaypalId,
                        PageName = tenant.PageName,
                        Logo = tenant.Logo,
                        Timezone = tenant.Timezone
                    };
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Tenant not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving tenant: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto)
        {
            var response = new ServiceResponse<TenantDto>();

            try
            {
                var tenant = await dataContext.Tenants.FindAsync(tenantDto.Id);
                if (tenant != null)
                {
                    tenant.PaypalId = tenantDto.PaypalId;
                    tenant.PageName = tenantDto.PageName;
                    tenant.Logo = tenantDto.Logo;
                    tenant.Timezone = tenantDto.Timezone;

                    dataContext.Tenants.Update(tenant);
                    await dataContext.SaveChangesAsync();

                    response.Data = tenantDto;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Tenant not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating tenant: {ex.Message}";
            }

            return response;
        }


        public async Task<ServiceResponse<TenantDto>> GetTenantPageNameForUserAsync(string PageName)
        {
            var response = new ServiceResponse<TenantDto>();
           // var pageName1 = "account-billing";
            try
            {
                var TenantPageName = await dataContext.Tenants.Where(p => p.PageName == PageName)
                                                                     .FirstOrDefaultAsync();

                if (TenantPageName != null)
                {
                    var tenantId = TenantPageName.Id;

                    var tenant = await dataContext.Tenants.FindAsync(tenantId);

                    if (tenant != null)
                    {
                        response.Data = new TenantDto
                        {
                            Id = tenant.Id,
                            TenantName = tenant.TenantName,
                            AdminEmail = tenant.AdminEmail,
                            PaypalId = tenant.PaypalId,
                            PageName = tenant.PageName,
                            Logo = tenant.Logo,
                            Timezone = tenant.Timezone

                        };

                        response.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving tenant: {ex.Message}";
            }
            return response;
        }

    }
}

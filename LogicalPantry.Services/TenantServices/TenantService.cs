using AutoMapper;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.TenantServices
{
    public class TenantService :ITenantService
    {
        private readonly ILogger<TenantService> logger;
        private readonly IMapper mapper;
        private readonly ApplicationDataContext dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor with dependency injection
        public TenantService(ILogger<TenantService> logger, IMapper mapper, ApplicationDataContext dataContext, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<TenantDto> GetTenantAsync()
        {
            if (_httpContextAccessor.HttpContext.Items.TryGetValue("Tenant", out var tenantObj) && tenantObj is Tenant tenant)
            {
                // Map Tenant to TenantDto
                var tenantDto = new TenantDto
                {
                    Id = tenant.Id,
                    TenantName = tenant.TenantName,
                    AdminEmail = tenant.AdminEmail,
                    PaypalId = tenant.PaypalId,
                    PageName = tenant.PageName ?? "Default Page Name",
                    Logo = tenant.Logo,
                    Timezone = tenant.Timezone
                };

                return await Task.FromResult(tenantDto);
            }

            return await Task.FromResult<TenantDto>(null);
        }


        public async Task<Tenant> GetTenantByIdentifierAsync(string identifier)
        {
            return await dataContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == identifier); // Adjust according to your identifier
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
    }
}


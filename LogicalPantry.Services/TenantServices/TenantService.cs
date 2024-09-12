using AutoMapper;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs;
using LogicalPantry.Models.Models;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace LogicalPantry.Services.TenantServices
{
    public class TenantService : ITenantService
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

        /// <summary>
        /// Retrieves the tenant information from the current HTTP context.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation, returning a <see cref="TenantDto"/> containing
        /// the tenant information, or <c>null</c> if the tenant is not found in the current HTTP context.
        /// </returns>
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

        /// <summary>
        /// Retrieves a tenant from the database by its unique identifier, such as the tenant name.
        /// </summary>
        /// <param name="identifier">The unique identifier of the tenant, such as the tenant name.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning the <see cref="Tenant"/> object if found,
        /// or <c>null</c> if no tenant with the given identifier exists.
        /// </returns>
        public async Task<Tenant> GetTenantByIdentifierAsync(string identifier)
        {
            return await dataContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == identifier); // Adjust according to your identifier
        }



        /// <summary>
        /// Retrieves a tenant from the database by its tenant ID and maps it to a <see cref="TenantDto"/>.
        /// </summary>
        /// <param name="tenantId">The unique ID of the tenant to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning a <see cref="TenantDto"/> object 
        /// with the tenant's details if found, or <c>null</c> if the tenant with the specified ID is not found.
        /// </returns>
        public async Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId)
        {
            //get tenant from tenant id 
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

        /// <summary>
        /// Retrieves the tenant ID associated with a given user's email.
        /// </summary>
        /// <param name="email">The email address of the tenant's admin user.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning the ID of the tenant associated with the provided email.
        /// Throws an exception if the tenant is not found.
        /// </returns>
        /// <exception cref="Exception">Thrown when no tenant is found for the specified email.</exception>
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




        /// <summary>
        /// Retrieves tenant information by its unique ID and maps it to a <see cref="TenantDto"/>.
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning a <see cref="ServiceResponse{TenantDto}"/> 
        /// that contains the tenant data or an error message if not found or if an exception occurs.
        /// </returns>
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

        /// <summary>
        /// Updates the details of an existing tenant in the database based on the provided <see cref="TenantDto"/>.
        /// </summary>
        /// <param name="tenantDto">The <see cref="TenantDto"/> containing the updated tenant information.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning a <see cref="ServiceResponse{TenantDto}"/> 
        /// that contains the updated tenant information if the update is successful, or an error message if the tenant is not found or an exception occurs.
        /// </returns>
        public async Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto)
        {
            var response = new ServiceResponse<TenantDto>();

            try
            {
                // Find the tenant by ID from the database
                var tenant = await dataContext.Tenants.FindAsync(tenantDto.Id);
                if (tenant != null)
                {
                    // Update tenant properties with values from tenantDto
                    tenant.PaypalId = tenantDto.PaypalId;
                    tenant.PageName = tenantDto.PageName;
                    tenant.Logo = tenantDto.Logo;
                    tenant.Timezone = tenantDto.Timezone;

                    // Mark the tenant entity as modified
                    dataContext.Tenants.Update(tenant);
                    await dataContext.SaveChangesAsync();

                    // Set the response with the updated tenantDto and indicate success
                    response.Data = tenantDto;
                    response.Success = true;
                }
                else
                {
                    // Set response indicating that the tenant was not found
                    response.Success = false;
                    response.Message = "Tenant not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and set response with error message
                response.Success = false;
                response.Message = $"Error updating tenant: {ex.Message}";
            }

            return response;
        }
    }
}


using AutoMapper;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Models.Models.Enums;
using LogicalPantry.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

        /// <summary>
        /// Retrieves tenant information by tenant ID.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <returns>A service response containing tenant data if successful, or an error message if the tenant is not found or an exception occurs.</returns>
        public async Task<ServiceResponse<TenantDto>> GetTenant(int tenantId)
        {
            // Initialize the service response
            var response = new ServiceResponse<TenantDto>();

            // Check if the tenantId is valid (non-null and not whitespace)
            if (string.IsNullOrWhiteSpace(tenantId.ToString()))
            {
                response.Success = false;  // Set response as failure
                response.Message = "Tenant ID is required.";  // Set appropriate error message
                response.Data = null;  // No data to return in case of failure
                return response;  // Return early as the tenantId is invalid
            }

            try
            {
                // Fetch tenant from the database based on the tenantId, selecting only relevant fields into TenantDto
                var tenant = await dataContext.Tenants
                    .Where(t => t.Id == tenantId)  // Filter by tenantId
                    .Select(t => new TenantDto
                    {
                        Id = t.Id,  // Assign tenant's Id to DTO
                        PaypalId = t.PaypalId,  // Assign PayPal ID
                        PageName = t.PageName,  // Assign page name
                        TenantDisplayName = t.TenantDisplayName,  // Assign display name
                        Logo = t.Logo,  // Assign logo URL or file path
                        Timezone = t.Timezone  // Assign tenant's timezone
                                               // You can include other necessary fields here
                    })
                    .FirstOrDefaultAsync();  // Fetch the first matching tenant or return null

                // Check if the tenant was found
                if (tenant == null)
                {
                    response.Success = false;  // Set response as failure
                    response.Message = "Tenant not found.";  // Inform that no tenant was found
                    response.Data = null;  // No data found
                }
                else
                {
                    response.Data = tenant;  // Set retrieved tenant as the response data
                    response.Success = true;  // Set response as success
                    response.Message = "Tenant retrieved successfully.";  // Success message
                }
            }
            catch (Exception ex)
            {
                // Catch any exception that occurs during the database operation
                response.Success = false;  // Set response as failure
                response.Message = $"Error retrieving tenant: {ex.Message}";  // Include the exception message
                response.Data = null;  // No data to return in case of error
            }

            // Return the response containing success/failure information and tenant data (if found)
            return response;
        }

        /// <summary>
        /// Updates tenant information based on the provided <see cref="TenantDto"/> data.
        /// </summary>
        /// <param name="tenantDto">The tenant data transfer object containing updated tenant information.</param>
        /// <returns>A service response indicating the success or failure of the update operation.
        /// If successful, returns <c>true</c>; otherwise, returns an error message.</returns>
        public async Task<ServiceResponse<bool>> PostTenant(TenantDto tenantDto)
        {
            // Initialize the service response
            var response = new ServiceResponse<bool>();

            // Get existing tenant details by name and assign the tenant ID to the DTO
            var tenantdetails = await GetTenantByNameAsync(tenantDto.TenantName);
            tenantDto.Id = tenantdetails.Data.Id;

            // Validate input: Check if tenantDto is null or if the Id is invalid
            if (tenantDto == null || tenantDto.Id <= 0)
            {
                response.Success = false;
                response.Message = "Invalid tenant data provided.";
                return response;
            }

            try
            {
                // Fetch the existing tenant from the database using the tenant ID
                var tenant = await dataContext.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantDto.Id);

                // Check if the tenant exists
                if (tenant == null)
                {
                    response.Success = false;
                    response.Message = "Tenant not found.";
                    return response;
                }

                // Update the tenant information from the DTO
                tenant.PaypalId = tenantDto.PaypalId;
                tenant.PageName = tenantDto.PageName;
                tenant.TenantDisplayName = tenantDto.TenantDisplayName;
                // Update the tenant logo only if a new logo is provided
                if (tenantDto.Logo != null)
                {
                    tenant.Logo = tenantDto.Logo;
                }                
                tenant.Timezone = tenantDto.Timezone;

                // Mark the tenant entity as updated in the data context
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
            // Return the final response
            return response;
        }

        /// <summary>
        /// Retrieves a tenant from the database based on the provided tenant ID.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to retrieve.</param>
        /// <returns>
        /// A <see cref="TenantDto"/> representing the tenant data if found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method maps the retrieved <see cref="Tenant"/> entity to a <see cref="TenantDto"/> object.
        /// If the tenant is not found, the method returns <c>null</c>.
        /// </remarks>
        public async Task<TenantDto> GetTenantFromDatabaseAsync(int tenantId)
        {
            // Retrieve the tenant by tenantId without tracking changes in the context
            var tenant = await dataContext.Tenants
                .AsNoTracking()// AsNoTracking is used for read-only operations
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
            {
                // If tenant is not found, return null (or throw an exception if needed)
                return null;
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
        /// Retrieves the tenant ID based on the admin's email address.
        /// </summary>
        /// <param name="email">The email address of the tenant's admin.</param>
        /// <returns>The ID of the tenant associated with the provided admin email.</returns>
        /// <exception cref="Exception">Thrown if no tenant is found for the given admin email.</exception>
        /// <remarks>
        /// This method queries the <see cref="Tenant"/> table to find a tenant associated with the provided admin email.
        /// If no tenant is found, an exception is thrown.
        /// </remarks>
        public async Task<int> GetTenantIdForUserAsync(string email)
        {
            // Query the database for a tenant that has the provided admin email
            var tenant = await dataContext.Tenants
                                       .Where(t => t.AdminEmail == email)
                                       .FirstOrDefaultAsync();
            // Check if a tenant was found
            if (tenant == null)
            {
                // Throw an exception if no tenant is found
                throw new Exception("Tenant not found for the user.");
            }

            // Return the tenant ID
            return tenant.Id;
        }




        /// <summary>
        /// Retrieves a tenant by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the tenant to retrieve.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{T}"/> object containing a <see cref="TenantDto"/> if the tenant is found;
        /// otherwise, an error message indicating failure.
        /// </returns>
        /// <remarks>
        /// This method attempts to find a tenant in the database by the provided ID.
        /// If found, it maps the tenant entity to a <see cref="TenantDto"/>. If not found, an appropriate message is returned.
        /// If an exception occurs during the process, the response includes the exception message.
        /// </remarks>
        public async Task<ServiceResponse<TenantDto>> GetTenantByIdAsync(int id)
        {
            var response = new ServiceResponse<TenantDto>();

            try
            {
                // Find the tenant by the provided ID
                var tenant = await dataContext.Tenants.FindAsync(id);
                if (tenant != null)
                {
                    // Map the tenant entity to the TenantDto if the tenant is found
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
                    // If the tenant is not found, return an error message
                    response.Success = false;
                    response.Message = "Tenant not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during the database operation
                response.Success = false;
                response.Message = $"Error retrieving tenant: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Updates the tenant information in the database.
        /// </summary>
        /// <param name="tenantDto">The <see cref="TenantDto"/> containing the updated tenant details.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{T}"/> object containing the updated <see cref="TenantDto"/> if the update is successful;
        /// otherwise, an error message indicating failure.
        /// </returns>
        /// <remarks>
        /// This method finds the tenant by its ID and updates its information (e.g., PaypalId, PageName, Logo, Timezone).
        /// If the tenant is not found, an error message is returned. If an exception occurs, the error message contains the exception details.
        /// </remarks>
        public async Task<ServiceResponse<TenantDto>> UpdateTenantAsync(TenantDto tenantDto)
        {
            var response = new ServiceResponse<TenantDto>();

            try
            {
                // Find the tenant by ID
                var tenant = await dataContext.Tenants.FindAsync(tenantDto.Id);
                if (tenant != null)
                {
                    // Update tenant properties
                    tenant.PaypalId = tenantDto.PaypalId;
                    tenant.PageName = tenantDto.PageName;
                    tenant.Logo = tenantDto.Logo;
                    tenant.Timezone = tenantDto.Timezone;

                    // Save changes to the database
                    dataContext.Tenants.Update(tenant);
                    await dataContext.SaveChangesAsync();

                    // Return the updated tenant data
                    response.Data = tenantDto;
                    response.Success = true;
                }
                else
                {
                    // If tenant not found, return error
                    response.Success = false;
                    response.Message = "Tenant not found.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during the update process
                response.Success = false;
                response.Message = $"Error updating tenant: {ex.Message}";
            }

            return response;
        }




        /// <summary>
        /// For anonymous page - 2-08-2024 kunal karne
        /// </summary>
        /// <param name="tenantName"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<TenantDto>> GetTenantPageNameForUserAsync(string tenantName)
        {
            var response = new ServiceResponse<TenantDto>();
           // var pageName1 = "account-billing";
            try
            {
                // Retrieve the page name for the specified tenant from the database
                var TenantPageName = await dataContext.Tenants.Where(p => p.TenantName == tenantName)
                                                                     .FirstOrDefaultAsync();
               
                if (TenantPageName != null)
                {
                    var tenantId = TenantPageName.Id;

                    // Retrieve the tenant information from the database using the specified tenant ID
                    var tenant = await dataContext.Tenants.FindAsync(tenantId);

                    // if tenant is not null return response with data
                    if (tenant != null)
                    {
                   
                        response.Data = new TenantDto
                        {
                            Id = tenant.Id,
                            TenantName = tenant.TenantName,
                            AdminEmail = tenant.AdminEmail,
                            TenantDisplayName = tenant.TenantDisplayName,
                            PaypalId = tenant.PaypalId,
                            PageName = tenant.PageName,
                            Logo = tenant.Logo,
                            Timezone = tenant.Timezone

                        };

                        response.Success = true;
                    }
                    
                }
                else
                {
                    response.Success = false;
                    response.Message = "   Page Not Found";
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
        /// Retrieves tenant details from the database by the tenant name.
        /// </summary>
        /// <param name="tenantName">The name of the tenant to search for.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{T}"/> object containing the <see cref="TenantDto"/> if the tenant is found;
        /// otherwise, an error message indicating that the tenant was not found.
        /// </returns>
        /// <remarks>
        /// This method searches for a tenant in the database by the tenant's name. If the tenant is found, 
        /// it maps the tenant's details to a <see cref="TenantDto"/> object and returns a successful response. 
        /// If the tenant is not found, the response contains an error message.
        /// </remarks>
        public async Task<ServiceResponse<TenantDto>> GetTenantByNameAsync(string tenantName)
        {
            var response = new ServiceResponse<TenantDto>();

            // Find the tenant by name in the database
            var tenant = await dataContext.Tenants.FirstOrDefaultAsync(t => t.TenantName == tenantName);
            if (tenant != null)
            {
                // Map tenant entity to TenantDto
                response.Data = new TenantDto
                {
                    Id = tenant.Id,
                    TenantName = tenant.TenantName,
                     AdminEmail = tenant.AdminEmail,
                     TenantDisplayName = tenant.TenantDisplayName,
                    PaypalId = tenant.PaypalId,
                    PageName = tenant.PageName,
                    Logo = tenant.Logo,
                    Timezone =   tenant.Timezone
       
                };
                response.Success = true;
            }
            else
            {
                // If tenant not found, return error message
                response.Success = false;
                response.Message = "Tenant not found";
            }

            return response;
        }



        /// <summary>
        /// Retrieves tenant details based on the provided user email and tenant name.
        /// </summary>
        /// <param name="userEmail">The email of the user to check for the associated tenant.</param>
        /// <param name="tenantName">The name of the tenant to search for if the user is not found or associated with a tenant.</param>
        /// <returns>
        /// A <see cref="ServiceResponse{TenantDto}"/> containing the tenant details if found, or an error message if not found.
        /// </returns>
        /// <remarks>
        /// This method first attempts to find a tenant based on the admin email provided. If no tenant is found using the email,
        /// it checks if the user with the provided email exists. If a user is found, it retrieves the tenant associated with the user.
        /// If no user is found, it then attempts to find a tenant based on the provided tenant name.
        /// If successful, the method returns the tenant details; otherwise, it returns an error message indicating the tenant was not found.
        /// </remarks>
        public async Task<ServiceResponse<TenantDto>> GetTenantIdByEmail(string userEmail, string tenantName)
        {
            var response = new ServiceResponse<TenantDto>();

            // Attempt to find a tenant using the admin email
            var tenant = await dataContext.Tenants.FirstOrDefaultAsync(t => t.AdminEmail == userEmail);
            

            if (tenant != null)
            {
                // Tenant found by admin email
                response.Data = new TenantDto
                {
                    Id = tenant.Id,
                    TenantName = tenant.TenantName,
                    // Add other properties as needed
                    PageName = tenant.PageName,
                    AdminEmail= tenant.AdminEmail,
                    TenantDisplayName = tenant.TenantDisplayName,
                };
                response.Success = true;
            }
            else
            {
                // Check if the user exists with the provided email
                var user = await dataContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    // Attempt to find a tenant by tenant name if user is not found
                    tenant = await dataContext.Tenants.FirstOrDefaultAsync(t => t.TenantName == tenantName);
                    if (tenant != null)
                    {
                        // Tenant found by tenant name
                        response.Data = new TenantDto
                        {
                            Id = tenant.Id,
                            TenantName = tenant.TenantName,
                            PageName = tenant.PageName,
                            TenantDisplayName = tenant.TenantDisplayName,
                        };
                        response.Success = true;
                        return response;
                    }
                    // No tenant found with the provided tenant name
                    response.Success = false;
                    response.Message = "Tenant not found";
                }
                else
                {
                    // Retrieve the tenant associated with the user's TenantId
                    tenant = await dataContext.Tenants.FirstOrDefaultAsync(t => t.Id == user.TenantId);

                    if (tenant.Id == user.TenantId)
                    {
                        // Tenant found associated with the user
                        response.Data = new TenantDto
                        {
                            Id = tenant.Id,
                            TenantName = tenant.TenantName,
                            // Add other properties as needed
                            PageName = tenant.PageName,
                            TenantDisplayName = tenant.TenantDisplayName,
                        };
                        response.Success = true;
                        response.Message = "Tenant found";
                    }
                    //else if (tenant.Id != user.TenantId)
                    //{
                    //    response.Success = false;
                    //    response.Message = "User is alredy added for another tenant";
                    //}
                    else
                    {
                        // No tenant associated with the user found
                        response.Success = false;
                        response.Message = "Tenant not found";
                    }

                }
            }

            return response;
        }

        /// <summary>
        /// Retrieves a tenant based on a provided identifier (e.g., tenant name).
        /// </summary>
        /// <param name="identifier">The identifier used to search for the tenant, such as the tenant name.</param>
        /// <returns>
        /// A <see cref="Tenant"/> entity if a tenant with the given identifier is found; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method queries the database to find a tenant that matches the provided identifier. 
        /// The identifier could be adjusted to other properties depending on the specific tenant identification logic.
        /// </remarks>
        public async Task<Tenant> GetTenantByIdentifierAsync(string identifier)
        {         
            return await dataContext.Tenants
                .FirstOrDefaultAsync(t => t.TenantName == identifier); // Adjust according to your identifier
        }
    }
}

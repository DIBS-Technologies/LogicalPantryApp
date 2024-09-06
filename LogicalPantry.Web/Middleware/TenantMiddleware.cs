
using LogicalPantry.Models.Models;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {        
        var path = context.Request.Path.Value;

        // Handle root path or empty tenant name
        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            // await context.Response.WriteAsync("Tenant name is missing");
            await context.Response.WriteAsync("Page Not Found");
            return;
        }

        // Skip middleware processing for static assets
        var staticFileExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".otf", ".eot", ".svg" };
        if (staticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) || path.Contains("assets") || path.Contains("fonts"))
        {
            await _next(context);
            return;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);    
        if (segments.Length > 0)
        {
            var tenantNameFromUrl = segments[0];

            if (segments.Length == 1)
            {

                context.Request.Path = "";
                context.Items["TenantName"] = tenantNameFromUrl;
                await _next(context);
                return;
            }
            if (tenantNameFromUrl == "TenantHomePage")
            {
                await _next(context);
                return;
            }

            if (context.User.Identity.IsAuthenticated)
            {
                var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (userEmail == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: User email not found");
                    return;
                }
                var informationService = context.RequestServices.GetRequiredService<IInformationService>();
                var tenant = await informationService.GetTenantIdByEmail(userEmail, tenantNameFromUrl);

                // Check if tenant and user email are cached
                if (!_cache.TryGetValue(tenantNameFromUrl, out (string TenantName, string UserEmail ) cachedValues))
                {
                    


                    if (tenant == null || tenant.Data == null || tenant.Data?.TenantName == null)
                    {                      
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        // await context.Response.WriteAsync("Tenant not found");
                        await context.Response.WriteAsync("Page Not Found");
                        return;
                    }
                    context.Items["TenantId"] = tenant.Data?.Id;
                    context.Items["TenantImage"] = tenant.Data?.Logo;
                    context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
                    if (tenant.Data?.TenantDisplayName == null)
                    {
                        context.Session.SetString("TenantDisplayName", string.Empty); // Set the session value to an empty string if null
                    }
                    else
                    {
                        context.Session.SetString("TenantDisplayName", tenant.Data.TenantDisplayName); // Set the session value if not null
                    }
                    var pageName = tenant.Data?.PageName;
                    var cachedTenantName = tenant.Data.TenantName;
                    _cache.Set(tenantNameFromUrl, (cachedTenantName, userEmail, pageName), TimeSpan.FromMinutes(10)); // Added expiration
                    cachedValues = (cachedTenantName, userEmail);
                }

                //if (tenantNameFromUrl != cachedValues.TenantName)
                if (!string.Equals(tenantNameFromUrl, cachedValues.TenantName, StringComparison.OrdinalIgnoreCase))
                {                    
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    //await context.Response.WriteAsync("Unauthorized: Tenant mismatch");
                    await context.Response.WriteAsync("Page Not Found");
                    return;                   
                  
                }

                context.Items["TenantName"] = cachedValues.TenantName;
                context.Items["UserEmail"] = cachedValues.UserEmail;
                context.Items["TenantId"] = tenant.Data.Id;
                context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
                if (tenant.Data?.TenantDisplayName == null)
                {
                    context.Session.SetString("TenantDisplayName", string.Empty); // Set the session value to an empty string if null
                }
                else
                {
                    context.Session.SetString("TenantDisplayName", tenant.Data.TenantDisplayName); // Set the session value if not null
                }
                var newPath = "/" + string.Join("/", segments.Skip(1));
                context.Request.Path = newPath;
            }
            else
            {

                var cachedTenantName = tenantNameFromUrl;
                context.Items["TenantName"] = cachedTenantName;
                var informationService = context.RequestServices.GetRequiredService<IInformationService>();
                var tenant = await informationService.GetTenantByNameAsync(cachedTenantName);
                if (tenant.Success == false)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    //await context.Response.WriteAsync("Tenant not found");
                    await context.Response.WriteAsync("Page Not Found");
                    return;
                }

                context.Items["TenantId"] = tenant.Data?.Id;
                var tName = tenant.Data.TenantName;
                context.Items["TenantImage"] = tenant.Data?.Logo;
                context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
                if (tenant.Data?.TenantDisplayName == null)
                {
                    context.Session.SetString("TenantDisplayName", string.Empty); // Set the session value to an empty string if null
                }
                else
                {
                    context.Session.SetString("TenantDisplayName", tenant.Data.TenantDisplayName); // Set the session value if not null
                }
                var newPath = "/" + string.Join("/", segments.Skip(1));
                context.Request.Path = newPath;
            }
        }      

        await _next(context);
    }
}

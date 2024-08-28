//using LogicalPantry.Services.InformationService;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Caching.Memory;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//public class TenantMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly IMemoryCache _cache;

//    public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
//    {
//        _next = next;
//        _cache = cache;
//    }



//    public async Task InvokeAsync(HttpContext context)
//    {
//        // Get the path of the current request
//        var path = context.Request.Path.Value;

//        // Skip middleware processing for static assets (e.g., CSS, JS, images, fonts)
//        var staticFileExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".otf", ".eot", ".svg" };

//        if (staticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) || path.Contains("assets") || path.Contains("fonts"))
//        {
//            // Skip further processing and pass the request to the next middleware
//            await _next(context);
//            return;
//        }

//        // Process tenant-specific logic
//        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);


//        if (segments.Length > 0)  // Ensure there's more than just the tenant in the URL
//        {
//            var tenantNameFromUrl = segments[0]; // Capture TenantA

//            // Simulate authentication check (remove this if you're already doing this elsewhere)
//            if (context.User.Identity.IsAuthenticated)
//            {
//                var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

//                if (!_cache.TryGetValue(tenantNameFromUrl, out string cachedTenantName))
//                {
//                    var informationService = context.RequestServices.GetRequiredService<IInformationService>();
//                    var tenant = await informationService.GetTenantIdByEmail(userEmail);

//                    if (tenant == null)
//                    {
//                        context.Response.StatusCode = StatusCodes.Status404NotFound;
//                        await context.Response.WriteAsync("Tenant not found");
//                        return;
//                    }

//                    cachedTenantName = tenant.Data.TenantName;
//                    _cache.Set(tenantNameFromUrl, cachedTenantName);
//                }

//                if (tenantNameFromUrl != cachedTenantName)
//                {
//                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
//                    await context.Response.WriteAsync("Unauthorized: Tenant mismatch");
//                    return;
//                }

//                // Store the tenant name in HttpContext.Items to access it in the controller
//                context.Items["TenantName"] = cachedTenantName;
//            }
//            else
//            {
//                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                await context.Response.WriteAsync("Unauthorized: User not authenticated");
//                return;
//            }

//            // Modify the path to remove the tenant segment so that the routing can work properly
//            //  var newPath = "/" + string.Join("/", segments.Skip(1));
//            //context.Request.Path = newPath; // Update the path to /TimeSlot/UserCalendar
//            var newPath = "/" + string.Join("/", segments.Skip(1)); // Adjust path to remove tenant segment
//            context.Request.Path = newPath;
//        }


//        await _next(context);
//    }
//}



//using LogicalPantry.Services.InformationService;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Caching.Memory;
//using System;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//public class TenantMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly IMemoryCache _cache;

//    public TenantMiddleware(RequestDelegate next, IMemoryCache cache)
//    {
//        _next = next;
//        _cache = cache;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        // Get the path of the current request
//        var path = context.Request.Path.Value;

//        // Handle root path or empty tenant name
//        if (string.IsNullOrWhiteSpace(path) || path == "/")
//        {
//            context.Response.StatusCode = StatusCodes.Status400BadRequest;
//            await context.Response.WriteAsync("Tenant name is missing ");
//            return;
//        }

//        // Skip middleware processing for static assets (e.g., CSS, JS, images, fonts)
//        var staticFileExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".woff", ".woff2", ".ttf", ".otf", ".eot", ".svg" };
//        if (staticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) || path.Contains("assets") || path.Contains("fonts"))
//        {
//            await _next(context);
//            return;
//        }

//        // Process tenant-specific logic
//        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
//        if (segments.Length > 0)
//        {
//            var tenantNameFromUrl = segments[0]; // Capture TenantA from the URL


//            if (segments.Length == 1)
//            {

//                context.Request.Path = "";
//                context.Items["TenantName"] = tenantNameFromUrl; // Store tenant name for later use in the controller
//                await _next(context);
//            }


//            if (context.User.Identity.IsAuthenticated)
//            {
//                var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

//                if (userEmail == null)
//                {
//                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                    await context.Response.WriteAsync("Unauthorized: User email not found");
//                    return;
//                }

//                // Check if tenant information is cached
//                if (!_cache.TryGetValue(tenantNameFromUrl, out string cachedTenantName))
//                {
//                    var informationService = context.RequestServices.GetRequiredService<IInformationService>();
//                    var tenant = await informationService.GetTenantIdByEmail(userEmail);

//                    if (tenant == null)
//                    {
//                        context.Response.StatusCode = StatusCodes.Status404NotFound;
//                        await context.Response.WriteAsync("Tenant not found");
//                        return;
//                    }

//                    cachedTenantName = tenant.Data.TenantName;
//                    _cache.Set(tenantNameFromUrl, cachedTenantName);
//                }

//                // Ensure the tenant name in the URL matches the tenant associated with the user
//                if (tenantNameFromUrl != cachedTenantName)
//                {
//                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
//                    await context.Response.WriteAsync("Unauthorized: Tenant mismatch");
//                    return;
//                }

//                // Store the tenant name in HttpContext.Items for use in controllers
//                context.Items["TenantName"] = cachedTenantName;
//                var newPath = "/" + string.Join("/", segments.Skip(1)); // Adjust path to remove tenant segment
//                context.Request.Path = newPath;
//            }
//        }

//        // Continue processing the request
//        await _next(context);
//    }
//}


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
            await context.Response.WriteAsync("Tenant name is missing");
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
        //if (path.Split('/', StringSplitOptions.RemoveEmptyEntries).Any(segment => segment.Equals("Auth", StringComparison.OrdinalIgnoreCase)))
        //{
        //    //var newPath = "/" + string.Join("/", segments.Skip(1));
        //    //context.Request.Path = newPath;
        //    await _next(context);
        //    return;
        //}
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
                        await context.Response.WriteAsync("Tenant not found");
                        return;
                    }
                    context.Items["TenantId"] = tenant.Data?.Id;
                    context.Items["TenantImage"] = tenant.Data?.Logo;
                    context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
                    var pageName = tenant.Data?.PageName;
                    var cachedTenantName = tenant.Data.TenantName;
                    _cache.Set(tenantNameFromUrl, (cachedTenantName, userEmail, pageName), TimeSpan.FromMinutes(10)); // Added expiration
                    cachedValues = (cachedTenantName, userEmail);
                }

                //if (tenantNameFromUrl != cachedValues.TenantName)
                if (!string.Equals(tenantNameFromUrl, cachedValues.TenantName, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Unauthorized: Tenant mismatch");
                    return;
                }

                context.Items["TenantName"] = cachedValues.TenantName;
                context.Items["UserEmail"] = cachedValues.UserEmail;
                context.Items["TenantId"] = tenant.Data.Id;
                context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
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
                    await context.Response.WriteAsync("Tenant not found");
                    return;
                }

                context.Items["TenantId"] = tenant.Data?.Id;
                var tName = tenant.Data.TenantName;
                context.Items["TenantImage"] = tenant.Data?.Logo;
                context.Items["TenantDisplayName"] = tenant.Data?.TenantDisplayName;
                var newPath = "/" + string.Join("/", segments.Skip(1));
                context.Request.Path = newPath;
            }
        }      

        await _next(context);
    }
}

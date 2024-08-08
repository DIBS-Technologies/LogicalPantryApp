using LogicalPantry.Services.InformationService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
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

            // Define static file extensions and directories to ignore
            var staticFileExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".woff", ".woff2", ".ttf", ".svg", ".otf" };
            var staticDirectories = new[] { "assets", "fonts", "images", "styles" };

            // Check if the request path contains static file extensions or directories
            if (staticFileExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) ||
                staticDirectories.Any(dir => path.Contains($"/{dir}/", StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Process tenant-specific logic
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);


        if (segments.Length > 0)  // Ensure there's more than just the tenant in the URL
                {
                    var tenantNameFromUrl = segments[0]; // Capture TenantA

                    // Simulate authentication check (remove this if you're already doing this elsewhere)
                    if (context.User.Identity.IsAuthenticated)
                    {
                        var userEmail = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                        if (!_cache.TryGetValue(tenantNameFromUrl, out string cachedTenantName))
                        {
                            var informationService = context.RequestServices.GetRequiredService<IInformationService>();
                            var tenant = await informationService.GetTenantIdByEmail(userEmail);
                 
                            if (tenant == null)
                            {
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                await context.Response.WriteAsync("Tenant not found");
                                return;
                            }

                            cachedTenantName = tenant.Data.TenantName;
                            _cache.Set(tenantNameFromUrl, cachedTenantName);
                        }

                        if (tenantNameFromUrl != cachedTenantName)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Unauthorized: Tenant mismatch");
                            return;
                        }

                        // Store the tenant name in HttpContext.Items to access it in the controller
                        context.Items["TenantName"] = cachedTenantName;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: User not authenticated");
                        return;
                    }

                    // Modify the path to remove the tenant segment so that the routing can work properly
                  //  var newPath = "/" + string.Join("/", segments.Skip(1));
                    //context.Request.Path = newPath; // Update the path to /TimeSlot/UserCalendar
                     var newPath = "/" + string.Join("/", segments.Skip(1)); // Adjust path to remove tenant segment
                context.Request.Path = newPath;
                }
     

                await _next(context);
        }
}

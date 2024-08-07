using LogicalPantry.Services.TenantServices;

namespace LogicalPantry.Web.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var user = context.User; // Get the logged-in user
            if (!user.Identity.IsAuthenticated)
            {

                context.Response.Redirect("/Home/Index");
                return;

            }
            else
            {            

                var path = context.Request.Path.Value;
                var tenantIdentifier = ExtractTenantIdentifier(path);

                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                var tenant = await tenantService.GetTenantByIdentifierAsync(tenantIdentifier);


                var userTenantIdClaim = user.FindFirst("TenantId")?.Value;
                if (userTenantIdClaim == null || tenant.Id.ToString() != userTenantIdClaim)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    await context.Response.WriteAsync("Unauthorized access.");
                    // return;
                }

                context.Items["Tenant"] = tenant;
                await _next(context);
            }
        }
        private string ExtractTenantIdentifier(string path)
        {
            var segments = path.Split('/');
            return segments.Length > 1 ? segments[1] : string.Empty;
        }
    }
}


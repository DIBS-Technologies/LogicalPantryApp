namespace LogicalPantry.Web.wwwroot.Helper
{
    public class TenantMiddleware
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
                await SetTenantIdAsync(context);
                await _next(context);
            }

            // Extracted method for setting the TenantId
            public async Task SetTenantIdAsync(HttpContext context)
            {
                // Set the TenantId header based on some logic
                var tenantId = "SomeTenantId"; // This could come from authentication or other logic
                context.Response.Headers["TenantId"] = tenantId;

                // Optionally perform additional async operations
                await Task.CompletedTask;
            }
        }

    }

}

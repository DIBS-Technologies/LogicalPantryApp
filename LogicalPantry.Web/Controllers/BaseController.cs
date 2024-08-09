using Microsoft.AspNetCore.Mvc;

namespace LogicalPantry.Web.Controllers
{
    public class BaseController : Controller
    {
        protected string TenantName => HttpContext.Items["TenantName"]?.ToString();
    }
}

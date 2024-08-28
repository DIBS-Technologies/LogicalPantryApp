using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LogicalPantry.Web.Controllers
{
    public class BaseController : Controller
    {
        // Properties to access TenantName and UserEmail from HttpContext.Items
        protected string TenantName => HttpContext.Items["TenantName"]?.ToString();
        protected string UserEmail => HttpContext.Items["UserEmail"]?.ToString();

        protected int? TenantId => HttpContext.Items["TenantId"] as int?;

        protected string PageName => HttpContext.Items["PageName"] as string;
        protected string TenantDisplayName => HttpContext.Items["TenantDisplayName"] as string;
        // Method to set ViewBag properties before action execution
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Set ViewBag properties for use in views
            ViewBag.TenantName = TenantName;
            ViewBag.UserEmail = UserEmail;
            ViewBag.TenantId = TenantId;
            ViewBag.PageName = PageName;
            ViewBag.TenantDisplayName = TenantDisplayName;
            // Call base method
            base.OnActionExecuting(filterContext);
        }
    }
}

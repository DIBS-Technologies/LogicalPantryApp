using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LogicalPantry.Web.Controllers
{
    /// <summary>
    /// Base controller class that provides common properties and methods for controllers in the application.
    /// </summary>
    public class BaseController : Controller
    {

        /// <summary>
        /// Gets the tenant name from <see cref="HttpContext.Items"/>.
        /// </summary>
        protected string TenantName => HttpContext.Items["TenantName"]?.ToString();

        /// <summary>
        /// Gets the user email from <see cref="HttpContext.Items"/>.
        /// </summary>
        protected string UserEmail => HttpContext.Items["UserEmail"]?.ToString();

        /// <summary>
        /// Gets the tenant ID from <see cref="HttpContext.Items"/>. Returns null if not available.
        /// </summary>
        protected int? TenantId => HttpContext.Items["TenantId"] as int?;

        /// <summary>
        /// Gets the page name from <see cref="HttpContext.Items"/>.
        /// </summary>
        protected string PageName => HttpContext.Items["PageName"] as string;

        /// <summary>
        /// Gets the tenant display name from <see cref="HttpContext.Items"/>.
        /// </summary>
        protected string TenantDisplayName => HttpContext.Items["TenantDisplayName"] as string;


        /// <summary>
        /// Sets properties in the <see cref="ViewBag"/> before the action method is executed.
        /// </summary>
        /// <param name="filterContext">The context of the action being executed.</param>
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

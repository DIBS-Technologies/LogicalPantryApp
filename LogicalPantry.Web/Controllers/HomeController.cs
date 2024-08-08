using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using LogicalPantry.Web.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using DotNetOpenAuth.AspNet.Clients;
using Facebook;
using FacebookClient = Facebook.FacebookClient;
using LogicalPantry.Web.Helper;
using LogicalPantry.DTOs.TenantDtos;

namespace LogicalPantry.Web.Controllers
{
    public class HomeController : Controller
    {
       // protected TenantDto Tenant => HttpContext.Items["Tenant"] as TenantDto;
        protected string TenantName => HttpContext.Items["TenantName"]?.ToString();

        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
       
        public IActionResult Index()
        {

            return View();
        }

       
        public IActionResult Privacy()
        {
            return View();
        }

       
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
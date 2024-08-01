using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Web.Models;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using LogicalPantry.Web.Helper;
using LogicalPantry.DTOs.UserDtos;
using System.Security.Claims;

namespace LogicalPantry.Web.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
       


    }


}

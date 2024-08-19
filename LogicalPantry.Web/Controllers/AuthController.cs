using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using LogicalPantry.Web.Helper;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using LogicalPantry.Models.Models;
using Microsoft.Extensions.Options;
using System.Security.Policy;

namespace LogicalPantry.Web.Controllers
{
    [Route("Auth")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userServices;
        private readonly IRoleService _roleService;
        private readonly ILogger _logger;
        public AuthController(IUserService userServices, IRoleService roleService , ILogger<AuthController> logger)
        {
            _userServices = userServices;
            _roleService = roleService;
            _logger = logger;
        }

        // Google Authentication
        [HttpGet("GoogleLogin")]
        public IActionResult GoogleLogin()
        {
            var tenantName = TenantName;
           // var tenantUrl = $"/{tenantName}/Auth/GoogleResponse"; // Construct the tenant-specific URL

            // Redirect to the tenant-specific login URL
            //return Redirect(tenantUrl);
            var properties = new AuthenticationProperties
            {
               // RedirectUri = Url.Action(nameof(GoogleResponse))
                RedirectUri = Url.Action("GoogleResponse", "Auth")
                //RedirectUri = Url.Action(tenantUrl)
            
        

           };
             return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            //return Redirect(tenantUrl);
        }

        [HttpPost("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });

            }
            else if (userInfo != null && userInfo.Role == "User")
            {
                if (userInfo.Message == "User registered successfully.")
                {

                    return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                }
                else
                {
                    return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                }
            }
            return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });


        }

        // Facebook Authentication
        [HttpGet("FacebookLogin")]
        public IActionResult FacebookLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(FacebookResponse))
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpPost("FacebookResponse")]
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });

            }
            else if (userInfo != null && userInfo.Role == "User")
            {

                return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
            }

            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });

        }

        // Microsoft Authentication
        [HttpGet("MicrosoftLogin")]
        public IActionResult MicrosoftLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftResponse))
            };
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        [HttpPost("MicrosoftResponse")]
        public async Task<IActionResult> MicrosoftResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });

            }
            else if (userInfo != null && userInfo.Role == "User")
            {

                return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
            }

            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });


        }

        //// Microsoft 365 Authentication
        //public IActionResult Microsoft365Login()
        //{
        //    var properties = new AuthenticationProperties
        //    {
        //        RedirectUri = Url.Action(nameof(Microsoft365Response))
        //    };
        //    return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        //}

        //public async Task<IActionResult> Microsoft365Response()
        //{
        //    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    var userInfo = await CheckIfUserExists(result);

        //    if (userInfo != null && userInfo.Role == "Admin")
        //    {
        //        // Redirect based on user role
        //        return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });

        //    }
        //    else if (userInfo != null && userInfo.Role == "User")
        //    {


        //        return RedirectToAction(ViewConstants.INDEX, ViewConstants.TimeSlotSignUp, new { area = "" });
        //    }

        //    return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });
        //}

        [HttpGet("loginView")]
        public IActionResult loginView()
        {
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH);
        }

        // Check if user exists and update claims
        private async Task<UserInfo> CheckIfUserExists(AuthenticateResult result)
        {
            if (result.Succeeded)
            {
                var claimsIdentity = (ClaimsIdentity)result.Principal.Identity;
                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

                var userEmail = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return null;
                }

                var userExists = await _userServices.GetUserByEmailAsync(userEmail);

                if (userExists.Success)
                {
                    // Retrieve user's role from the role service
                    var role = await _userServices.GetUserRoleAsync(userExists.Data.Id);

                    if (role != null)
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, UserRoles.User.ToString()));

                        HttpContext.Response.Headers.Add("TenantId", userExists.Data.TenantId.ToString());

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);

                        return new UserInfo
                        {
                            UserId = userExists.Data.Id,
                            Role = role.RoleName,
                            Message = userExists.Message
                        };
                    }


                    
                }
            }
            return null;
        }

        public class UserInfo
        {
            public int UserId { get; set; }
            public string Role { get; set; }
            public string Message { get; set; }
        }

       
    }
}

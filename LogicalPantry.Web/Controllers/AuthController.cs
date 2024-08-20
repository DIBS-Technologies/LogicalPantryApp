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
using Microsoft.AspNetCore.Identity;

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
        [HttpPost("GoogleLogin")]
        public IActionResult GoogleLogin()
        {            
            // Redirect to the tenant-specific login URL
            //return Redirect(tenantUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/{TenantName}/Auth/GoogleResponse"
                // RedirectUri = Url.Action(nameof(GoogleResponse))
                // RedirectUri = Url.Action("GoogleResponse", "Auth")
                // RedirectUri = Url.Action("GoogleResponse", "Auth", new { tenantName = TenantName })
                //RedirectUri = Url.RouteUrl($"{TenantName}/Auth/GoogleResponse")

            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }



        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var tenantName = TenantName;
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                //  return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });
                return Redirect($"/{tenantName}/TimeSlot/Calendar");
        
            }
            else if (userInfo != null && userInfo.Role == "User")
            {
                if (userInfo.Message == "User registered successfully.")
                {

                    // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                    return Redirect($"/{tenantName}/Registration/INDEX");
                }
                else
                {
                    //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{tenantName}/TimeSlot/UserCalandar");
                }
            }
            //return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });
            return Redirect($"/{tenantName}/Registration/INDEX");
        }


        //[HttpPost("GoogleLogin")]
        //public IActionResult GoogleLogin()
        //{
        //    var properties = new AuthenticationProperties
        //    {
        //        // Set the redirect URI to the tenant-specific GoogleResponse action
        //        RedirectUri = Url.Action("GoogleResponse", "Auth", new { tenantName = TenantName })
        //    };

        //    // Challenge the Google authentication scheme
        //    return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        //}

        //[HttpGet("GoogleResponse")]
        //public async Task<IActionResult> GoogleResponse(string tenantName)
        //{
        //    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    var userInfo = await CheckIfUserExists(result);

        //    if (userInfo != null)
        //    {
        //        if (userInfo.Role == "Admin")
        //        {
        //            // Redirect Admin users to the calendar page
        //            return Redirect($"/{tenantName}/TimeSlot/Calendar");
        //        }
        //        else if (userInfo.Role == "User")
        //        {
        //            if (userInfo.Message == "User registered successfully.")
        //            {
        //                // Redirect registered users to the registration index page
        //                return Redirect($"/{tenantName}/Registration/INDEX");
        //            }
        //            else
        //            {
        //                // Redirect unregistered users to the user calendar page
        //                return Redirect($"/{tenantName}/TimeSlot/UserCalandar");
        //            }
        //        }
        //    }

        //    // Redirect to login view if user info is not available
        //    return RedirectToAction("Login", "Auth");
        //}


        // Facebook Authentication
        [HttpPost("FacebookLogin")]
        public IActionResult FacebookLogin()
        {
            var properties = new AuthenticationProperties
            {
                //RedirectUri = Url.Action(nameof(FacebookResponse))
                RedirectUri = $"/{TenantName}/Auth/FacebookResponse"
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("FacebookResponse")]
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            //if (userInfo != null && userInfo.Role == "Admin")
            //{
            //    // Redirect based on user role
            //    // return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });
            //    return Redirect($"/{TenantName}/TimeSlot/Calandar");
            //}
            //else if (userInfo != null && userInfo.Role == "User")
            //{

            //    return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
            //}

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                //  return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });
                return Redirect($"/{TenantName}/TimeSlot/Calendar");

            }
            else if (userInfo != null && userInfo.Role == "User")
            {
                if (userInfo.Message == "User registered successfully.")
                {

                    // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                    return Redirect($"/{TenantName}/Registration/INDEX");
                }
                else
                {
                    //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{TenantName}/TimeSlot/UserCalandar");
                }
            }

            // return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });
            return Redirect($"/{TenantName}/AUTH/LOGINVIEW");
        }

        // Microsoft Authentication
        [HttpPost("MicrosoftLogin")]
        public IActionResult MicrosoftLogin()
        {
            var properties = new AuthenticationProperties
            {
                // RedirectUri = Url.Action(nameof(MicrosoftResponse))
                RedirectUri = $"/{TenantName}/Auth/MicrosoftResponse"
            };
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        [HttpGet("MicrosoftResponse")]
        public async Task<IActionResult> MicrosoftResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                //  return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });
                return Redirect($"/{TenantName}/TimeSlot/Calendar");

            }
            else if (userInfo != null && userInfo.Role == "User")
            {
                if (userInfo.Message == "User registered successfully.")
                {

                    // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                    return Redirect($"/{TenantName}/Registration/INDEX");
                }
                else
                {
                    //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{TenantName}/TimeSlot/UserCalandar");
                }
            }

            // return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });
            return Redirect($"/{TenantName}/AUTH/LOGINVIEW");


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

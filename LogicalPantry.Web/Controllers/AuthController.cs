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
            _logger.LogInformation($"GoogleResponse Method is call started");

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
                    return Redirect($"/{tenantName}/TimeSlot/UserCalendar");
                }
            }
            //return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });
            return Redirect($"/{tenantName}/Registration/INDEX");
        }

        // Facebook Authentication
        [HttpPost("FacebookLogin")]
        public IActionResult FacebookLogin()
        {
            _logger.LogInformation($"FacebookLogin Method is call started");

            var properties = new AuthenticationProperties
            {
                //RedirectUri = Url.Action(nameof(FacebookResponse))
                RedirectUri = $"/{TenantName}/Auth/FacebookResponse"
            };
            _logger.LogInformation($"FacebookLogin Method is call ended");
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("FacebookResponse")]
        public async Task<IActionResult> FacebookResponse()
        {
            _logger.LogInformation($"FacebookResponse Method is call started");
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
                    return Redirect($"/{TenantName}/TimeSlot/UserCalendar");
                }
            }

            // return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });
            return Redirect($"/{TenantName}/AUTH/LOGINVIEW");
        }

        // Microsoft Authentication
        [HttpPost("MicrosoftLogin")]
        public IActionResult MicrosoftLogin()
        {
            _logger.LogInformation($"MicrosoftLogin Method is call started");
            var properties = new AuthenticationProperties
            {
                // RedirectUri = Url.Action(nameof(MicrosoftResponse))
                RedirectUri = $"/{TenantName}/Auth/MicrosoftResponse"
            };
            _logger.LogInformation($"MicrosoftLogin Method is call ended");
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        [HttpGet("MicrosoftResponse")]
        public async Task<IActionResult> MicrosoftResponse()
        {
            _logger.LogInformation($"MicrosoftResponse Method is call started");
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

        [HttpGet("loginView")]
        public IActionResult loginView()
        {
            //_logger.LogInformation($"loginView Method is call started");
            //_logger.LogInformation("loginView page accessed.");
            //_logger.LogInformation($"loginView Method is call ended");
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation($"Logout Method is call started");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"Logout Method is call ended");
            return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH);
        }

        // Check if user exists and update claims
        private async Task<UserInfo> CheckIfUserExists(AuthenticateResult result)
        {
            _logger.LogInformation($"CheckIfUserExists Method is call started");
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
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName.ToString()));

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
            _logger.LogInformation($"CheckIfUserExists Method is call ended");
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

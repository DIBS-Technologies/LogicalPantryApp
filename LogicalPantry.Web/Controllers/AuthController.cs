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

namespace LogicalPantry.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userServices;
        private readonly IRoleService _roleService;

        public AuthController(IUserService userServices, IRoleService roleService, ILogger<AuthController> logger)
        {
            _userServices = userServices;
            _roleService = roleService;
            _logger = logger;
        }

        // Google Authentication
        public IActionResult GoogleLogin()
        {
            _logger.LogInformation($"GoogleLogin Method is called Started");
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleResponse))
            };
            _logger.LogInformation($"GoogleLogin Method is call ended");
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            _logger.LogInformation($"GoogleResponse Method is call started");

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
            _logger.LogInformation($"GoogleResponse Method is call ended");
            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });

   
        }

        // Facebook Authentication
        public IActionResult FacebookLogin()
        {
            _logger.LogInformation($"FacebookLogin Method is call started");

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(FacebookResponse))
            };
            _logger.LogInformation($"FacebookLogin Method is call ended");
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> FacebookResponse()
        {
            _logger.LogInformation($"FacebookResponse Method is call started");
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
            _logger.LogInformation($"FacebookResponse Method is call ended");
            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });

        }

        // Microsoft Authentication
        public IActionResult MicrosoftLogin()
        {
            _logger.LogInformation($"MicrosoftLogin Method is call started");
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftResponse))
            };
            _logger.LogInformation($"MicrosoftLogin Method is call ended");
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> MicrosoftResponse()
        {
            _logger.LogInformation($"MicrosoftResponse Method is call started");
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
            _logger.LogInformation($"MicrosoftResponse Method is call ended");
            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });

         
        }

        // Microsoft 365 Authentication
        public IActionResult Microsoft365Login()
        {
            _logger.LogInformation($"Microsoft365Login Method is call started");
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Microsoft365Response))
            };
            _logger.LogInformation($"Microsoft365Login Method is call ended");
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> Microsoft365Response()
        {
            _logger.LogInformation($"Microsoft365Response Method is call started");
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var userInfo = await CheckIfUserExists(result);

            if (userInfo != null && userInfo.Role == "Admin")
            {
                // Redirect based on user role
                return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });

            }
            else if (userInfo != null && userInfo.Role == "User")
            {
                

                return RedirectToAction(ViewConstants.INDEX, ViewConstants.TimeSlotSignUp, new { area = "" });
            }
            _logger.LogInformation($"Microsoft365Response Method is call ended");
            return RedirectToAction(ViewConstants.AUTH, ViewConstants.LOGINVIEW, new { area = "" });
        }

        public IActionResult loginView()
        {
            _logger.LogInformation($"loginView Method is call started");
            _logger.LogInformation("loginView page accessed.");
            _logger.LogInformation($"loginView Method is call ended");
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

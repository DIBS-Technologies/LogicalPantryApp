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
using LogicalPantry.DTOs.UserDtos;

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

        /// <summary>
        /// Initiates Google authentication by redirecting to the Google login page.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method sets up the authentication properties for Google login and then redirects to the Google login page.
        /// It logs the start and end of the method execution for tracking purposes.
        /// The <c>RedirectUri</c> specifies where the user will be redirected after a successful authentication, which is tenant-specific.
        /// </remarks>
        [HttpPost("GoogleLogin")]
        public IActionResult GoogleLogin()
        {
            // Log the beginning of the Index method execution.
            _logger.LogInformation($"GoogleLogin Method is call started");

            // Redirect to the tenant-specific login URL
            //return Redirect(tenantUrl);
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/{TenantName}/Auth/GoogleResponse"              
                //RedirectUri = Url.RouteUrl($"{TenantName}/Auth/GoogleResponse")

            };
            // Log the ending of the Index method execution.
            _logger.LogInformation($"GoogleLogin Method is call ended");
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response from Google authentication and redirects the user based on their role and registration status.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method processes the authentication result received from Google, extracts user claims, and redirects the user based on their role and registration status.
        /// - If the user is an "Admin", they are redirected to the calendar page for admins.
        /// - If the user is a "User", they are redirected to either the registration page, user calendar, or donation page based on their registration status and allowed status.
        /// - If no claims are found or the user is not authenticated, it redirects to the registration page.
        /// The method logs the start and end of its execution for tracking purposes.
        /// </remarks>
        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var tenantName = TenantName;
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsIdentity = (ClaimsIdentity)result.Principal.Identity;

            if (claimsIdentity != null)
            {
                var claimToRemove = claimsIdentity.FindFirst(ClaimTypes.Role);
                if (claimToRemove != null)
                {
                    claimsIdentity.RemoveClaim(claimToRemove);
                }

                var userInfo = await CheckIfUserExists(result);
                if (userInfo != null && userInfo.Role == "Admin")
                {
                    // Redirect based on user role
                    //  return RedirectToAction(ViewConstants.Calandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{tenantName}/TimeSlot/Calendar");
                }
                else if (userInfo != null && userInfo.Role == "User")
                {
                    if (userInfo.Message == "User registered as User successfully.")
                    {

                        // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                        return Redirect($"/{tenantName}/User/Register");
                    }
                    else if (userInfo.IsAllowed)
                    {
                        //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                        return Redirect($"/{tenantName}/TimeSlot/UserCalendar");
                    }
                    else
                    {
                        ViewBag.Message = "User Is Not Allowed";
                        return Redirect($"/{tenantName}/Donation/PayPal");
                    }
                }

                // Log the ending of the Index method execution.
                _logger.LogInformation($"GoogleResponse Method is call ended");
                //return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });
                return Redirect($"/{tenantName}/User/Register");
            }
            return View();
        }


        /// <summary>
        /// Initiates the Facebook authentication process and redirects to the Facebook login page.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method starts the Facebook login process by creating an authentication challenge with the Facebook authentication scheme.
        /// It sets the redirect URI to a tenant-specific URL where the response from Facebook will be processed.
        /// The method logs the start and end of its execution for tracking purposes.
        /// </remarks>
        [HttpPost("FacebookLogin")]
        public IActionResult FacebookLogin()
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation($"FacebookLogin Method is call started");

            var properties = new AuthenticationProperties
            {
                //RedirectUri = Url.Action(nameof(FacebookResponse))
                RedirectUri = $"/{TenantName}/Auth/FacebookResponse"
            };
            // Log the ending of the Index method execution.
            _logger.LogInformation($"FacebookLogin Method is call ended");
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }


        /// <summary>
        /// Handles the response from Facebook authentication, processes the user's login, and redirects to the appropriate page based on the user's role and registration status.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method processes the authentication result from Facebook, checking if the user exists and their role. 
        /// Based on the user's role ("Admin" or "User") and their registration status, it redirects them to the appropriate page:
        /// - Admin users are redirected to the calendar page.
        /// - Registered users are redirected to either the registration page or user calendar, depending on their status and permissions.
        /// - Users who are not allowed are redirected to the PayPal donation page.
        /// If the user does not meet any of the specified conditions, they are redirected to the registration page.
        /// The method logs the start and end of its execution for tracking purposes.
        /// </remarks>
        [HttpGet("FacebookResponse")]
        public async Task<IActionResult> FacebookResponse()
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation($"FacebookResponse Method is call started");
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
                if (userInfo.Message == "User registered as User successfully.")
                {

                    // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                    return Redirect($"/{tenantName}/User/Register");
                }
                else if (userInfo.IsAllowed)
                {
                    //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{tenantName}/TimeSlot/UserCalendar");
                }
                else
                {
                    ViewBag.Message = "User Is Not Allowed";
                    return Redirect($"/{tenantName}/Donation/PayPal");
                }
            }

            // Log the ending of the Index method execution.
            _logger.LogInformation($"GoogleResponse Method is call ended");
            //return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });
            return Redirect($"/{tenantName}/User/Register");
        }

        /// <summary>
        /// Initiates the Microsoft authentication process by redirecting to the Microsoft login page.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method logs the start and end of the authentication process for Microsoft login.
        /// It sets up the authentication properties and redirects the user to Microsoft's authentication page.
        /// The redirect URI is set to the `MicrosoftResponse` action which will handle the authentication result.
        /// </remarks>
        [HttpPost("MicrosoftLogin")]
        public IActionResult MicrosoftLogin()
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation($"MicrosoftLogin Method is call started");
            var properties = new AuthenticationProperties
            {
                // RedirectUri = Url.Action(nameof(MicrosoftResponse))
                RedirectUri = $"/{TenantName}/Auth/MicrosoftResponse"
            };
            // Log the ending of the Index method execution.
            _logger.LogInformation($"MicrosoftLogin Method is call ended");
            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response from Microsoft authentication, processes the user's login, and redirects to the appropriate page based on the user's role and registration status.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> that represents the result of the action.</returns>
        /// <remarks>
        /// This method processes the authentication result from Microsoft, checking if the user exists and their role.
        /// Based on the user's role ("Admin" or "User") and their registration status, it redirects them to the appropriate page:
        /// - Admin users are redirected to the calendar page.
        /// - Registered users are redirected to either the registration page or user calendar, depending on their status and permissions.
        /// - Users who are not allowed are redirected to the PayPal donation page.
        /// If the user does not meet any of the specified conditions, they are redirected to the registration page.
        /// The method logs the start and end of its execution for tracking purposes.
        /// </remarks>
        [HttpGet("MicrosoftResponse")]
        public async Task<IActionResult> MicrosoftResponse()
        {
            // Log the starting of the Index method execution.
            _logger.LogInformation($"MicrosoftResponse Method is call started");
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
                if (userInfo.Message == "User registered as User successfully.")
                {

                    // return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
                    return Redirect($"/{tenantName}/User/Register");
                }
                else if (userInfo.IsAllowed)
                {
                    //return RedirectToAction(ViewConstants.UserCalandar, ViewConstants.TimeSlot, new { area = "" });
                    return Redirect($"/{tenantName}/TimeSlot/UserCalendar");
                }
                else
                {
                    ViewBag.Message = "User Is Not Allowed";
                    return Redirect($"/{tenantName}/Donation/PayPal");
                }
            }

            // Log the ending of the Index method execution.
            _logger.LogInformation($"GoogleResponse Method is call ended");
            //return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH, new { area = "" });
            return Redirect($"/{tenantName}/User/Register");


        }
        /// <summary>
        /// Displays the login view.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet("loginView")]
        public IActionResult loginView()
        {
            return View();
        }

        /// <summary>
        /// Logs out the user by signing out of the authentication scheme and clearing the session.
        /// </summary>
        /// <returns>A redirect to the home page of the current tenant.</returns>
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
          // Log the starting of the Index method execution.
          _logger.LogInformation($"Logout Method is call started");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          
            // Clear the session
            HttpContext.Session.Clear();

          
            return Redirect($"/{TenantName}");
        }

        /// <summary>
        /// Checks if the user exists based on the authentication result and updates the user's claims if found.
        /// </summary>
        /// <param name="result">The authentication result containing user information.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="UserInfo"/> object if the user exists, otherwise null.</returns>
        private async Task<UserInfo> CheckIfUserExists(AuthenticateResult result)
        {
            // Log the starting of the Index method execution.
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
                var tenantId = TenantId;
                var userExists = await _userServices.GetUserByEmailAsync(userEmail, (int)tenantId);

                if (userExists.Success)
                {
                    var role = await _userServices.GetUserRoleAsync(userExists.Data.Id);

                    if (role != null)
                    {
                        // Remove existing role claims if necessary
                        var claimToRemove = claimsIdentity.FindFirst(ClaimTypes.Role);
                        if (claimToRemove != null)
                        {
                            claimsIdentity.RemoveClaim(claimToRemove);
                        }

                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName.ToString()));

                        HttpContext.Response.Headers.Add("TenantId", userExists.Data?.TenantId.ToString());
                        //HttpContext.Request.Headers.Cookie.Add("IsUserAllowed", userExists.Data?.IsAllow.ToString());

                        HttpContext.Items["IsUserAllowed"] = userExists.Data?.IsAllow;



                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);


                   
                        // Add new role claim
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName.ToString()));

                        return new UserInfo
                        {
                            UserId = userExists.Data.Id,
                            Role = role.RoleName,
                            Message = userExists.Message,
                            IsRegistered = userExists.Data.IsRegistered,
                            IsAllowed = userExists.Data.IsAllow,
                        };
                    }

                }
            }
            // Log the ending of the Index method execution.
            _logger.LogInformation($"CheckIfUserExists Method is call ended");
            return null;
        }





    

       
    }
}

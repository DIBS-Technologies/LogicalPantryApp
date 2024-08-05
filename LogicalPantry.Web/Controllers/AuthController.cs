using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using FacebookClient = Facebook.FacebookClient;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using static DotNetOpenAuth.OpenId.Extensions.AttributeExchange.WellKnownAttributes.Contact;
using LogicalPantry.Web.Helper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using LogicalPantry.Models.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Http;
namespace LogicalPantry.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userServices;
        private readonly IRoleService _roleService;

        public AuthController(IUserService userServices, IRoleService roleService, ILogger<UserController> logger)
        {
            _userServices = userServices;
            _roleService = roleService;
            _logger = logger;
        }

        // Package For Authentication:
        // Google Authentication     =>  Microsoft.AspNetCore.Authentication.Google
        // Facebook Authentication   =>  Microsoft.AspNetCore.Authentication.Facebook
        // Microsoft Authentication  =>  Microsoft.AspNetCore.Authentication.MicrosoftAccount => web
        // Office 365 Authentication =>  Microsoft.AspNetCore.Authentication.OpenIdConnect

        /// <summary>
        /// Handles the login action for authentication with Google.
        /// </summary>
        /// <returns>Returns Google Response.</returns>
        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(GoogleResponse)) // Pass the name of the GoogleResponse method
                });
        }

        /// <summary>
        /// Handles the response after successful authentication with Google.
        /// </summary>
        /// <returns>Returns a redirection to the home page.</returns>
        public async Task<IActionResult> GoogleResponse()
            {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var response = await CheckIfUserExist(result);

            if(response == true)
            {
                return RedirectToAction(ViewConstants.INDEX, ViewConstants.Registration, new { area = "" });
            }
            return RedirectToAction(ViewConstants.INDEX, ViewConstants.HOME, new { area = "" });
            }

        /// <summary>
        /// Displays the login view.
        /// </summary>
        /// <returns>Returns the login view.</returns>
        public IActionResult loginView()
        {
            _logger.LogInformation("loginView page accessed.");
            return View();
        }

        /// <summary>
        /// Redirects to Facebook for authentication.
        /// </summary>
        /// <returns>Returns a challenge to authenticate with Facebook.</returns>
        public IActionResult FacebookLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(FacebookResponse))
            };

            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response after successful authentication with Facebook.
        /// </summary>
        /// <returns>Returns a redirection to the home page.</returns>
        public async Task<IActionResult> FacebookResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await CheckIfUserExist(result);


            // Optionally, process the user claims

            return RedirectToAction(ViewConstants.INDEX, ViewConstants.HOME);
        }

        /// <summary>
        /// Redirects to Microsoft for authentication.
        /// </summary>
        /// <returns>Returns a challenge to authenticate with Microsoft.</returns>
        public IActionResult MicrosoftLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftResponse))
            };

            return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response after successful authentication with Microsoft.
        /// </summary>
        /// <returns>Returns a redirection to the home page.</returns>
        public async Task<IActionResult> MicrosoftResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await CheckIfUserExist(result);


            // Optionally, process the user claims

            return RedirectToAction(ViewConstants.INDEX, ViewConstants.HOME);
        }

        /// <summary>
        /// Redirects to Microsoft 365 for authentication.
        /// </summary>
        /// <returns>Returns a challenge to authenticate with Microsoft 365.</returns>
        public IActionResult Microsoft365Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Microsoft365Response))
            };

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response after successful authentication with Microsoft 365.
        /// </summary>
        /// <returns>Returns a redirection to the home page.</returns>
        public async Task<IActionResult> Microsoft365Response()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await CheckIfUserExist(result);


            // Optionally, process the user claims

            return RedirectToAction(ViewConstants.INDEX, ViewConstants.HOME);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>Returns the login view.</returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Optionally, clear other user-related data or perform additional logout actions

            return RedirectToAction(ViewConstants.LOGINVIEW, ViewConstants.AUTH);
        }
        /// <summary>
        /// Method is used to check the user if it exists in database and add role from database to the claims.
        /// </summary>
        /// <param name="result">Authentication Result</param>
        /// <returns>It returns true if authentication is successful or user exists else false.</returns>
        private async Task<bool> CheckIfUserExist(AuthenticateResult result)
            {
            if (result.Succeeded)
                {

                // get the identity details from the authenticated result
                var claimsIdentity = (ClaimsIdentity)result.Principal.Identity;
                // add user role from database to the claim
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, UserRoles.User.ToString()));
                //var accessToken = result.Properties.GetTokenValue("access_token");
                var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
                    {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value,

                    });

                var userName = claims.Where(x => x.Issuer == claimsIdentity.AuthenticationType).Select(x => x.Value).Skip(4).FirstOrDefault();
                var userExisist = await _userServices.GetUserByEmailAsync(userName);


                HttpContext.Response.Headers.Add("TenantId", userExisist.Data.TenantId.ToString());

                await HttpContext.SignInAsync(result.Principal);

                return userExisist.Success;
                }
            return false;
            }


        }
}





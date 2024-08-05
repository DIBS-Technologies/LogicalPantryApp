using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using NLog.Extensions.Logging;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.TenantServices;

using System.Configuration;
using LogicalPantry.DTOs.PayPalSettingDtos;
using LogicalPantry.Services.RegistrationService;
using Autofac.Core;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.TimeSlotSignupService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework Core DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")));

// Configure session
builder.Services.AddSession(options =>
{
    // Set session timeout to 30 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Only accessible via HTTP
    options.Cookie.IsEssential = true; // Cookie is essential for the application
});

// Configure authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default authentication scheme
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default sign-in scheme
})
.AddCookie() // Add cookie authentication
.AddOpenIdConnect(options =>
{
    // Configure OpenID Connect authentication
    options.ClientId = builder.Configuration["Authentication:OpenIdConnectS:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:OpenIdConnectS:ClientSecret"];
    options.CallbackPath = builder.Configuration["Authentication:OpenIdConnectS:CallbackPath"];
    options.Authority = builder.Configuration["Authentication:OpenIdConnectS:Authority"];
    options.SaveTokens = true; // Save authentication tokens
    options.Scope.Add("openid"); // Request openid scope
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name", // Set name claim type
        RoleClaimType = "role"  // Set role claim type
    };
})
.AddMicrosoftAccount(options =>
{
    // Configure Microsoft Account (Azure AD) authentication
    options.ClientId = builder.Configuration["Authentication:AzureAd:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:AzureAd:ClientSecret"];
    options.CallbackPath = builder.Configuration["Authentication:AzureAd:CallbackPath"];
})
.AddGoogle(options =>
{
    // Configure Google authentication
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
})
.AddFacebook(options =>
{
    // Configure Facebook authentication
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
});

// Add scoped services for dependency injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
builder.Services.AddScoped<IRegistrationService , RegistrationService>();
builder.Services.AddScoped<IInformationService, InformationService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ITimeSlotSignupService, TimeSlotSignupService>();
//builder.Services.AddScoped<ITimeSlotSignupService, TimeSlotSignupService>();
builder.Services.Configure<PayPalDto>(builder.Configuration.GetSection("PayPal"));




//builder.Services.AddScoped<ITenantService,TenantService>();


// Add AutoMapper for object mapping
builder.Services.AddAutoMapper(typeof(Startup));

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    loggingBuilder.AddNLog("nlog.config");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Show detailed error page in development mode
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redirect to error page in production mode
    app.UseHsts(); // Use HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Serve static files from wwwroot folder
app.UseSession(); // Enable session middleware
app.UseRouting(); // Enable routing
app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization(); // Enable authorization middleware

// Configure default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=loginView}/{id?}");

app.Run();

// Packages for Authentication
// Google Authentication     =>  Microsoft.AspNetCore.Authentication.Google
// Facebook Authentication   =>  Microsoft.AspNetCore.Authentication.Facebook
// Microsoft Authentication  =>  Microsoft.AspNetCore.Authentication.MicrosoftAccount
// Office 365 Authentication =>  Microsoft.AspNetCore.Authentication.OpenIdConnect

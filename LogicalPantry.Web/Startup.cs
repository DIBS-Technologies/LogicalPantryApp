using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.RoleServices;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.TimeSlotSignupService;
using Microsoft.EntityFrameworkCore.Internal;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Services.TenantServices;
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {

        // Add authorization
        services.AddAuthorization();

        // Add MVC services
        services.AddControllersWithViews();

        // Add DbContext and services
        services.AddDbContext<ApplicationDataContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultSQLConnection")));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IInformationService, InformationService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ITimeSlotSignupService, TimeSlotSignupService>();
        services.AddScoped<ITenantService,TenantService>();
       
        // Add AutoMapper
        services.AddAutoMapper(typeof(Startup));

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSession();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();


        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}

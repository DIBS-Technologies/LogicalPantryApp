using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.Test.RegistrationService;
using LogicalPantry.Web;
using Newtonsoft.Json;
using System.Text;

namespace LogicalPantry.Tests.Controllers
{
    public class UserControllerTest
    {

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private ApplicationDataContext _context;
        private IRegistrationTestService _registrationTestService;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {

                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }


                        var connectionString = "Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True";

                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));


                        services.AddTransient<IRegistrationTestService, RegistrationTestService>();

                        var serviceProvider = services.BuildServiceProvider();


                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<ApplicationDataContext>();
                            _registrationTestService = scopedServices.GetRequiredService<IRegistrationTestService>();


                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }



    }
}

using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.IntegrationTests
{
    [TestClass]
    public class InformationControllerIntegrationTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private IInformationService _informationService;
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            //Setup configuration to load appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

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

                        // Configure in-memory database or real database as needed
                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");
                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));


                        services.AddTransient<IInformationService, InformationService>();

                    
                        var serviceProvider = services.BuildServiceProvider();

                 
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _informationService = scopedServices.GetRequiredService<IInformationService>();
                        }
                    });
                });

            _client = _factory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7041");
        }

        
        [TestMethod]
        public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
        {
            int tenantId = 5; // Ensure this ID exists in your test database

            var response = await _client.GetAsync($"/TenantB/Information/Get?tenantid={tenantId}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            //var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(response);
            Assert.IsNotNull(responseContent);
            // logo 
        }

        [TestMethod]
        public async Task AddTenant_ShouldAddTenant_WhenModelIsValid()
        {
            var tenantDto = new TenantDto
            {
                TenantName = "Test Tenant",
                AdminEmail = "admin@test.com",
                PaypalId = "paypal123",
                PageName = "test-page",
                Logo = "/Image/test-logo.png",
                Timezone = "UTC"
            };

            // All Data Should be check 
            var form = new MultipartFormDataContent();

            var response = await _client.PostAsync("TenantB/Information/AddTenant", form);

 
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

  
            var isAddSuccessful = await _informationService.GetTenantByNameAsync(tenantDto.TenantName);
            Assert.IsTrue(isAddSuccessful.Success, "Tenant should be added successfully.");
        }

        [TestMethod]
        public async Task AddTenant_ShouldReturnBadRequest_WhenModelIsInvalid() //  
        {
            var tenantDto = new TenantDto
            {
                // Missing required fields for invalid data
                TenantName = null,
                AdminEmail = null,
                PaypalId = null
            };

            var response = await _client.PostAsJsonAsync("/TenantB/Information/AddTenant", tenantDto);

            // Check the response
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task RedirectTenant_ShouldReturnView_WhenTenantExists()
        {
            int tenantId = 1; // Ensure this ID exists in your test database

            var response = await _client.GetAsync($"/Information/RedirectTenant?id={tenantId}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            // Optionally check the content or view rendering
        }

        [TestMethod]
        public async Task Home_ShouldReturnView_WhenPageNameIsValid() // Negative 
        {
            var pageName = "valid-page-name"; // Ensure this page name exists in your test environment

            var response = await _client.GetAsync($"/Information/Home?PageName={pageName}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            // Optionally check the content or view rendering
        }

        [TestMethod]
        public async Task GetTenantIdByName_ShouldReturnOk_WhenTenantExists()
        {
            var tenantName = "Test Tenant";

            var response = await _client.GetAsync($"/Information/GetTenant?tenantName={tenantName}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);
            Assert.AreEqual(tenantName, tenant.TenantName);
        }

        [TestMethod]
        public async Task GetTenantIdByEmail_ShouldReturnOk_WhenUserEmailIsValid()
        {
            var userEmail = "admin@test.com"; // Ensure this email exists in your test environment

            var response = await _client.GetAsync($"/Information/GetTenantByUserEmail?userEmail={userEmail}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);
            Assert.AreEqual(userEmail, tenant.AdminEmail);
        }
    }
}

using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer("Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True")); 

                     
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
        }

        [TestMethod]
        public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
        {
            int tenantId = 1; // Ensure this ID exists in your test database

            var response = await _client.GetAsync($"/Information/Get?tenantid={tenantId}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);
            Assert.AreEqual(tenantId, tenant.Id);
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

            var form = new MultipartFormDataContent();

            
          
            var response = await _client.PostAsync("/Information/AddTenant", form);

     
    
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

  
            var isAddSuccessful = await _informationService.GetTenantByNameAsync(tenantDto.TenantName);
            Assert.IsTrue(isAddSuccessful.Success, "Tenant should be added successfully.");
        }

        [TestMethod]
        public async Task AddTenant_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            var tenantDto = new TenantDto
            {
                // Missing required fields for invalid data
                TenantName = null,
                AdminEmail = null,
                PaypalId = null
            };

            var response = await _client.PostAsJsonAsync("/Information/AddTenant", tenantDto);

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
        public async Task Home_ShouldReturnView_WhenPageNameIsValid()
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

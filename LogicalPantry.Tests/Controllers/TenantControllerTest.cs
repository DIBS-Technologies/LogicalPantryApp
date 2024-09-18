//using LogicalPantry.DTOs.Test.TenantDtos;
//using LogicalPantry.Services.Test.TenantTestService;
//using LogicalPantry.Web;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Newtonsoft.Json;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Text;
//using System.Threading.Tasks;

//namespace LogicalPantry.Tests
//{
//    [TestClass]
//    public class TenantControllerTest
//    {

//        private WebApplicationFactory<Startup> _factory;
//        private HttpClient _client;
//        private ITenantTestService _tenantTestService;
//        private IConfiguration _configuration;

//        [TestInitialize]
//        public void Setup()
//        {
//            //Setup configuration to load appsettings.json
//            var builder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
//                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

//            _configuration = builder.Build();

//            _factory = new WebApplicationFactory<Startup>()
//                .WithWebHostBuilder(builder =>
//                {
//                    builder.ConfigureServices(services =>
//                    {
//                        // Remove the existing ApplicationDbContext registration
//                        var descriptor = services.SingleOrDefault(
//                            d => d.ServiceType == typeof(DbContextOptions<TestApplicationDataContext>));
//                        if (descriptor != null)
//                        {
//                            services.Remove(descriptor);
//                        }

//                        // Add ApplicationDbContext using a real database for testing
//                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");
//                        services.AddDbContext<TestApplicationDataContext>(options =>
//                        {
//                            options.UseSqlServer(connectionString); 
//                        });

//                        // Register the test implementation of ITenantTestService
//                        services.AddTransient<ITenantTestService, TenantTestService>();

//                        // Build the service provider
//                        var serviceProvider = services.BuildServiceProvider();

//                        // Create a scope to obtain a reference to the services
//                        using (var scope = serviceProvider.CreateScope())
//                        {
//                            var scopedServices = scope.ServiceProvider;
//                            _tenantTestService = scopedServices.GetRequiredService<ITenantTestService>();
//                        }
//                    });
//                });

//            _client = _factory.CreateClient();
//        }

//        /// <summary>
//        /// Add Tenant in the database when model is valid.
//        ///</summary>

//        [TestMethod]
//        public async Task AddTenant_ShouldAddTenant_WhenModelIsValid()
//        {
//            var tenantDto = new TenantDto
//            {
//                TenantName = "Test Tenant",
//                AdminEmail = "admin@test.com",
//                PaypalId = "paypal123",
//                PageName = "test-page",
//                Logo = "/Pages/test-logo.png",
//                Timezone = "UTC"
//            };

//            var content = new StringContent(JsonConvert.SerializeObject(tenantDto), Encoding.UTF8, "application/json");
//            var response = await _client.PostAsync("/TenantB/Tenant/AddTenat", content);
          
//            // Check the response
//            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, response.StatusCode);

//            // Verify the tenant information via ITenantTestService
//            var isAddSuccessful = await _tenantTestService.IsAddSuccessful(tenantDto);
//            Assert.IsTrue(isAddSuccessful, "Tenant should be added successfully.");
//        }

//        /// <summary>
//        /// Edit Tenant when model is valid.
//        /// </summary>
//        /// <returns></returns>

//        [TestMethod]
//        public async Task EditTenant_ShouldUpdateTenant_WhenModelIsValid()
//        {
//            var tenantDto = new TenantDto
//            {
//                Id = 15, // Ensure this ID exists in the test database
//                TenantName = "Updated Tenant",
//                AdminEmail = "admin@updated.com",
//                PaypalId = "paypal456",
//                PageName = "updated-page",
//                Logo = "/Pages/updated-logo.png",
//                Timezone = "UTC"
//            };

//            var content = new StringContent(JsonConvert.SerializeObject(tenantDto), Encoding.UTF8, "application/json");


//            var response = await _client.PostAsync("/TenantB/Tenant/EditTenant", content);
//            var responseContent = await response.Content.ReadAsStringAsync();
//            // Check the response
//            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, response.StatusCode);

//            // Verify the tenant information via ITenantTestService
//            var isUpdateSuccessful = await _tenantTestService.IsUpdateSuccessful(tenantDto);
//            Assert.IsTrue(isUpdateSuccessful, "Tenant should be updated successfully.");
//        }

//        /// <summary>
//        ///Edit tenant method  Return a badRequest when model is invalid.
//        /// </summary>
//        /// <returns></returns>
//        [TestMethod]
//        public async Task AddTenant_ShouldReturnBadRequest_WhenModelIsInvalid()
//        {
//            var tenantDto = new TenantDto
//            {
//                // Missing required fields for invalid data
//                TenantName = null,
//                AdminEmail = null,
//                PaypalId = null
//            };

//            // service response 
//            var response = await _client.PostAsJsonAsync("/LP/Tenant/EditTenant", tenantDto);

//            // Check the response
//            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
//        }

//        /// <summary>
//        /// Edit tenant return not found when tenant does not exist in the database.
//        /// </summary>
//        /// <returns></returns>

//        [TestMethod]
//        public async Task EditTenant_ShouldReturnNotFound_WhenTenantDoesNotExist()
//        {
//            /// test data 
//            var tenantDto = new TenantDto
//            {
//                Id = 999, // ID does not exist
//                TenantName = "Nonexistent Tenant",
//                AdminEmail = "nonexistent@test.com",
//                PaypalId = "paypal999",
//                PageName = "nonexistent-page",
//                Logo = "/Pages/nonexistent-logo.png",
//                Timezone = "UTC"
//            };
//            // Api response 
//            var response = await _client.PostAsJsonAsync("/LP/TenantB/EditTenant", tenantDto);

//            // Check the response
//            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
//        }
//    }
//}

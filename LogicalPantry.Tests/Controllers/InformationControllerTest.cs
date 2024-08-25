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
        /// <summary>
        /// Initialize required services  to frtch data from api   works as startup.cs  
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            //web application factory setup 
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                       
                        // generate db context 
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Initialize sql server
                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer("Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True")); 

                        //Register service 
                        services.AddTransient<IInformationService, InformationService>();

                    
                        var serviceProvider = services.BuildServiceProvider();

                        // declare scope 
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _informationService = scopedServices.GetRequiredService<IInformationService>();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        /// <summary>
        ///  check if tenant  with given name exisist in database if found return true else return  false  status code will be ok 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
        {
            int tenantId = 1; // Ensure this ID exists in your test database

            var response = await _client.GetAsync($"/Information/Get?tenantid={tenantId}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);

            // check tenant id is mating  with user 
            Assert.AreEqual(tenantId, tenant.Id);
            // logo 
        }

        /// <summary>
        ///   If Model is Valid then add tenant information
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task AddTenant_ShouldAddTenant_WhenModelIsValid()
        {
            // Data for testing info
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
            // generate form 
            var form = new MultipartFormDataContent();

            var response = await _client.PostAsync("/Information/AddTenant", form);

            // if tenant added return  ststus ocde  200  with ok 
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // checks  record is added in database  if data match then returns true else return false 
            var isAddSuccessful = await _informationService.GetTenantByNameAsync(tenantDto.TenantName);
            Assert.IsTrue(isAddSuccessful.Success, "Tenant should be added successfully.");
        }

        /// <summary>
        ///  When Model is invalid then  return 404 bad request 
        /// </summary>
        /// <returns></returns>
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

            var response = await _client.PostAsJsonAsync("/Information/AddTenant", tenantDto);

            // Check the response
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Returns index page with data   of tenent based on tenant id 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RedirectTenant_ShouldReturnView_WhenTenantExists()
        {
            int tenantId = 1; // Ensure this ID exists in your test database

            //redirect to home page 
            var response = await _client.GetAsync($"/Information/RedirectTenant?id={tenantId}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            // Optionally check the content or view rendering
        }
        /// <summary>
        ///    if user entered Tenant name is correct  then it should return page name  rendered in iframe
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Home_ShouldReturnView_WhenPageNameIsValid() // Negative 
        {
            // test data 
            var pageName = "Test Tenant"; // Ensure this page name exists in your test environment

            // api ressponse 
            var response = await _client.GetAsync($"/Information/Home?PageName={pageName}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            // Optionally check the content or view rendering
        }

        /// <summary>
        ///   checks tenant with page  name availble in database 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenantIdByName_ShouldReturnOk_WhenTenantExists()
        {
            // test data 
            var tenantName = "Test Tenant";

            // get service response
            var response = await _client.GetAsync($"/Information/GetTenant?tenantName={tenantName}");

            // status 
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // 
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            //
            Assert.IsNotNull(tenant);

            // comapre test and actual data 
            Assert.AreEqual(tenantName, tenant.TenantName);
        }

        /// <summary>
        ///  Get Tenant Infomation by email
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenantIdByEmail_ShouldReturnOk_WhenUserEmailIsValid()
        {

            // mail for test 
            var userEmail = "admin@test.com"; 

            // Api Call
            var response = await _client.GetAsync($"/Information/GetTenantByUserEmail?userEmail={userEmail}");

            // status respone
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);

            // check  email with response
            Assert.AreEqual(userEmail, tenant.AdminEmail);
        }
    }
}

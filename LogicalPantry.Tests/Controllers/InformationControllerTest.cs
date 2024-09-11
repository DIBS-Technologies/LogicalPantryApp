
using LogicalPantry.DTOs.Test.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web;
using Microsoft.AspNetCore.Http;
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

                        // declare scope 
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

        /// <summary>
        ///  check if tenant  with given name exisist in database if found return true else return  false  status code will be ok 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenant_ShouldReturnOk_WhenTenantExists()
        {
            int tenantId = 17; //Ensure this ID exists in your test database

            var response = await _client.GetAsync($"/LogicalPantry/Information/Get?tenantid={tenantId}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            //var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            //Assert.IsNotNull(tenant);

            // check tenant id is mating  with user 
            //Assert.AreEqual(tenantId, tenant.Id);
            // logo 
        }

        /// <summary>
        ///If Model is Valid then add tenant information
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddTenant_ShouldAddTenant_WhenModelIsValid()
        {
            // Create the TenantDto
            var tenantDto = new TenantDto
            {
                TenantName = "Test Tenant",
                AdminEmail = "admin@test.com",
                PaypalId = "paypal123",
                PageName = "test-page",
                Logo = "/Image/test-logo.png",
                Timezone = "UTC"
            };

            //// Create MultipartFormDataContent to send the form data
            var form = new MultipartFormDataContent();

            // Add each property of TenantDto as form data with keys matching the property names
            form.Add(new StringContent(tenantDto.TenantName), nameof(tenantDto.TenantName));
            form.Add(new StringContent(tenantDto.AdminEmail), nameof(tenantDto.AdminEmail));
            form.Add(new StringContent(tenantDto.PaypalId), nameof(tenantDto.PaypalId));
            form.Add(new StringContent(tenantDto.PageName), nameof(tenantDto.PageName));
            form.Add(new StringContent(tenantDto.Logo), nameof(tenantDto.Logo));
            form.Add(new StringContent(tenantDto.Timezone), nameof(tenantDto.Timezone));

            // Create a mock IFormFile
            var fileName = "test-logo.png";
            var contentType = "image/png";
            var fileContent = new byte[] { /* file content */ }; // Example file content as byte array
            var fileStream = new MemoryStream(fileContent);
            var logoFile = new FormFile(fileStream, 0, fileContent.Length, "LogoFile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            // Add the IFormFile to the form with the correct key name
            form.Add(new StreamContent(logoFile.OpenReadStream()), "LogoFile", logoFile.FileName);

            // Send the POST request to the correct URL
            var response = await _client.PostAsync("/LogicalPantry/Information/AddTenant", form);

            // Assert the response status code
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }






        /// <summary>
        ///  When Model is invalid then  return 404 bad request 
        /// </summary>
        /// <returns></returns>
        
        [TestMethod]
        public async Task AddTenant_ShouldReturnBadRequest_WhenModelIsInvalid()   
        {
            var tenantDto = new TenantDto
            {
                // Missing required fields for invalid data
                Id = 0,
                TenantName = null,
                AdminEmail = null,
                PaypalId = null
            };

            var response = await _client.PostAsJsonAsync("/LogicalPantry/Information/AddTenant", tenantDto);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Check the response
            Assert.IsNull(responseContent);
            
        }

        /// <summary>
        /// Returns index page with data   of tenent based on tenant id 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task RedirectTenant_ShouldReturnView_WhenTenantExists()
        {
            int tenantId = 17; // Ensure this ID exists in your test database

            //redirect to home page 
            var response = await _client.GetAsync($"/LogicalPantry/Information/RedirectTenant?id={tenantId}");
            var content = await response.Content.ReadAsStringAsync();

            //Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            //check the content or view rendering
            Assert.IsTrue(content.Contains("Home"));

        }


        /// <summary>
        ///if user entered Tenant name is correct  then it should return page name  rendered in iframe
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Home_ShouldReturnView_WhenTenantNameIsValid() 
        {
            // test data 
            var pageName = "Index"; // Ensure this page name exists in your test environment

            // api ressponse 
            var response = await _client.GetAsync($"/LP/Information/Home?PageName={pageName}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            
        }

        /// <summary>
        ///checks tenant with page  name availble in database 
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenantIdByName_ShouldReturnOk_WhenTenantExists()
        {
            // test data 
            var tenantName = "LP";

            // get service response
            var response = await _client.GetAsync($"/LP/Information/GetTenant?tenantName={tenantName}");

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
            var userEmail = "jayantgaikwad410@gmail.com"; 

            // Api Call
            var response = await _client.GetAsync($"/LP/Information/GetTenantByUserEmail?userEmail={userEmail}");

            // status respone
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);
        }
    }
}

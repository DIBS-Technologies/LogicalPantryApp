﻿
using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.Test.TenantTestService;
using LogicalPantry.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
       // private IInformationService _informationService;
        private ITenantTestService _tenantTestService;
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


                        services.AddTransient<ITenantTestService, TenantTestService>();

                    
                        var serviceProvider = services.BuildServiceProvider();

                        // declare scope 
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _tenantTestService = scopedServices.GetRequiredService<ITenantTestService>();
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
                TenantName = "Logic",
                AdminEmail = "Shrikantdandiledib1@gmail.com",
                PaypalId = "Shrikantdandiledib1@gmail.com",
                PageName = "Index.html",
                Logo = "/Image/838a7921-35e3-4a75-a9df-e6e6541aef30.svg",
                Timezone = "US/Eastern"
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
            var fileName = "/5114218a-5aba-482f-a342-4bbb5cc184e3.svg";
            var contentType = "image/svg";
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
            var response = await _client.PostAsync("/Logic/Information/AddTenant", form);

            // Act: Retrieve the tenant from the database to verify it was added
            var result = await _tenantTestService.IsAddSuccessful(tenantDto);

            //Compare the data
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(tenantDto.TenantName, result.Data.TenantName);
            Assert.AreEqual(tenantDto.PageName, result.Data.PageName);
            Assert.AreEqual(tenantDto.PaypalId, result.Data.PaypalId);
            Assert.AreEqual(tenantDto.AdminEmail, result.Data.AdminEmail);
            Assert.AreEqual(tenantDto.Timezone, result.Data.Timezone);


            // Assert the response status code
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

      


        /// <summary>
        /// Tests whether the Home method returns the correct view when the tenant name is valid.
        /// This simulates a request to the homepage with a valid tenant name and verifies 
        /// that the server responds with an HTTP OK status and non-null content.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Home_ShouldReturnView_WhenTenantNameIsValid()
        {
            //Home page passing with tenant name 
            var response = await _client.GetAsync($"/LP");

            // Act: Read response content
            var result = await response.Content.ReadAsStringAsync();

            // Assert: Verify the response status is OK and the response is not null
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(response);
        }

        /// <summary>
        /// Tests whether the Home method returns an HTTP NotFound status 
        /// when an incorrect or invalid tenant name is passed. This ensures the method 
        /// handles missing tenants and returns the expected error response.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Home_ShouldReturnBadRequest_WhenTenantisIncorrect()
        {
            // Act: Passing an incorrect tenant name to simulate a request to a non-existent tenant
            var response = await _client.GetAsync("/L");

            // Assert: Verify that the status code returned is NotFound (404)
            var tenant = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Tests whether the Home method returns an HTTP BadRequest status when no tenant name is passed.
        /// This ensures that when a tenant is not provided or doesn't exist, the method responds appropriately 
        /// with an error message indicating a bad request.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Home_ShouldReturnBadRequest_WhenTenantisNotExists()
        {
            // Act: Simulate a request with a missing or invalid tenant
            var response = await _client.GetAsync("/");

            // Assert: Verify that the response status is BadRequest (400)
            var tenant = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
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
            var adminEmail = "jayantgaikwad410@gmail.com"; 

            // Api Call
            var response = await _client.GetAsync($"/LogicalPantry/Information/GetTenantByUserEmail?userEmail={adminEmail}");

            // status respone
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>();
            Assert.IsNotNull(tenant);

            // comapre test and actual data 
            Assert.AreEqual(adminEmail, tenant.AdminEmail);
        }


        /// <summary>
        /// Test to verify if GetTenantIdByEmail method returns BadRequest when the user email is invalid or not found.
        /// This ensures that the system handles invalid or missing email addresses properly.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenantIdByEmail_ShouldReturnBadRequest_WhenUserEmailIsInvalid()
        {
            // Invalid email (email that does not exist in the system)
            var invalidEmail = "nonexistentemail@example.com";

            // Act: Make the API call with an invalid email
            var response = await _client.GetAsync($"/LogicalPantry/Information/GetTenantByUserEmail?userEmail={invalidEmail}");

            // Assert: Verify that the status code returned is BadRequest (400) or NotFound (404) depending on your API design
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test to verify if GetTenantIdByEmail method returns BadRequest when the user email is blank.
        /// This ensures that the API handles empty email inputs gracefully and responds with a proper error.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTenantIdByEmail_ShouldReturnBadRequest_WhenUserEmailIsBlank()
        {
            // Blank email
            var blankEmail = "";

            // Act: Make the API call with a blank email
            var response = await _client.GetAsync($"/LogicalPantry/Information/GetTenantByUserEmail?userEmail={blankEmail}");

            // Assert: Verify that the status code returned is BadRequest (400)
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }






    }
}

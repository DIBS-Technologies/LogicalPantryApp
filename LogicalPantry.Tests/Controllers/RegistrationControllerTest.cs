using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using LogicalPantry.Services.Test.RegistrationService;
using LogicalPantry.Web;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using LogicalPantry.DTOs.Test.UserDtos;

namespace LogicalPantry.Tests
{
    [TestClass]
    public class RegistrationControllerTest
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private ApplicationDataContext _context;
        private IRegistrationTestService _registrationTestService;
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


                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

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
            _client.BaseAddress = new Uri("https://localhost:7041");
        }


        /// <summary>
        /// Register user with valid data
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task Register_ShouldRedirectToUserCalendar_WhenRegistrationIsSuccessful()
        {
            var userDto = new UserDto
            {
                TenantId = 17,
                FullName = "Sample User3",
                Email = "sampleUser@gmail.com",
                PhoneNumber = "+1 (123) 456-7890",
                Address = "Sample Address"
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/LogicalPantry/Registration/Register", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var userInfo =  _registrationTestService.GetUser(userDto);

            Assert.IsNotNull(userInfo);
            Assert.IsTrue(userInfo.Success);
            Assert.AreEqual("User details are correct.", userInfo.Message);

        }

        /// <summary>
        /// Return Bad Request when pass the Dto null.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenUserDtoIsNull()
        {
            
            var response = await _client.PostAsJsonAsync("/LogicalPantry/Registration/Register", (UserDto)null);
            var contentResponse = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(response);
            Assert.IsTrue(contentResponse.Contains("Object reference not set to an instance of an object"));
        }

        /// <summary>
        /// Return BadRequest when pass the tenantId is Invalid
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenTenantIdIsInvalid()
        {
            var userDto = new UserDto
            {
              
                FullName = "Sample User",
                Email = "sampleUser@gmail.com",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };

            
            var response = await _client.PostAsJsonAsync("/LogicalPantry/Registration/Register", userDto);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Check if the response content contains the expected TempData message.
            Assert.IsTrue(responseContent.Contains("Failed to Save User server error."));
        }

       
        /// <summary>
        /// Return BadRequest when Email Already exist 
        /// </summary>
        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_EmailAlreadyExist()
        {
            var userDto = new UserDto
            {
                TenantId = 1,
                FullName = "Sample User",
                Email = "swappnilfromdibs@gmail.com",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };
            var response = await _client.PostAsJsonAsync("/LP/Registration/Register", userDto);
            var responseContent = await response.Content.ReadAsStringAsync();
            // Check if the response content contains the expected TempData message.
            Assert.IsTrue(responseContent.Contains("Failed to Save User server error."));
        }
    }
}

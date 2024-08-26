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
using Microsoft.Extensions.Configuration;

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

        [TestMethod]
        public async Task Register_ShouldRedirectToUserCalendar_WhenRegistrationIsSuccessful()
        {
            var userDto = new UserDto
            {
                TenantId = 5,
                FullName = "Sample User",
                Email = "swappnilfromdibs2@gmail.com",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/TenantB/Registration/Register", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            Assert.AreEqual(System.Net.HttpStatusCode.Redirect, response.StatusCode);
            var redirectUri = response.Headers.Location.ToString();
            Assert.IsTrue(redirectUri.Contains("/TimeSlot/UserCalendar"));


            var userInfo =  _registrationTestService.GetUser(userDto);

            Assert.IsNotNull(userInfo);
            Assert.IsTrue(userInfo.Success);
            Assert.AreEqual("User details are correct.", userInfo.Message);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenUserDtoIsNull()
        {
           
            var response = await _client.PostAsJsonAsync("/Registration/Register", (UserDto)null);

            
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenTenantIdIsInvalid()
        {
            var userDto = new UserDto
            {
                TenantId = 0, 
                FullName = "Sample User",
                Email = "sampleUser@gmail.com",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };

            
            var response = await _client.PostAsJsonAsync("/Registration/Register", userDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenNameEmailPhoneNumberAreNull()
        {
            var userDto = new UserDto
            {
                TenantId = 1,
                FullName = null,
                Email = null,
                PhoneNumber = null
            };

            
            var response = await _client.PostAsJsonAsync("/Registration/Register", userDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
        {
            var userDto = new UserDto
            {
                TenantId = 1,
                FullName = "Sample User",
                Email = "invalid-email-format",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };

            
            var response = await _client.PostAsJsonAsync("/Registration/Register", userDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Register_ShouldReturnBadRequest_WhenTenantIdMismatch()
        {
            var userDto = new UserDto
            {
                TenantId = 1,
                FullName = "Sample User",
                Email = "sampleUser@gmail.com",
                PhoneNumber = "1234567890",
                Address = "Sample Address"
            };

          
            _client.DefaultRequestHeaders.Add("TenantId", "2");

            
            var response = await _client.PostAsJsonAsync("/Registration/Register", userDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

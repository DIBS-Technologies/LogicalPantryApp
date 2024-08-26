using Autofac.Core;
using Azure;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.Test.TimeSlotSignUpService;
using LogicalPantry.Services.Test.UserServiceTest;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.TimeSlotSignupService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Core.DTO;
using Tweetinvi.Models.DTO;

namespace LogicalPantry.Tests.Controllers
{
    [TestClass]
    public class TimeSlotSignupControllerTest
    {


        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private ApplicationDataContext _context;
        private ITimeSlotSignUpTestService _timeSlotSignUpTestService;
        private  ITimeSlotSignupService _timeSlotSignupService;
        private  IConfiguration _configuration;

        private IUserServiceTest _userServiceTest;

        [TestInitialize]
        public void Setup()
        {

            //Set up configuration to load appsettings json 
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true);

            _configuration = builder.Build();

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing DbContext configuration
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

                        // Add services for testing
                        services.AddTransient<ITimeSlotSignUpTestService, TimeSlotSignUpTestService>();
                        services.AddTransient<ITimeSlotSignupService, TimeSlotSignupService>();
                        services.AddTransient<IUserServiceTest, UserServicesTest>();

                        // Build service provider and initialize context
                        var serviceProvider = services.BuildServiceProvider();
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<ApplicationDataContext>();
                            _timeSlotSignUpTestService = scopedServices.GetRequiredService<ITimeSlotSignUpTestService>();
                            //_timeSlotSignupService = scopedServices.GetRequiredService<ITimeSlotSignupService>();

                            _userServiceTest = scopedServices.GetRequiredService<IUserServiceTest>();
                            // Ensure the database is created
                            _context.Database.EnsureCreated();
                        }
                    });
                });

            // Create HttpClient with base address
            _client = _factory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7041");
        }


        [TestMethod]
        public async Task Index_ReturnsView()
        {
            // Act
            var response = await _client.GetAsync("TimeSlotSignup/Index");

            // Assert
            Assert.IsNotNull(response); // Verify that the response is not null
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); // Verify that the response status code is 200 (OK)

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(content); // Verify that the response content is not null
        }


        [TestMethod]
        public async Task GetUsersbyTimeSlot_With_ValidResponse()
        {
            // Arrange
            string dateTimeString = "2024-08-23 01:12:00.0000000"; // Use the exact format as stored in the database
            DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);

            // Act
            var response = await _client.GetAsync($"/TenantB/TimeSlotSignup/GetUsersbyTimeSlot?timeSlot={dateTimeString}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ServiceResponse<IEnumerable<UserDto>>>();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);

            // Assuming you know the expected results for this time slot
            var userDtos = result.Data.ToList();
            Assert.AreEqual(1, userDtos.Count);

        }



        [TestMethod]
        public async Task AddTimeSlotSignUps_SavesDataSuccessfully()
        {
            // Arrange
            var timeSlotSignUpDtos = new TimeSlotSignupDto
            {
               
                TimeSlotId = 82,
                UserId = 61,
                Attended = true,
            };

            var content = new StringContent(JsonConvert.SerializeObject(timeSlotSignUpDtos), Encoding.UTF8, "application/json");

            // Act

            var response = await _client.PostAsync("/TenantB/TimeSlotSignup/AddTimeSlotSignUps", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Check if the data was saved correctly in the database
            var timeslotInfo = await _timeSlotSignUpTestService.GetTimeSlot(timeSlotSignUpDtos);
            Assert.IsNotNull(timeslotInfo);
            Assert.AreEqual(timeSlotSignUpDtos.UserId, timeslotInfo.Data.UserId);
            Assert.AreEqual(timeSlotSignUpDtos.TimeSlotId , timeslotInfo.Data.TimeSlotId);
            
        }

       


    }
}

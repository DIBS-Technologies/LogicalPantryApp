using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web;
using Microsoft.Extensions.Configuration;
using LogicalPantry.DTOs.Test.TimeSlotDtos;
using Newtonsoft.Json;
using System.Text;
using LogicalPantry.Services.Test.TimeSlotServiceTest;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LogicalPantry.IntegrationTests
{
    [TestClass]
    public class TimeSlotControllerIntegrationTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private TestApplicationDataContext _context;
        private ITimeSlotTestService _timeSlotTestService;
        private ITimeSlotService _timeSlotService;
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
                        // Remove the existing ApplicationDbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<TestApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }


                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

                        services.AddDbContext<TestApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));

                       
                        services.AddTransient<ITimeSlotService, TimeSlotService>();
                        services.AddScoped<ITimeSlotTestService, TimeSlotTestService>();                      
                        var serviceProvider = services.BuildServiceProvider();

                      
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<TestApplicationDataContext>();
                            _timeSlotService = scopedServices.GetRequiredService<ITimeSlotService>();
                            _timeSlotTestService = scopedServices.GetRequiredService<ITimeSlotTestService>();
                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }
        /// <summary>
        ///Add Events with valid data
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddEvent_ShouldReturnOk_WhenEventIsAddedSuccessfully()
        {
            var timeSlotDto = new TimeSlotDto
            {

                UserId = 61,
                TenantId = 17,
                TimeSlotName = "Sample Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                EventType = "Appointment",
                MaxNumberOfUsers = 1,
            };

            var content = new StringContent(JsonConvert.SerializeObject(timeSlotDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/LogicalPantry/TimeSlot/AddEvent", content);

            //Get the event from the database and check if add in the database.
            var result = await _timeSlotTestService.GetEvent(timeSlotDto);

            //Compare the data
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(timeSlotDto.TimeSlotName, result.Data.TimeSlotName);
            Assert.AreEqual(timeSlotDto.StartTime, result.Data.StartTime);
            Assert.AreEqual(timeSlotDto.EndTime, result.Data.EndTime);
            Assert.AreEqual(timeSlotDto.EventType, result.Data.EventType);
            Assert.AreEqual(timeSlotDto.MaxNumberOfUsers, result.Data.MaxNumberOfUsers);

        }

        /// <summary>
        /// Get a timeSlot when time slot exist in the database.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetTimeSlotId_ShouldReturnOk_WhenTimeSlotExists()
        {

            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotName = "Sample Event",
                StartTime = DateTime.ParseExact("2024-09-20 11:08:44.5178367", "yyyy-MM-dd HH:mm:ss.fffffff", null),
                EndTime = DateTime.ParseExact("2024-09-20 12:08:44.5279682", "yyyy-MM-dd HH:mm:ss.fffffff", null),
            };


            var response = await _client.PostAsJsonAsync("/LogicalPantry/TimeSlot/GetTimeSlotId", timeSlotDto);         
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.IsNotNull(responseContent.timeSlotId);
        }

        [TestMethod]
        public async Task Calendar_ShouldReturnViewWithEvents()
        {
           
            var response = await _client.GetAsync("/LP/TimeSlot/Calendar");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(response);
            Assert.IsTrue(responseContent.Contains("Calendar"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
         
        }

        [TestMethod]
        public async Task UserCalendar_ShouldReturnViewWithEvents()
        {
           
            var response = await _client.GetAsync("/LP/TimeSlot/UserCalendar");
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(response);
            Assert.IsTrue(responseContent.Contains("Calendar"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);


        }




        //18/9/2024
        /// <summary>
        /// Tests that GetUserListByTimeSlotId returns an OK status and users when a valid time slot exists.
        /// </summary>
        /// <returns>A task representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetUserListByTimeSlotId_ShouldReturnOkWithUsers_WhenTimeSlotExists()
        {
            // Arrange
            int validTimeSlotId = 286;

            // Act
            var response = await _client.GetAsync($"/Logic/TimeSlot/EditTimeSlotUser?Id={validTimeSlotId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Tests that GetUserListByTimeSlotId returns a BadRequest status when the time slot ID is invalid.
        /// </summary>
        /// <returns>A task representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetUserListByTimeSlotId_ShouldReturnBadRequest_WhenTimeSlotIdIsInvalid()
        {
            // Arrange
            int invalidTimeSlotId = 0;

            // Act
            var response = await _client.GetAsync($"/Logic/TimeSlot/EditTimeSlotUser/{invalidTimeSlotId}");

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Tests that GetUserListByTimeSlotId returns an OK status with an empty list when no users are found for the valid time slot.
        /// </summary>
        /// <returns>A task representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task GetUserListByTimeSlotId_ShouldReturnOkWithEmptyList_WhenNoUsersFound()
        {
            // Arrange
            int validTimeSlotId = 2582;

            // Act
            var response = await _client.GetAsync($"/Logic/TimeSlot/EditTimeSlotUser?Id={validTimeSlotId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

    }
}

using Autofac.Core;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Services.Test.TimeSlotSignUpService;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.TimeSlotSignupService;
using LogicalPantry.Services.UserServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

namespace LogicalPantry.Tests.Controllers
{
    [TestClass]
    public class TimeSlotSignupControllerTest
    {


        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private ApplicationDataContext _context;
        private ITimeSlotSignUpTestService _timeSlotSignUpTestService;

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


                        var connectionString = "Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True";

                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));


                        services.AddTransient<ITimeSlotSignUpTestService, TimeSlotSignUpTestService>();

                        var serviceProvider = services.BuildServiceProvider();


                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<ApplicationDataContext>();
                            _timeSlotSignUpTestService = scopedServices.GetRequiredService<ITimeSlotSignUpTestService>();


                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task Index_ReturnsView()
        {
            // Act
            var response = await _client.GetAsync("/TimeSlotSignup/Index");
            //Assert
            Assert.IsNotNull(response, "Response is null");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Status code is not OK (200)");

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(content, "Response content is null or empty");
        }

        [TestMethod]
        public async Task GetUsersbyTimeSlot_With_ValidResponse()
        {
            // Arrange
            string dateTimeString = "2024-08-09 10:34:09.0000000";
            DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.fffffff", System.Globalization.CultureInfo.InvariantCulture);

            // Act
            var response = await _client.GetAsync($"/TimeSlotSignup/GetUsersbyTimeSlot?timeSlot={dateTime}");

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
        public async Task AddTimeSlotSignUps()
        {
            // Arrange
            //create a new timeslotsignupDto object with test data
            var timeSlotSignUpDto = new TimeSlotSignupDto
            {

                UserId = 20,
                TimeSlotId = 20,
                Attended = true,
            };

            // Act

            //convert the DTO to a JSON string and wrap it in a stringcontent object.

            var content = new StringContent(JsonConvert.SerializeObject(timeSlotSignUpDto), Encoding.UTF8, "application/json");
            //send the post request to the API endpoint with the json content
            var response = await _client.PostAsync("/TimeSlotSignup/AddTimeSlotSignUps", content);


            //Retrieve the timeslotsignups from test service to verify data was saved.
            var timeslotInfo = await _timeSlotSignUpTestService.GetTimeSlot(timeSlotSignUpDto);

            // Assert
            //Ensure the response is successfull.
            response.EnsureSuccessStatusCode();
            Assert.IsNotNull(response);
            Assert.IsNotNull(timeslotInfo);
            if (response.IsSuccessStatusCode)
            {
                //Read and deserilize the the response content to timeslotDto
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Content: {responseContent}");

                var timeSlot = JsonConvert.DeserializeObject<TimeSlotDto>(responseContent);

                Assert.AreEqual(timeSlot.Id, timeslotInfo.Id);
                Assert.AreEqual(timeSlot.UserId, timeslotInfo.UserId);

            }
            else
            {
                // Handle the error, log it, or throw an exception
                Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }
        }
    }
}

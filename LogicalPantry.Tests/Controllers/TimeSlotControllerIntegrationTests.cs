using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.Services.TimeSlotServices;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web;

namespace LogicalPantry.IntegrationTests
{
    [TestClass]
    public class TimeSlotControllerIntegrationTests
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private ApplicationDataContext _context;
        private ITimeSlotService _timeSlotService;
        private IUserService _userService;
        private IInformationService _informationService;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing ApplicationDbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        
                        var connectionString = "Server=localhost;Database=TestDatabase;Trusted_Connection=True;";

                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));

                       
                        services.AddTransient<ITimeSlotService, TimeSlotService>();
                        services.AddTransient<IUserService, UserService>();
                        services.AddTransient<IInformationService, InformationService>();

                      
                        var serviceProvider = services.BuildServiceProvider();

                      
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<ApplicationDataContext>();
                            _timeSlotService = scopedServices.GetRequiredService<ITimeSlotService>();
                            _userService = scopedServices.GetRequiredService<IUserService>();
                            _informationService = scopedServices.GetRequiredService<IInformationService>();

                          
                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }
        /// <summary>
        ///   Add  Events
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AddEvent_ShouldReturnOk_WhenEventIsAddedSuccessfully()
        {
            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotName = "Sample Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

          
            var response = await _client.PostAsJsonAsync("/TimeSlot/AddEvent", timeSlotDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SaveEvent_ShouldAddNewEvent_WhenIdIsZero()
        {
            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotName = "New Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            
            var response = await _client.PostAsJsonAsync("/TimeSlot", timeSlotDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

        
            var events = await _timeSlotService.GetAllEventsAsync();
            Assert.IsTrue(events.Any(e => e.TimeSlotName == "New Event"));
        }

        [TestMethod]
        public async Task DeleteEvent_ShouldReturnOk_WhenEventIsDeleted()
        {
            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotName = "Event to Delete",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            // First add the event
            await _client.PostAsJsonAsync("/TimeSlot/AddEvent", timeSlotDto);

            // Get the event ID
            var events = await _timeSlotService.GetAllEventsAsync();
            var eventToDelete = events.SingleOrDefault(e => e.TimeSlotName == "Event to Delete");

            
            var deleteResponse = await _client.PostAsJsonAsync("/TimeSlot", new TimeSlotDto { Id = eventToDelete.Id });

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, deleteResponse.StatusCode);

            // Verify that the event was deleted
            events = await _timeSlotService.GetAllEventsAsync();
            Assert.IsFalse(events.Any(e => e.TimeSlotName == "Event to Delete"));
        }

        [TestMethod]
        public async Task GetTimeSlotId_ShouldReturnOk_WhenTimeSlotExists()
        {
            var timeSlotDto = new TimeSlotDto
            {
                TimeSlotName = "Existing Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2)
            };

           
            await _client.PostAsJsonAsync("/TimeSlot/AddEvent", timeSlotDto);

            var response = await _client.PostAsJsonAsync("/TimeSlot/GetTimeSlotId", timeSlotDto);

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.IsNotNull(responseContent.timeSlotId);
        }

        [TestMethod]
        public async Task Calendar_ShouldReturnViewWithEvents()
        {
           
            var response = await _client.GetAsync("/TimeSlot/Calendar");

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
         
        }

        [TestMethod]
        public async Task UserCalendar_ShouldReturnViewWithEvents()
        {
           
            var response = await _client.GetAsync("/TimeSlot/UserCalendar");

            
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
          
        }
    }
}

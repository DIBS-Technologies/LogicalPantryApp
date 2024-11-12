using LogicalPantry.DTOs.Test.PayPalSettingDtos;
using LogicalPantry.Services.Test.TimeSlotSignUpService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Tests.Controllers
{
    [TestClass]
    public class DonationControllerTest
    {

        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private TestApplicationDataContext _context;
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {

            //Setup configuration to load appsettings.json

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional:true, reloadOnChange:true);

            _configuration = builder.Build();

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {

                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<TestApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }


                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

                        services.AddDbContext<TestApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));

                    
                        services.AddTransient<ITimeSlotSignUpTestService, TimeSlotSignUpTestService>();

                        var serviceProvider = services.BuildServiceProvider();


                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<TestApplicationDataContext>();
                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        /// <summary>
        ///Tests the method to ensure to return view when valid session data.
        /// </summary>
        
        [TestMethod]
        public async Task PayPal_ReturnsView_WithValidSessionData()
        {
            // Act
            var response = await _client.GetAsync("/LogicalPantry/Donation/PayPal");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Extract the content
            var content = await response.Content.ReadAsStringAsync();

            // Check that the response contains the view
            Assert.IsTrue(content.Contains("PayPal"));
        }

        /// <summary>
        /// Test the method to ensure to Complete Payment with valida data.
        /// </summary>
        

        [TestMethod]
        public async Task CompletePayment_ValidData_ReturnsOk()
        {
            // Arrange
            var paymentDto = new PayPalPaymentDto
            {
                OrderId = "12345",
                PayerId = "payer123",
                PaymentId = "12345",
            };

            var content = new StringContent(JsonConvert.SerializeObject(paymentDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/LogicalPantry/Donation/CompletePayment", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Return bad request when Invalid data for Complete Payment.
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task CompletePayment_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var paymentDto = new PayPalPaymentDto
            {
                OrderId = "", // Invalid OrderId
                PayerId = ""  // Invalid PayerId
            };

            var content = new StringContent(JsonConvert.SerializeObject(paymentDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/LogicalPantry/Donation/CompletePayment", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual("Invalid payment details", responseContent);
        }
    }
}

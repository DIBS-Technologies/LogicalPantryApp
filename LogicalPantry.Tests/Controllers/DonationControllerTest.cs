using LogicalPantry.DTOs.PayPalSettingDtos;
using LogicalPantry.Services.Test.TimeSlotSignUpService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
        private ApplicationDataContext _context;
       

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
                            _context.Database.EnsureCreated();
                        }
                    });
                });

            _client = _factory.CreateClient();
        }

        [TestMethod]
        public async Task PayPalReturnview()
        {
            // Act
            var response = await _client.GetAsync("/Donation/PayPal");
            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(content, "Response content is null or empty");
        }

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
            var response = await _client.PostAsync("/Donation/CompletePayment", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

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
            var response = await _client.PostAsync("/Donation/CompletePayment", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("Invalid payment details", responseContent);
        }
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.Test.UserService;
using LogicalPantry.Web;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using LogicalPantry.Services.Test.UserServiceTest;

namespace LogicalPantry.Tests
{
    [TestClass]
    public class UserControllerTest
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private IUserServiceTest _userTestService;

        // This method is called before each test method is run.
        [TestInitialize]
        public void Setup()
        {
            // Create a factory for the web application, configuring it for testing.
            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing DbContext registration (use an in-memory database for testing).
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<ApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        // Register an in-memory database for testing.
                        var connectionString = "Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True";

                        services.AddDbContext<ApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));

                        // Register the test service.
                        services.AddTransient<IUserServiceTest, UserServicesTest>();

                        var serviceProvider = services.BuildServiceProvider();

                        // Create a scope to get the DbContext and the test service.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _userTestService = scopedServices.GetRequiredService<IUserServiceTest>();

                            // Ensure the in-memory database is created.
                            var db = scopedServices.GetRequiredService<ApplicationDataContext>();
                            db.Database.EnsureCreated();
                        }
                    });
                });

            // Create an HttpClient for sending requests to the API.
            _client = _factory.CreateClient();
        }

        // Test method for updating users.
        [TestMethod]
        public async Task UpdateUsers_ShouldReturnSuccess_WhenUpdateIsValid()
        {
            //  Define initial users and add them to the in-memory database.
            var initialUsers = new List<UserDto>
            {
                new UserDto { Id = 1, FullName = "Initial User 1", Email = "initialuser1@example.com", PhoneNumber = "1234567890" },
                new UserDto { Id = 2, FullName = "Initial User 2", Email = "initialuser2@example.com", PhoneNumber = "0987654321" }
            };

            // Add initial users.
            foreach (var userDto in initialUsers)
            {
                var responseAdd = await _client.PostAsJsonAsync("/User/AddUser", userDto);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, responseAdd.StatusCode);
            }

            // Define updated user data.
            var updatedUsers = new List<UserDto>
            {
                new UserDto { Id = 1, FullName = "Updated User 1", Email = "updateduser1@example.com", PhoneNumber = "1111111111" },
                new UserDto { Id = 2, FullName = "Updated User 2", Email = "updateduser2@example.com", PhoneNumber = "2222222222" }
            };

            // Send a PUT request to update users.
            var response = await _client.PutAsJsonAsync("/User/UpdateUsers", updatedUsers);

            // Assert: Verify the response status code and that the users were updated.
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // Check the database to ensure the users are updated.
            foreach (var updatedUserDto in updatedUsers)
            {
                var user = await _userTestService.GetUserByIdAsync(updatedUserDto.Id);
                Assert.IsNotNull(user);
                // Ensure the updated user's data matches what was sent in the request.
                Assert.AreEqual(updatedUserDto.FullName, user.FullName);
                Assert.AreEqual(updatedUserDto.Email, user.Email);
                Assert.AreEqual(updatedUserDto.PhoneNumber, user.PhoneNumber);
            }
        }

        // Test method for adding a user.
        [TestMethod]
        public async Task AddUser_ShouldReturnSuccess_WhenAdditionIsValid()
        {
            //  Define a new user to add.
            var newUser = new UserDto
            {
                FullName = "New User",
                Email = "newuser@example.com",
                PhoneNumber = "3333333333"
            };

            // Send a POST request to add the new user.
            var response = await _client.PostAsJsonAsync("/User/AddUser", newUser);

            // Verify the response status code is OK (200).
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // Retrieve the added user from the database to confirm
            var addedUser = await _userTestService.GetUserByEmailAsync(newUser.Email);
            Assert.IsNotNull(addedUser);

            // added data matches what was sent in the request.
            Assert.AreEqual(newUser.FullName, addedUser.FullName);
            Assert.AreEqual(newUser.Email, addedUser.Email);
            Assert.AreEqual(newUser.PhoneNumber, addedUser.PhoneNumber);
        }

        // Test method for deleting a user.
        [TestMethod]
        public async Task DeleteUser_ShouldReturnSuccess_WhenDeletionIsValid()
        {
            //  Define a user to deleted.
            var userId = 1;

            // Send a DELETE request to remove the user.
            var response = await _client.DeleteAsync($"/User/DeleteUser/{userId}");

            //  Verify the response status code is OK (200).
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // Confirm the user was removed from the database.
            var deletedUser = await _userTestService.GetUserByIdAsync(userId);
            Assert.IsNull(deletedUser);
        }
    }
}

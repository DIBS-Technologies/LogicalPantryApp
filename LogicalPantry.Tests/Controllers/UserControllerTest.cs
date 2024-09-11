using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.DTOs.UserDtos;

using LogicalPantry.Web;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using System.Net;
using LogicalPantry.Services.Test.UserServiceTest;
using Microsoft.EntityFrameworkCore;
using LogicalPantry.DTOs.Test.UserDtos;

namespace LogicalPantry.Tests
{
    [TestClass]
    public class UserControllerTest
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private TestApplicationDataContext _context;
        private IUserServiceTest _userServiceTest;
        private IConfiguration _configuration;

        // This method is called before each test method is run.
        [TestInitialize]
        public void Setup()
        {
            // Setup configuration to load appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing DbContext registration (use an in-memory database for testing).
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<TestApplicationDataContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }


                        var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

                        services.AddDbContext<TestApplicationDataContext>(options =>
                            options.UseSqlServer(connectionString));

                        // Register the test service.
                        services.AddTransient<IUserServiceTest, UserServicesTest>();

                        var serviceProvider = services.BuildServiceProvider();

                        // Create a scope to get the DbContext and the test service.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<TestApplicationDataContext>();
                            _userServiceTest = scopedServices.GetRequiredService<IUserServiceTest>();

                            // Ensure the in-memory database is created.
                            var db = scopedServices.GetRequiredService<TestApplicationDataContext>();
                            db.Database.EnsureCreated();
                        }
                    });
                });

            // Create an HttpClient for sending requests to the API.
            _client = _factory.CreateClient();
        }


        /// <summary>
        ///Tests the <see cref="UpdateUser_when_ValidData"/> method to ensure it correctly update user by given user data.
        /// </summary>
        /// <returns>A <see cref="UserDto"/> representing the updated user information.</returns>

        [TestMethod]
        public async Task UpdateUser_when_ValidData()
        {

            // Arrange
            var userId = 67;
            var isAllow = true;

            var userDto = new UserDto
            {
                Id = userId,
                IsAllow = isAllow,
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");


            //Act
            var response = await _client.PostAsync("/TenantB/User/UpdateUser", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response status code: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");


            //Assert
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            //check if the data saved correctly in the database

            var user = await _userServiceTest.CheckUserPostResponse(userDto);

            Assert.IsNotNull(user);
            Assert.AreEqual(userDto.Id, user.Data.Id);
            Assert.AreEqual(userDto.IsAllow, user.Data.IsAllow);

        }

        /// <summary>
        ///Tests the <see cref="UpdateUserBatch_when_ValidData"/> method to ensure it correctly update the List of the users.
        /// </summary>
        ///<returns>A <see cref="List<UserDto></UserDto>"/> representing the multiple updated user information.</returns>
        [TestMethod]
        public async Task UpdateUserBatch_when_ValidData()
        {
            //Arrage
            var userDto = new List<UserDto>
            {
                new UserDto{Id = 61, TimeSlotId = 82, Attended = true},
                new UserDto{Id = 67, TimeSlotId = 82, Attended =true},
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");

            //Act

            var response = await _client.PostAsync("/TenantB/User/UpdateUserBatch", content);

            //Assert
            Assert.IsNotNull(response);

            var users = await _userServiceTest.CheckUpdateUserBatch(userDto);
            Assert.IsNotNull(users);

            foreach (var expected in userDto)
            {
                var actual = users.Data
                    .FirstOrDefault(x => x.UserId == expected.Id && x.TimeSlotId == expected.TimeSlotId && x.Attended == expected.Attended);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.TimeSlotId, actual.TimeSlotId);
                Assert.AreEqual(expected.Attended, actual.Attended);

            }
        }

        /// <summary>
        ///Tests the <see cref="ManageUsers_GetAllRegisterUsers"/> method to ensure it correctly retrieves All the register users.
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task ManageUsers_GetAllRegisterUsers()
        {
            //Arrange
            var userCount = 3;

            var tenantId = 5;

            var session = _client.DefaultRequestHeaders;
            session.Add("TenantId", tenantId.ToString());

            //Act
            var response = await _client.GetAsync("/LP/User/ManageUsers");
            var responseContent = await response.Content.ReadAsStringAsync();

            //Assert
            Assert.IsNotNull(response);
            response.EnsureSuccessStatusCode();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

        }

        /// <summary>
        /// Tests the <see cref="GetUserIdByEmail"/> method to ensure it correctly retrieves the user ID for a given email address.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>

        [TestMethod]
        public async Task GetUserIdByEmail()
        {
            //Arrange
            var userEmail = "jaygaikwad2312@gmail.com";

            var userDto = new UserDto
            {
                Email = userEmail,
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PostAsync("/LP/User/GetUserIdByEmail", content);

            //Assert
            response.EnsureSuccessStatusCode();
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        /// <summary>
        /// Tests the <see cref="DeleteUserById"/> method to ensure it correctly delete user form the database for a given id.
        /// </summary>
        /// <returns></returns>

        [TestMethod]
        public async Task DeleteUserById()
        {
            //Arrange
            var userId = 72;
            //Act
            var response = await _client.DeleteAsync($"/TenantB/User/DeleteUser/{userId}");
            //Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            var DeleteUser = await _userServiceTest.CheckUserDeleteResponse(userId);
            Assert.IsNotNull(DeleteUser);
            Assert.IsTrue(true);
            Assert.AreEqual("User deleted Successfully", DeleteUser.Message);
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
            var addedUser = await _userServiceTest.GetUserByIdAsync(newUser.Id);
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
            var deletedUser = await _userServiceTest.GetUserByIdAsync(userId);
            Assert.IsNull(deletedUser);
        }
    }
}

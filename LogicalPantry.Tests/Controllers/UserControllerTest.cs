using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LogicalPantry.Web;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Net;
using LogicalPantry.Services.Test.UserServiceTest;
using Microsoft.EntityFrameworkCore;
using LogicalPantry.DTOs.Test.UserDtos;
using Microsoft.AspNetCore.Http;
using NuGet.Protocol;
using Microsoft.AspNetCore.Mvc;
using LogicalPantry.Models.Test.ModelTest;
using LogicalPantry.Services.Test.RegistrationService;

namespace LogicalPantry.Tests
{
    [TestClass]
    public class UserControllerTest
    {
        private WebApplicationFactory<Startup> _factory;
        private HttpClient _client;
        private TestApplicationDataContext _context;
        private IUserServiceTest _userServiceTest;
        //private IRegistrationTestService _registrationTestService;
        private IConfiguration _configuration;

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

                        //services.AddTransient<IRegistrationTestService, RegistrationTestService>();
                        // Register the test service.
                        services.AddTransient<IUserServiceTest, UserServicesTest>();

                        var serviceProvider = services.BuildServiceProvider();

                        // Create a scope to get the DbContext and the test service.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            _context = scopedServices.GetRequiredService<TestApplicationDataContext>();
                            _userServiceTest = scopedServices.GetRequiredService<IUserServiceTest>();
                            //_registrationTestService = scopedServices.GetRequiredService<IRegistrationTestService>();
                            //// Ensure the in-memory database is created.
                            //_context.Database.EnsureCreated();
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
           
            var userDto = new UserDto
            {
                Id = 372,
                IsAllow = false,
            };


            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/Logic/User/UpdateUser", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Check if the data saved correctly in the database
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
                new UserDto{Id = 372, TimeSlotId = 284, Attended = true},
                new UserDto{Id = 372, TimeSlotId = 285, Attended =true},
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");

            //Act

            var response = await _client.PostAsync("/Logic/User/UpdateUserBatch", content);

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
           

            //Act
            var response = await _client.GetAsync("/Logic/User/ManageUsers");
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
            var userEmail = "jayantgaikwad410@gmail.com";

            var userDto = new UserDto
            {
                Email = userEmail,
            };

            var content = new StringContent(JsonConvert.SerializeObject(userDto), Encoding.UTF8, "application/json");

            //Act
            var response = await _client.PostAsync("/LP/User/GetUserIdByEmail", content);
            var contentResponse = await response.Content.ReadFromJsonAsync<UserDto>();
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
            // Arrange
            var userId = 61;

            // Act
            var response = await _client.PostAsync($"/LogicalPantry/User/DeleteUser?userId={userId}", null);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine();
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
                FullName = "new user",
                Address = "abc",
                Email = "newUser@gmail.com",
                PhoneNumber = "123456454",
                IsAllow = true,
                IsRegistered = true,
                DateOfBirth = new DateTime(2000, 1, 1), // Provide a valid DateTime
                ZipCode = "12345",
                HouseholdSize = 4,
                HasSchoolAgedChildren = true,
                IsMarried = false,
                ProfilePictureUrl = "profile.jpj",
                EmploymentStatus = "Employed",
                IsVeteran = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
            // Send a POST request to add the new user.
            var response = await _client.PostAsync("Logic/User/Register", content);

            // Verify the response status code is OK (200).
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            // Retrieve the added user from the database to confirm
            var addedUser = await _userServiceTest.CheckUserPostResponse(newUser);
            Assert.IsNotNull(addedUser);

            //added data matches what was sent in the request.
            Assert.AreEqual(newUser.FullName, addedUser.Data.FullName);
            Assert.AreEqual(newUser.Email, addedUser.Data.Email);
            Assert.AreEqual(newUser.PhoneNumber, addedUser.Data.PhoneNumber);
        }

     
        //18/9/24

        //Add User profile when valid data 
        [TestMethod]
        public async Task Add_WhenValidData_Profile()
        {
            // Create the UserDto
            var user = new UserDto
            {
                FullName = "abcd",
                Address = "abcd",
                Email = "abcd@gmail.com",
                PhoneNumber = "abc",
                IsAllow = true,
                IsRegistered = true,
                DateOfBirth = new DateTime(2000, 1, 1), // Provide a valid DateTime
                ZipCode = "12345",
                HouseholdSize = 4,
                HasSchoolAgedChildren = true,
                IsMarried = false,
                ProfilePictureUrl = "profile.jpj",
                EmploymentStatus = "Employed",
                IsVeteran = false
            };

            // Create MultipartFormDataContent to send the form data
            var form = new MultipartFormDataContent();

            // Add each property of UserDto as form data with keys matching the property names
            form.Add(new StringContent(user.FullName), nameof(user.FullName));
            form.Add(new StringContent(user.Address), nameof(user.Address));
            form.Add(new StringContent(user.Email), nameof(user.Email));
            form.Add(new StringContent(user.PhoneNumber), nameof(user.PhoneNumber));
            form.Add(new StringContent(user.IsAllow.ToString()), nameof(user.IsAllow));
            form.Add(new StringContent(user.IsRegistered.ToString()), nameof(user.IsRegistered));
            form.Add(new StringContent(user.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty), nameof(user.DateOfBirth));
            form.Add(new StringContent(user.ZipCode ?? string.Empty), nameof(user.ZipCode));
            form.Add(new StringContent(user.HouseholdSize?.ToString() ?? string.Empty), nameof(user.HouseholdSize));
            form.Add(new StringContent(user.HasSchoolAgedChildren?.ToString() ?? string.Empty), nameof(user.HasSchoolAgedChildren));
            form.Add(new StringContent(user.IsMarried?.ToString() ?? string.Empty), nameof(user.IsMarried));
            form.Add(new StringContent(user.ProfilePictureUrl), nameof(user.ProfilePictureUrl));
            form.Add(new StringContent(user.EmploymentStatus ?? string.Empty), nameof(user.EmploymentStatus));
            form.Add(new StringContent(user.IsVeteran.ToString()), nameof(user.IsVeteran));

            // Create a mock IFormFile
            var fileName = "test-logo.png";
            var contentType = "image/png";
            var fileContent = new byte[] { /* file content */ }; // Example file content as byte array
            var fileStream = new MemoryStream(fileContent);
            var ProfilePicture = new FormFile(fileStream, 0, fileContent.Length, "ProfilePicture", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            // Add the IFormFile to the form with the correct key name
            form.Add(new StreamContent(ProfilePicture.OpenReadStream()), "ProfilePicture", ProfilePicture.FileName);

            // Send the POST request to the correct URL
            var response = await _client.PostAsync("/Logic/User/Profile", form);
            var responseContent = await response.Content.ReadAsStringAsync();

            var userProfile = await _userServiceTest.ProfileAsync(user.Email);
            Assert.AreEqual(user.FullName , userProfile.Data.FullName);
            Assert.AreEqual(user.Address, userProfile.Data.Address);
            Assert.AreEqual(user.Email, userProfile.Data.Email);
            Assert.AreEqual(user.PhoneNumber, userProfile.Data.PhoneNumber);
        }

    }
}

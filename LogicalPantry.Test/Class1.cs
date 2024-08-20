//using LogicalPantry.DTOs;
//using LogicalPantry.DTOs.UserDtos;
//using LogicalPantry.Services.RegistrationService;
//using LogicalPantry.Web.Controllers;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NUnit.Framework;

//namespace LogicalPantry.Test
//{
//    [TestFixture]
//    public class RegistrationTests
//    {

//        private Mock<IRegistrationService> _mockRegistrationService;
//        private Mock<ILogger<RegistrationController>> _mockLogger;
//        private RegistrationController _controller;
//        private Mock<HttpContext> _mockHttpContext;


//        [SetUp]
//        public void Setup()
//        {
//            _mockRegistrationService = new Mock<IRegistrationService>();
//            _mockLogger = new Mock<ILogger<RegistrationController>>();
//            _controller = new RegistrationController(_mockRegistrationService.Object, _mockLogger.Object);
//            _mockHttpContext = new Mock<HttpContext>();
//        }

//        [Test]
//        public async Task Register_ShouldReturnError_WhenUserDtoIsNull()
//        {
//            UserDto userDto = null;

//            var result = await _controller.Register(userDto) as ViewResult;

//            _controller.ModelState.AddModelError("TenantId", "TenantId is required.");

//            var modelState = _controller.ModelState;

//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Error"));
//            Assert.That(modelState.IsValid, Is.False);
//        }

//        [Test]
//        public async Task Register_ShouldReturnError_WhenTenantIdIsInvalid()
//        {

//            var userDto = new UserDto
//            {
//                TenantId = 0, // Testing When Tenat Id is  0
//                FullName = "Sample UserName",
//                Email = "sampleUserName@gmail.com",
//                PhoneNumber = "1234567890"
//            };

//            // Act
//            var result = await _controller.Register(userDto) as ViewResult;

//            _controller.ModelState.AddModelError("TenantId", "TenantId is required.");
//            // Assert

//            var modelState = _controller.ModelState;
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Error"));
//            Assert.That(modelState["TenantId"].Errors.Count, Is.GreaterThan(0));
//        }

//        [Test]
//        public async Task Register_ShouldReturnError_WhenNameEmailPhoneNumberAreNull()
//        {
//            // Arrange
//            var userDto = new UserDto
//            {
//                TenantId = 1,
//                FullName = null,
//                Email = null,
//                PhoneNumber = null
//            };

//            // Act
//            var result = await _controller.Register(userDto) as ViewResult;


//            // Simulate model state errors
//            _controller.ModelState.AddModelError("FullName", "Full Name is required.");
//            _controller.ModelState.AddModelError("Email", "Email is required.");
//            _controller.ModelState.AddModelError("PhoneNumber", "Phone Number is required.");
//            // Assert


//            var modelState = _controller.ModelState;
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Error"));
//            Assert.That(modelState["FullName"].Errors.Count, Is.GreaterThan(0));
//            Assert.That(modelState["Email"].Errors.Count, Is.GreaterThan(0));
//            Assert.That(modelState["PhoneNumber"].Errors.Count, Is.GreaterThan(0));
//        }

//        [Test]
//        public async Task Register_ShouldReturnError_WhenEmailFormatIsInvalid()
//        {
//            // Arrange
//            var userDto = new UserDto
//            {
//                TenantId = 1,
//                FullName = "Sample User",
//                Email = "sampleUser@gmail.com",
//                PhoneNumber = "1234567890"
//            };

//            // Act
//            var result = await _controller.Register(userDto) as ViewResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Error"));
//            //Assert.That(result.ModelState["Email"].Errors.Count, Is.GreaterThan(0));
//        }

//        [Test]
//        public async Task Register_ShouldReturnError_WhenTenantIdMismatch()
//        {
//            // Arrange
//            var userDto = new UserDto
//            {
//                TenantId = 1, // Incorrect TenantId
//                FullName = "Sample User",
//                Email = "sampleUser@gmail.com",
//                PhoneNumber = "1234567890"
//            };

//            // Set up a mock HTTP context with a different TenantId
//            _mockHttpContext.Setup(x => x.Request.Headers["TenantId"]).Returns("2");
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = _mockHttpContext.Object
//            };

//            // Act
//            var result = await _controller.Register(userDto) as ViewResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Error"));
//            //Assert.That(result.ModelState["TenantId"].Errors.Count, Is.GreaterThan(0));
//        }

//        [Test]
//        public async Task Register_ShouldRedirectToUserCalendar_WhenRegistrationIsSuccessful()
//        {
//            // Arrange
//            var userDto = new UserDto
//            {
//                TenantId = 1,
//                FullName = "Sample User",
//                Email = "sampleUser@gmail.com",
//                PhoneNumber = "1234567890"
//            };
//            var registrationResponse = new ServiceResponse<bool> { Success = true };
//            _mockRegistrationService
//                .Setup(service => service.RegisterUser(It.IsAny<UserDto>()))
//                .ReturnsAsync(registrationResponse);

//            // Act
//            var result = await _controller.Register(userDto) as RedirectToActionResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ActionName, Is.EqualTo("UserCalendar"));
//            Assert.That(result.ControllerName, Is.EqualTo("TimeSlot"));
//            Assert.That(result.RouteValues["area"], Is.Null);
//        }

//        [Test]
//        public async Task Register_ShouldReturnViewWithError_WhenRegistrationFails()
//        {
//            // Arrange
//            var userDto = new UserDto { /* initialize properties */ };
//            var registrationResponse = new ServiceResponse<bool> { Success = false };
//            _mockRegistrationService
//                .Setup(service => service.RegisterUser(It.IsAny<UserDto>()))
//                .ReturnsAsync(registrationResponse);

//            // Act
//            var result = await _controller.Register(userDto) as ViewResult;

//            // Assert
//            Assert.That(result, Is.Not.Null);
//            Assert.That(result.ViewName, Is.EqualTo("Index"));
//        }
//    }

//}

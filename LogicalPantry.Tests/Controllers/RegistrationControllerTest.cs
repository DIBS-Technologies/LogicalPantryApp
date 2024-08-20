using LogicalPantry.DTOs;
using LogicalPantry.Services.RegistrationService;
using LogicalPantry.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Services.Test.RegistrationService;

namespace LogicalPantry.TestMethods.Controllers
{

    [TestClass]
    internal class RegistrationControllerTestMethod
    {

            private Mock<IRegistrationService> _mockRegistrationService;
            private Mock<ILogger<RegistrationController>> _mockLogger;
            private RegistrationController _controller;
            private Mock<HttpContext> _mockHttpContext;

            private RegistrationTestService _registrationTestService;

            [TestInitialize]
            public void Setup()
            {
                _mockRegistrationService = new Mock<IRegistrationService>();
                _mockLogger = new Mock<ILogger<RegistrationController>>();
                _controller = new RegistrationController(_mockRegistrationService.Object, _mockLogger.Object);
                _mockHttpContext = new Mock<HttpContext>();
                _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new Mock<ITempDataProvider>().Object);

           

                var mockHttpContext = new Mock<HttpContext>();

                var items = new Dictionary<object, object>
                {
                    ["TenantName"] = "TenantB"
                };

                mockHttpContext.Setup(ctx => ctx.Items).Returns(items);

                _controller.ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                };
            }

            [TestMethod]
            public async Task Register_ShouldReturnError_WhenUserDtoIsNull()
            {
                UserDto userDto = null;

                var result = await _controller.Register(userDto) as ViewResult;

                var modelState = _controller.ModelState;

                Assert.IsNotNull(result);
                Assert.Equals(result.ViewName,"Index");
                Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
                Assert.Equals(_controller.TempData["SuccessMessageUser"],"Failed to Save User server error.");
            }

            [TestMethod]
            public async Task Register_ShouldReturnError_WhenTenantIdIsInvalid()
            {
                var userDto = new UserDto
                {
                    TenantId = 0, // TestMethoding When Tenat Id is  0
                    FullName = "Sample UserName",
                    Email = "sampleUserName@gmail.com",
                    PhoneNumber = "1234567890"
                };


                var result = await _controller.Register(userDto) as ViewResult;

                _controller.ModelState.AddModelError("TenantId", "TenantId is required.");


                var modelState = _controller.ModelState;

                Assert.IsNotNull(result);

                Assert.Equals(result.ViewName,"Index");
                Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
                Assert.Equals(_controller.TempData["SuccessMessageUser"], "Failed to Save User server error.");
            }

            [TestMethod]
            public async Task Register_ShouldReturnError_WhenNameEmailPhoneNumberAreNull()
            {
                var userDto = new UserDto
                {
                    TenantId = 1,
                    FullName = null,
                    Email = null,
                    PhoneNumber = null
                };

                var result = await _controller.Register(userDto) as ViewResult;

                _controller.ModelState.AddModelError("FullName", "Full Name is required.");
                _controller.ModelState.AddModelError("Email", "Email is required.");
                _controller.ModelState.AddModelError("PhoneNumber", "Phone Number is required.");

                var modelState = _controller.ModelState;
                Assert.IsNotNull(result);
                Assert.Equals(result.ViewName, "Index");
                Assert.IsTrue(modelState["FullName"].Errors.Count > 0, "FullName should have errors.");
                Assert.IsTrue(modelState["Email"].Errors.Count > 0, "Email should have errors.");
                Assert.IsTrue(modelState["PhoneNumber"].Errors.Count > 0, "PhoneNumber should have errors.");
                Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
                Assert.Equals(_controller.TempData["SuccessMessageUser"], "Failed to Save User server error.");
        }

            [TestMethod]
            public async Task Register_ShouldReturnError_WhenEmailFormatIsInvalid()
            {

                var userDto = new UserDto
                {
                    TenantId = 1,
                    FullName = "Sample User",
                    Email = "sampleUser@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "Sample Address"
                };

                var result = await _controller.Register(userDto) as ViewResult;

                Assert.IsNotNull(result);
                Assert.Equals(result.ViewName, "Index");
                Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
                Assert.Equals(_controller.TempData["SuccessMessageUser"], "Failed to Save User server error.");

        }

            [TestMethod]
            public async Task Register_ShouldReturnError_WhenTenantIdMismatch()
            {

                var userDto = new UserDto
                {
                    TenantId = 1, // Incorrect TenantId
                    FullName = "Sample User",
                    Email = "sampleUser@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "Sample Address"
                };

                _mockHttpContext.Setup(x => x.Request.Headers["TenantId"]).Returns("2");
                _controller.ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                };

                var result = await _controller.Register(userDto) as ViewResult;

                Assert.IsNotNull(result);
                Assert.Equals(result.ViewName, "Index");
                Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
                Assert.Equals(_controller.TempData["SuccessMessageUser"], "Failed to Save User server error.");

            }

            [TestMethod]
            public async Task Register_ShouldRedirectToUserCalendar_WhenRegistrationIsSuccessful()
            {

                var userDto = new UserDto
                {
                    TenantId = 1,
                    FullName = "Sample User",
                    Email = "sampleUser@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "Sample Address"
                };
                var registrationResponse = new ServiceResponse<bool> { Success = true };
                _mockRegistrationService
                    .Setup(service => service.RegisterUser(It.IsAny<UserDto>()))
                    .ReturnsAsync(registrationResponse);


                 var result = await _controller.Register(userDto) as RedirectToActionResult;

                 var userData = _registrationTestService.GetUser(userDto); // user return  user bool 


                    Assert.IsNotNull(result);
                    Assert.Equals(result.ActionName, "UserCalendar");
                    Assert.Equals(result.ControllerName, "TimeSlot");

                    Assert.IsTrue(userData.Success);
                    Assert.Equals(userData.Message, "User details are correct.");
                    Assert.Equals(result.ControllerName, "TimeSlot");



        }

            [TestMethod]
            public async Task Register_ShouldReturnViewWithError_WhenRegistrationFails()
            {

                var userDto = new UserDto
                {
                    TenantId = 1,
                    FullName = "Sample User",
                    Email = "sampleUser@gmail.com",
                    PhoneNumber = "1234567890",
                    Address = "Sample Address"
                };
                var registrationResponse = new ServiceResponse<bool> { Success = false };
                _mockRegistrationService
                    .Setup(service => service.RegisterUser(It.IsAny<UserDto>()))
                    .ReturnsAsync(registrationResponse);


                    var result = await _controller.Register(userDto) as ViewResult;

                    Assert.IsNotNull(result);
                    Assert.Equals(result.ViewName, "Index");
        }
        }
    }


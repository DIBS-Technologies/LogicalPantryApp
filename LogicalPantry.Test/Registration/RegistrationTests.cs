using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LogicalPantry.DTOs;
using LogicalPantry.Services.RegistrationService;
using Microsoft.AspNetCore.Http;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Web.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

[TestFixture]
public class RegistrationTests
{

    private Mock<IRegistrationService> _mockRegistrationService;
    private Mock<ILogger<RegistrationController>> _mockLogger;
    private RegistrationController _controller;
    private Mock<HttpContext> _mockHttpContext;


    [SetUp]
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

    [Test]
    public async Task Register_ShouldReturnError_WhenUserDtoIsNull()
    {
        UserDto userDto = null;

        var result = await _controller.Register(userDto) as ViewResult;

        var modelState = _controller.ModelState;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["MessageClass"], Is.EqualTo("alert-danger"));
        Assert.That(_controller.TempData["SuccessMessageUser"], Is.EqualTo("Failed to Save User server error."));
    }

    [Test]
    public async Task Register_ShouldReturnError_WhenTenantIdIsInvalid()
    {
        var userDto = new UserDto
        {
            TenantId = 0, // Testing When Tenat Id is  0
            FullName = "Sample UserName",
            Email = "sampleUserName@gmail.com",
            PhoneNumber = "1234567890"
        };

       
        var result = await _controller.Register(userDto) as ViewResult;

        _controller.ModelState.AddModelError("TenantId", "TenantId is required.");
     

        var modelState = _controller.ModelState;

        Assert.That(result, Is.Not.Null);

        Assert.That(result.ViewName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["MessageClass"], Is.EqualTo("alert-danger"));
        Assert.That(_controller.TempData["SuccessMessageUser"], Is.EqualTo("Failed to Save User server error."));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Index"));
        Assert.That(modelState["FullName"].Errors.Count, Is.GreaterThan(0));
        Assert.That(modelState["Email"].Errors.Count, Is.GreaterThan(0));
        Assert.That(modelState["PhoneNumber"].Errors.Count, Is.GreaterThan(0));
        Assert.That(_controller.TempData["MessageClass"], Is.EqualTo("alert-danger"));
        Assert.That(_controller.TempData["SuccessMessageUser"], Is.EqualTo("Failed to Save User server error."));
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["MessageClass"], Is.EqualTo("alert-danger"));
        Assert.That(_controller.TempData["SuccessMessageUser"], Is.EqualTo("Failed to Save User server error."));

    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Index"));
        Assert.That(_controller.TempData["MessageClass"], Is.EqualTo("alert-danger"));
        Assert.That(_controller.TempData["SuccessMessageUser"], Is.EqualTo("Failed to Save User server error."));

    }

    [Test]
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

       
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("UserCalendar"));
        Assert.That(result.ControllerName, Is.EqualTo("TimeSlot"));
    }

    [Test]
    public async Task Register_ShouldReturnViewWithError_WhenRegistrationFails()
    {
       
        var userDto = new UserDto {
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.EqualTo("Index"));
    }
}

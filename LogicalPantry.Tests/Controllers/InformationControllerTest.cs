using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using LogicalPantry.DTOs;
using Microsoft.AspNetCore.Routing;

namespace LogicalPantry.Tests
{
    [TestClass]
    public class InformationControllerTests
    {
        private Mock<IInformationService> _mockService;
        private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private Mock<ILogger<InformationController>> _mockLogger;
        private Mock<IMemoryCache> _mockMemoryCache;
        private InformationController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockService = new Mock<IInformationService>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<InformationController>>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            // Initialize the controller with mocked dependencies
            _controller = new InformationController(
                _mockService.Object,
                _mockWebHostEnvironment.Object,
                _mockLogger.Object,
                _mockMemoryCache.Object
            );
        }

        [TestMethod]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Get_ValidTenantId_ReturnsOkResult()
        {
            // Arrange
            var tenantId = 1;
            var tenantDto = new TenantDto
            {
                Id = 1,
                PaypalId = "Paypal123",
                PageName = "Sample Page",
                Logo = "logo.png",
                Timezone = "GMT"
            };

            _mockService
                .Setup(x => x.GetTenant(It.IsAny<int>()))
                .ReturnsAsync(new ServiceResponse<TenantDto>
                {
                    Success = true,
                    Data = tenantDto,
                    Message = "Tenant retrieved successfully."
                });

            // Act
            var result = await _controller.Get(tenantId) as JsonResult;

            // Assert
            Assert.IsNotNull(result);
            var response = result.Value as TenantDto;
            Assert.IsNotNull(response);
            Assert.AreEqual(tenantDto.Id, response.Id);
            Assert.AreEqual(tenantDto.PaypalId, response.PaypalId);
        }

        [TestMethod]
        public async Task Get_InvalidTenantId_ReturnsNotFound()
        {
            // Arrange
            var tenantId = 1;

            // Act
            var result = await _controller.Get(tenantId) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);


            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task Get_ServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            var tenantId = 1;
            _mockService
                .Setup(x => x.GetTenant(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Get(tenantId) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }


        [TestMethod]
        public async Task AddTenant_WithNullTenantName_ShouldSetTempDataError()
        {

            var tenantDto = new TenantDto { TenantName = null, Logo = null, PaypalId = null, Timezone = null };
            var fileMock = new Mock<IFormFile>();

            // Act
            await _controller.AddTenant(tenantDto, fileMock.Object);

            // Assert
            Assert.Equals(_controller.TempData["MessageClass"], "alert-danger");
            Assert.Equals(_controller.TempData["ErrorMessageInfo"], "Internal server error.");
            var modelStateErrors = _controller.ModelState.Values.SelectMany(v => v.Errors).ToList();
            Assert.IsNotNull(modelStateErrors);

        }

        [TestMethod]
        public async Task AddTenant_WithValidData_ShouldSetTempDataSuccess()
        {

            var tenantDto = new TenantDto
            {
                Id = 4,
                TenantName = "TenantB",
                Logo = "/Image/logo.png",
                PaypalId = "swapnil@gmail.com",
                Timezone = "Asia/India",
                PageName = "account"
            };
            var fileMock = new Mock<IFormFile>();
            _mockService.Setup(s => s.PostTenant(It.IsAny<TenantDto>()))
                        .ReturnsAsync(new ServiceResponse<bool> { Success = true });


            await _controller.AddTenant(tenantDto, fileMock.Object);


            Assert.Equals(_controller.TempData["MessageClass"], "alert-success");
            Assert.Equals(_controller.TempData["SuccessMessageInfo"], "Infromation Saved Successfully");
        }

        [TestMethod]
        public async Task GetTenantIdByName_WithExistingTenant_ShouldReturnOk()
        {

            var tenantName = "TenantA";
            _mockService.Setup(s => s.GetTenantByNameAsync(tenantName))
                        .ReturnsAsync(new ServiceResponse<TenantDto>
                        {
                            Success = true,
                            Data = new TenantDto { TenantName = tenantName }
                        });


            var result = await _controller.GetTenantIdByName(tenantName) as OkObjectResult;


            Assert.IsNotNull(result);
            Assert.Equals(result.StatusCode, 200);
        }

        [TestMethod]
        public async Task GetTenantIdByName_WithNonExistingTenant_ShouldReturnNotFound()
        {

            var tenantName = "TenantB";
            _mockService.Setup(s => s.GetTenantByNameAsync(tenantName))
                        .ReturnsAsync(new ServiceResponse<TenantDto>
                        {
                            Success = false,
                            Message = "Tenant not found."
                        });

            // Act
            var result = await _controller.GetTenantIdByName(tenantName) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.Equals(result.StatusCode, 404);
            Assert.Equals(result.Value, "Tenant not found.");
        }

        [TestMethod]
        public async Task GetTenantIdByEmail_WithValidEmail_ShouldReturnOk()
        {

            var email = "swappnilfromdibs@gmail.com";
            _mockService.Setup(s => s.GetTenantIdByEmail(email))
                        .ReturnsAsync(new ServiceResponse<TenantDto>
                        {
                            Success = true
                        });

            // Act
            var result = await _controller.GetTenantIdByEmail(email) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.Equals(result.StatusCode, 200);
        }

        [TestMethod]
        public async Task GetTenantIdByEmail_WithInvalidEmail_ShouldReturnNotFound()
        {

            var email = "invalid@example.com";
            _mockService.Setup(s => s.GetTenantIdByEmail(email))
                        .ReturnsAsync(new ServiceResponse<TenantDto>
                        {
                            Success = false

                        });

            // Act
            var result = await _controller.GetTenantIdByEmail(email) as NotFoundObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.Equals(result.StatusCode, 404);
            Assert.Equals(result.Value, "Tenant not found.");
        }

        [TestMethod]
        public async Task Home_WithExistingPage_ShouldReturnView()
        {

            var pageName = "account-billing";

            var tenantName = _controller.HttpContext.Items["TenantName"].ToString();

            var tenantResponse = new ServiceResponse<TenantDto>
            {
                Success = true,
                Data = new TenantDto { PageName = "TenantA" }
            };
            _mockService.Setup(s => s.GetTenantPageNameForUserAsync(tenantName))
                        .ReturnsAsync(tenantResponse);


            var result = await _controller.Home() as ViewResult;

            Assert.IsNotNull(result);
            Assert.Equals(_controller.TempData["PageName"], pageName + ".html");


        }


        [TestMethod]
        public async Task Home_WithNonExistingPage_ShouldReturnNotFound()
        {

            var tenantName = _controller.HttpContext.Items["TenantName"].ToString();

            _mockService.Setup(s => s.GetTenantByNameAsync(tenantName))
                        .ReturnsAsync(new ServiceResponse<TenantDto>
                        {
                            Success = false,
                            Message = "Page not found."
                        });

            var result = await _controller.Home() as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.Equals(result.StatusCode, 404);
            Assert.Equals(result.Value, "Page not found.");

        }
    }

}


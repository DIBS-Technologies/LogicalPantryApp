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

namespace LogicalPantry.Tests
{
    [TestClass]
    public class InformationControllerTests
    {
        private Mock<IInformationService> _mockInformationService;
        private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private Mock<ILogger<InformationController>> _mockLogger;
        private Mock<IMemoryCache> _mockMemoryCache;
        private InformationController _controller;

        [TestInitialize]
        public void Setup()
        {
            // Initialize mocks
            _mockInformationService = new Mock<IInformationService>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockLogger = new Mock<ILogger<InformationController>>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            // Initialize the controller with mocked dependencies
            _controller = new InformationController(
                _mockInformationService.Object,
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
            var tenantId = "1";
            var tenantDto = new TenantDto
            {
                Id = 1,
                PaypalId = "Paypal123",
                PageName = "Sample Page",
                Logo = "logo.png",
                Timezone = "GMT"
            };

            _mockInformationService
                .Setup(x => x.GetTenant(It.IsAny<int>()))
                .ReturnsAsync(new ServiceResponse<TenantDto>
                {
                    Success = true,
                    Data = tenantDto,
                    Message = "Tenant retrieved successfully."
                });

            // Act
            var result = await _controller.Get(tenantId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var response = result.Value as TenantDto;
            Assert.IsNotNull(response);
            Assert.AreEqual(tenantDto.Id, response.Id);
            Assert.AreEqual(tenantDto.PaypalId, response.PaypalId);
        }

        [TestMethod]
        public async Task Get_InvalidTenantId_ReturnsNotFound()
        {
            // Arrange
            var tenantId = "invalid";

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
            var tenantId = "1";
            _mockInformationService
                .Setup(x => x.GetTenant(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Get(tenantId) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

    }
}

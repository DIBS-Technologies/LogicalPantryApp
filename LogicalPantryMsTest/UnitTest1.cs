using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LogicalPantry.Web.Controllers;
using LogicalPantry.Services.InformationService;
using LogicalPantry.Models.Models;

[TestClass]
public class InformationControllerTests
{
    private Mock<ILogger<InformationController>> _mockLogger;
    private Mock<IInformationService> _mockInformationService;
    private InformationController _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<InformationController>>();
        _mockInformationService = new Mock<IInformationService>();
        _controller = new InformationController(_mockLogger.Object, _mockInformationService.Object);
    }

    [TestMethod]
    public void Get_TenantIdIsZero_ReturnsNull()
    {
        // Arrange
        string tenantId = "0";

        // Act
        var result = _controller.Get(tenantId);

        var expectedResult = 0;
        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Get_TenantIdIsValid_ReturnsTenant()
    {
        // Arrange
        string tenantId = "1";
        var expectedTenant = new Tenant(); // Replace with actual tenant object
        await _mockInformationService.Setup(service => service.GetTenant(It.IsAny<int>()))
                               .ReturnsAsync(expectedTenant);

        // Act
        var result = _controller.Get(tenantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedTenant, result);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public void Get_TenantIdIsInvalid_ThrowsFormatException()
    {
        // Arrange
        string tenantId = "invalid";

        // Act
        _controller.Get(tenantId);

        // Assert is handled by ExpectedException attribute
    }
}

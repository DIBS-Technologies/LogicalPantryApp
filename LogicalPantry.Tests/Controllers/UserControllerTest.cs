using Castle.Core.Logging;
using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using LogicalPantry.Services.UserServices;
using LogicalPantry.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using Tweetinvi.Core.DTO;
using Tweetinvi.Core.Models;
using Tweetinvi.Models.V2;

namespace LogicalPantry.Tests.Controllers
{
    [TestClass]
    public class UserControllerTest
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<UserController>> _mockLogger;
        private UserController _userController;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserController>>();

            _userController = new UserController(_mockUserService.Object, _mockLogger.Object);
        }

        [TestMethod]
        public void Index_Return_View()
        {
            //Act
            var result = _userController.Index();

            //Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        //Test Cases for ManageUsers method

        [TestMethod]
        public async Task ManageUsers_Success_ReturnsViewWithUsers()
        {
            // Arrange
                var userDtos = new List<UserDto>
                     {
                         new UserDto 
                         {   Id = 1, 
                             FullName = "Jayant Gaikwad", 
                             Email = "jayantgaikwad410@gmail.com", 
                             PhoneNumber = "9511796199", 
                             IsAllow = true, 
                             IsRegistered = true 
                         }
                     };

            var serviceResponse = new ServiceResponse<IEnumerable<UserDto>>
            {
                Success = true,
                Data = userDtos,
              
            };

            _mockUserService.Setup(service => service.GetAllRegisteredUsersAsync())
                .ReturnsAsync(serviceResponse);

            // Act
            var result =  await _userController.ManageUsers() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as IEnumerable<UserDto>;
            Assert.IsNotNull(model);
            Assert.AreEqual(userDtos.Count, model.Count());
        }

        [TestMethod]
        public async Task ManageUsers_ServiceFailure_ReturnsViewWithEmptyModel()
        {
            // Arrange
            var serviceResponse = new ServiceResponse<IEnumerable<UserDto>>
            {
                Success = false,
                Data = Enumerable.Empty<UserDto>(),
                Message = "Error retrieving users."
            };
            _mockUserService.Setup(service => service.GetAllRegisteredUsersAsync())
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _userController.ManageUsers() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as IEnumerable<UserDto>;
            Assert.IsNotNull(model);
            Assert.IsFalse(model.Any());
        }

        //Test Cases for GetUserbyId Method
        [TestMethod]
        public async Task GetUserbyId_Success_ReturnsExpectedData()
        {
            // Arrange
            var tenantId = 1;
            var userDto = new UserDto
            {
                Id = 1,
                FullName = "Jayant Gaikwad",
                Email = "jayantgaikwad410@gmail.com",
                PhoneNumber = "9511796199",
                Address = "pune",
                IsAllow = true,
                IsRegistered = false,
            };

            var serviceResponse = new ServiceResponse<UserDto>
            {
                Success = true,
                Data = userDto,
            };


            _mockUserService.Setup(service => service.GetUserByIdAsync(tenantId))
                .ReturnsAsync(serviceResponse);

            // Act
            var result =  _userController.GetUserbyId(tenantId) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var resultData = result.Value as UserDto;
            Assert.IsNotNull(resultData);
            Assert.AreEqual(userDto.Id, resultData.Id);
            Assert.AreEqual(userDto.FullName, resultData.FullName);
            Assert.AreEqual(userDto.Email, resultData.Email);
            Assert.AreEqual(userDto.PhoneNumber, resultData.PhoneNumber);
            Assert.AreEqual(userDto.IsAllow, resultData.IsAllow);
        }

        [TestMethod]
        public async Task GetUserbyId_InvalidTenantId_ReturnsNull()
        {
            // Act
            var result =  _userController.GetUserbyId(0);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserbyId_ServiceFailure_ReturnsError()
        {
            // Arrange
            var tenantId = 0;
            _mockUserService.Setup(service => service.GetUserByIdAsync(tenantId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result =  _userController.GetUserbyId(tenantId);

            // Assert
            Assert.IsNull(result); // Since you are returning null in the controller on service failure
        }

        //Test cases for GetUserIdByEmail method

        [TestMethod]
        public async Task GetUserIdByEmail_Success_ReturnsUserId()
        {
            // Arrange
            var email = "jayantgaikwad410@gmail.com";
            var userId = 1;
            var serviceResponse = new ServiceResponse<int>
            {
                Success = true,
                Data = userId,
            };

            _mockUserService.Setup(service => service.GetUserIdByEmail(email))
                .ReturnsAsync(serviceResponse);

            var dto = new UserDto { Email = email };

            // Act
            var result = await _userController.GetUserIdByEmail(dto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task GetUserIdByEmail_InvalidEmail_ReturnsBadRequest()
        {

          
            // Act
            var result = await _userController.GetUserIdByEmail(null) as BadRequestObjectResult;

          

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            var resultData = result.Value as dynamic;
           // Assert.AreEqual("Invalid email.", resultData.Message);
        }

        [TestMethod]
        public async Task GetUserIdByEmail_ServiceFailure_ReturnsStatusCode500()
        {
            // Arrange
            var email = "john.doe@example.com";
            var serviceResponse = new ServiceResponse<int>
            {
                Success = false,
                Data = 0,
                Message = "Error retrieving user ID."
            };

            _mockUserService.Setup(service => service.GetUserIdByEmail(email))
                .ReturnsAsync(serviceResponse);

            var dto = new UserDto { Email = email };

            // Act
            var result = await _userController.GetUserIdByEmail(dto) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
            var resultData = result.Value as ServiceResponse<int>;
            Assert.IsFalse(resultData.Success);
            Assert.AreEqual("Error retrieving user ID.", resultData.Message);
        }


        // Test Cases For Delete User by Id

        [TestMethod]
        public async Task DeleteUser_Success_ReturnsNoContent()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(service => service.DeleteUserAsync(userId))
                .ReturnsAsync(new ServiceResponse<bool> { Success = true });

            // Act
            var result = await _userController.DeleteUser(userId.ToString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task DeleteUser_InvalidIdFormat_ReturnsBadRequest()
        {
            // Act
            var result = await _userController.DeleteUser("invalid");

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DeleteUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(service => service.DeleteUserAsync(userId))
                .ReturnsAsync(new ServiceResponse<bool> { Success = false, Message = "User not found." });

            // Act
            var result = await _userController.DeleteUser(userId.ToString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task DeleteUser_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var userId = 1;
            _mockUserService.Setup(service => service.DeleteUserAsync(userId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _userController.DeleteUser(userId.ToString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }




        // Write test cases for GetUesrByTimeSlot method
        [TestMethod]
        public async Task GetUsersbyTimeSlot_Success_ReturnsExpectedData()
        {
            // Arrange

            DateTime timeSlot = DateTime.Parse("2024-08-05 10:34:09.0000000");
            var tenantId = 1;
            var userDtos = new List<UserDto>
                {
                    new UserDto 
                    { 
                        Id = 1, 
                        FullName = "Jayant Gaikwad", 
                        Email = "jayantgaikwad410@gmail.com", 
                        PhoneNumber = "9511796199", 
                        IsAllow = true 
                    }
                 };

            _mockUserService.Setup(service => service.GetUsersbyTimeSlot(timeSlot, tenantId))
                .ReturnsAsync(new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Data = userDtos,
                    Message = "Tenant retrieved successfully."
                });

            // Act
            var result =  _userController.GetUsersbyTimeSlot(timeSlot, tenantId) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var resultData = result.Value as IEnumerable<UserDto>;
            Assert.AreEqual(userDtos.Count, resultData.Count());
        }

        [TestMethod]
        public async Task GetUsersbyTimeSlot_InvalidParameters_ReturnsNull()
        {
            // Act
            var result =  _userController.GetUsersbyTimeSlot(DateTime.MinValue, 0);

            // Assert
            Assert.IsNull(result);
        }

      

        //Write test case for PutUserStatus method

        [TestMethod]
        public async Task PutUserStatus_Success_ReturnsExpectedResponse()
        {
            // Arrange
            var userDtos = new List<UserAttendedDto>
            {
                new UserAttendedDto { Id = 1, IsAttended = true },
           
            };

            var serviceResponse = new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "User allow status updated successfully."
            };

            _mockUserService.Setup(service => service.UpdateUserAllowStatusAsync(userDtos))
                .ReturnsAsync(serviceResponse);

            // Act
            var result =  _userController.PutUserStatus(userDtos) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var resultData = result.Value as ServiceResponse<bool>;
            Assert.IsNotNull(resultData);
            Assert.IsTrue(resultData.Success);
            Assert.AreEqual("User allow status updated successfully.", resultData.Message);
        }

        [TestMethod]
        public async Task PutUserStatus_NullInput_ReturnsNull()
        {
            // Act
            var result =  _userController.PutUserStatus(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task PutUserStatus_EmptyList_ReturnsNull()
        {
            // Act
            var result =  _userController.PutUserStatus(new List<UserAttendedDto>());

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task PutUserStatus_ServiceFailure_ReturnsServiceError()
        {
            // Arrange
            var userDtos = new List<UserAttendedDto>
        {
            new UserAttendedDto { Id = 1, IsAttended = true }
        };

            var serviceResponse = new ServiceResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Error updating user allow status."
            };

            _mockUserService.Setup(service => service.UpdateUserAllowStatusAsync(userDtos))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = _userController.PutUserStatus(userDtos) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
            var resultData = result.Value as ServiceResponse<bool>;
            Assert.IsNotNull(resultData);
            Assert.IsFalse(resultData.Success);

        }

        //Write test case for UpdateUser
        [TestMethod]
        public async Task UpdateUser_ValidData_ReturnsOkResult()
        {
            // Arrange
            var userDto = new UserDto { Id = 1, IsAllow = true };

            //convert userDto Object to Json string
            var userDtoJson = JsonConvert.SerializeObject(new UserAllowStatusDto { Id = userDto.Id, IsAllow = userDto.IsAllow });

            // Setup the mock service to return a successful response
            _mockUserService
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserDto>()))
                .ReturnsAsync(new ServiceResponse<UserDto> { Success = true });

            // Act
            var result = await _userController.UpdateUser(userDtoJson) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var response = result.Value as dynamic;
            Assert.IsTrue(response.success);
        }


        [TestMethod]
        public async Task UpdateUser_InternalServerError_ReturnsStatusCode500()
        {
            // Arrange
            var userDto = new UserDto { Id = 1, IsAllow = true };
            var userDtoJson = JsonConvert.SerializeObject(new UserAllowStatusDto { Id = userDto.Id, IsAllow = userDto.IsAllow });

            // Setup the mock service to return a failed response
            _mockUserService
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserDto>()))
                .ReturnsAsync(new ServiceResponse<UserDto> { Success = false, Message = "Internal server error." });

            // Act
            var result = await _userController.UpdateUser(userDtoJson) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }

        //Test Cases for UpdateUserBatch

        [TestMethod]
        public async Task UpdateUserBatch_ValidData_ReturnsOkResult()
        {
            // Arrange
            var userStatuses = new List<UserAttendedDto>
             {
                new UserAttendedDto { Id = 1, IsAttended = true },
             };

            _mockUserService
                .Setup(x => x.UpdateUserAllowStatusAsync(It.IsAny<List<UserAttendedDto>>()))
                .ReturnsAsync(new ServiceResponse<bool> { Success = true });

            // Act
            var result = await _userController.UpdateUserBatch(userStatuses) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var response = result.Value as dynamic;
            Assert.IsTrue(response.success);
        }

        [TestMethod]
        public async Task UpdateUserBatch_InvalidData_ReturnsBadRequest()
        {
            // Act
            var result = await _userController.UpdateUserBatch(null) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("Invalid data.", result.Value);
        }

        [TestMethod]
        public async Task UpdateUserBatch_InternalServerError_ReturnsStatusCode500()
        {
            // Arrange
            var userStatuses = new List<UserAttendedDto>
        {
            new UserAttendedDto { Id = 4, IsAttended = true }
        };

            _mockUserService
                .Setup(x => x.UpdateUserAllowStatusAsync(It.IsAny<List<UserAttendedDto>>()))
                .ReturnsAsync(new ServiceResponse<bool> { Success = false, Message = "Internal server error." });

            // Act
            var result = await _userController.UpdateUserBatch(userStatuses) as StatusCodeResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }

        

      
    }


}



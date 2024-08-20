using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.RegistrationService
{
    public class RegistrationTestService : IRegistrationTestService
    {


        private readonly ApplicationDataContext dataContext; // Dependency injection for DataContext

        // Constructor with dependency injection
        public RegistrationTestService(ApplicationDataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        //public  ServiceResponse<bool> GetUser(UserDto user)
        //{
        //    var response = new ServiceResponse<bool>();
        //    try
        //    {
        //    // check if the user with the current userDto available in database  
        //     var isUserExisist = dataContext.Users.Where(x=> x.Email == user.Email).FirstOrDefault();
             
        //        if(isUserExisist != null)
        //        {
        //            var req = isUserExisist.Email == user.Email ? isUserExisist.FullName == user.FullName ? isUserExisist.PhoneNumber == user.PhoneNumber ? true : false : false : false ;

        //            response.Data = req;
        //            response.Success = req;
        //            response.Message = "User Registration Successfull";
        //        }
        //        else
        //        {
        //            response.Data = false;
        //            response.Success = false;
        //            response.Message = "User Not Fount";
        //        }

        //    }catch (Exception ex)
        //    {
        //        response.Data = false;
        //        response.Success = false;
        //        response.Message = "User Not Fount";

        //    }


        //    return response;
        //}

        public ServiceResponse<bool> GetUser(UserDto user)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Check if the user with the current UserDto is available in the database
                var existingUser = dataContext.Users
                    .FirstOrDefault(x => x.Email == user.Email);

                if (existingUser != null)
                {
                  // check new added user
                    bool detailsMatch = existingUser.FullName == user.FullName &&
                                        existingUser.PhoneNumber == user.PhoneNumber;

                    response.Data = detailsMatch;
                    response.Success = detailsMatch;
                    response.Message = detailsMatch ? "User details are correct." : "User details do not match.";
                }
                else
                {
                    response.Data = false;
                    response.Success = false;
                    response.Message = "User not found.";
                }
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = "An error occurred while processing the request.";
            }

            return response;
        }

    }
}

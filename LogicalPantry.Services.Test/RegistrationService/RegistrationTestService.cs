using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
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
        private IConfiguration _configuration;


        // Constructor with dependency injection
        public RegistrationTestService()
        {

            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();

            //Set up configuration to load appsettings json 
            var builder1 = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder1.Build();

            var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

            builder.UseSqlServer(connectionString);

            // Initialize dataContext with the configured options
            this.dataContext = new ApplicationDataContext(builder.Options);
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

                // Check All Data of User 
               
                var existingUser = dataContext.Users
                    .FirstOrDefault(x => x.Email == user.Email);

                if (existingUser != null)
                {
                  
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

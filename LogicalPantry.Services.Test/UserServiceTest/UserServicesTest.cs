using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.UserServiceTest
{
    public class UserServicesTest : IUserServiceTest
    {
           private readonly ApplicationDataContext dataContext;
        public UserServicesTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();


            builder.UseSqlServer("");

            // Initialize dataContext with the configured options
            this.dataContext = new ApplicationDataContext(builder.Options);
        }

            public async Task CheckUserDeleteResponse(UserDto user)
            {
                // Check if user still exists in the database
                var existingUser = await dataContext.Users.FindAsync(user.Id);

                if (existingUser != null)
                {
                    throw new Exception($"User with ID {user.Id} was not deleted.");
                }
            }

            public async Task CheckUserPostResponse(UserDto user)
            {
                // Check if the newly created user matches the provided UserDto
                var createdUser = await dataContext.Users.FindAsync(user.Id);

                if (createdUser == null)
                {
                    throw new Exception($"User with ID {user.Id} was not found after creation.");
                }

                // Compare the created user with the provided user details
                if (!UserDtoEquals(createdUser, user))
                {
                    throw new Exception($"User data does not match the provided UserDto. Expected: {user}, Actual: {createdUser}");
                }
            }

            public async Task CheckUserPutResponse(UserDto user)
            {
                // Check if the updated user matches the provided UserDto
                var updatedUser = await dataContext.Users.FindAsync(user.Id);

                if (updatedUser == null)
                {
                    throw new Exception($"User with ID {user.Id} was not found after update.");
                }

                // Compare the updated user with the provided user details
                if (!UserDtoEquals(updatedUser, user))
                {
                    throw new Exception($"User data does not match the provided UserDto after update. Expected: {user}, Actual: {updatedUser}");
                }
            }

            private bool UserDtoEquals(User existingUser, UserDto userDto)
            {
                
                return existingUser.Id == userDto.Id &&
                       existingUser.FullName == userDto.FullName &&
                       existingUser.Email == userDto.Email && existingUser.IsRegistered == userDto.IsRegistered && existingUser.IsAllow == existingUser.IsAllow ;


            }
        }




        //public Task CheckUserDeleteResponse(UserDto user)
        //{
        //    // Check User Deleted
        //    throw new NotImplementedException();
        //}

        //public Task CheckUserPostResponse(UserDto user)
        //{
        //    // check all data for current user to database  with eaxt mathing dto // check all data for current user to database  with eaxt mathing dto
        //    throw new NotImplementedException();
        //}

        //public Task CheckUserPutResponse(UserDto user)
        //{
        //    // check all data for current user to database  with eaxt mathing dto
        //    throw new NotImplementedException();
        //}
    }


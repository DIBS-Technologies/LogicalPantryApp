using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TimeSlotSignUpService
{
    public class TimeSlotSignUpTestService : ITimeSlotSignUpTestService
    {
        private readonly ApplicationDataContext _context;
        private readonly IConfiguration _configuration;

        public TimeSlotSignUpTestService(ApplicationDataContext context)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();


            //Set up configuration to load appsettings json 
            var builderConnectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builderConnectionString.Build();

            var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

            builder.UseSqlServer(connectionString);

            // Initialize dataContext with the configured options
            _context = new ApplicationDataContext(builder.Options);
        }

        // Get the timeslot from the database
        public async Task<ServiceResponse<TimeSlotSignupDto>> GetTimeSlot(TimeSlotSignupDto timeSlotSignupDto)
        {
            var response = new ServiceResponse<TimeSlotSignupDto>();

            if (timeSlotSignupDto == null)
            {
                response.Success = false;
                response.Message = "timeslot dto is empty";
                return response;
            }
            try
            {
                var timeslot = await _context.TimeSlotSignups
                    .FirstOrDefaultAsync(u => u.UserId == timeSlotSignupDto.UserId && u.TimeSlotId == timeSlotSignupDto.TimeSlotId);


                if (timeslot != null)
                {
                    response.Success = true;
                    response.Data = new TimeSlotSignupDto
                    {
                        UserId = timeslot.UserId,
                        TimeSlotId = timeslot.TimeSlotId,
                        Attended = timeslot.Attended,
                    };
                }
                else
                {
                    response.Success = false;
                    response.Message = "TimeSlot not found";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occurred while getting timeslot: {ex.Message}";
            }

            return response;
        }

       
    }
}

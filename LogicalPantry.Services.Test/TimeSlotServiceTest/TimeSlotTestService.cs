using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.TimeSlotDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.TimeSlotServiceTest
{
    public class TimeSlotTestService : ITimeSlotTestService
    {
        private readonly TestApplicationDataContext _context;
        private readonly IConfiguration _configuration;

        public TimeSlotTestService()
        {
            var builder = new DbContextOptionsBuilder<TestApplicationDataContext>();
            //Set up configuration to load appsettings json 
            var builderConnectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) //Ensure the correct path
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
               _configuration = builderConnectionString.Build();

            var connectionString = _configuration.GetConnectionString("DefaultSQLConnection");

            builder.UseSqlServer(connectionString);

            // Initialize dataContext with the configured options
            _context = new TestApplicationDataContext(builder.Options);

        }

        public async Task<ServiceResponse<TimeSlotDto>> GetEvent(TimeSlotDto timeSlotDto)
        {
            var response = new ServiceResponse<TimeSlotDto>();

            var result = await _context.TimeSlots.Where(ts => ts.TimeSlotName == timeSlotDto.TimeSlotName
                                                && ts.TenantId == ts.TenantId
                                                && ts.UserId == timeSlotDto.UserId).FirstOrDefaultAsync();
            try
            {
                if(result != null)
                {
                    response.Message = "Event found Successfully";
                    response.Data = new TimeSlotDto
                    {
                        Id = result.Id,
                        TenantId = result.TenantId,
                        UserId = result.UserId,
                        TimeSlotName = result.TimeSlotName,
                        StartTime = result.StartTime,
                        EndTime = result.EndTime,
                    };
                    return response;
                }
                else
                {
                    response.Message = "Event is not found";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occured while Getting a Event :{ex.Message}";
            }

            return response;
        }
    }
}

using LogicalPantry.DTOs;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using Microsoft.EntityFrameworkCore;
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

        public TimeSlotSignUpTestService(ApplicationDataContext context)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDataContext>();


            builder.UseSqlServer("Server=Server1\\SQL19Dev,12181;Database=LogicalPantryDB;User ID=sa;Password=x3wXyCrs;MultipleActiveResultSets=true;TrustServerCertificate=True");

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
                var timeslot = await _context.TimeSlotSignups.FirstOrDefaultAsync(u => u.UserId == timeSlotSignupDto.UserId);

                if (timeslot != null)
                {
                    response.Success = true;
                    response.Data = new TimeSlotSignupDto
                    {
                        Id = timeslot.Id,
                        UserId = timeslot.UserId,
                        TimeSlotId = timeslot.Id,
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

        Task<TimeSlotSignupDto> ITimeSlotSignUpTestService.GetTimeSlot(TimeSlotSignupDto timeSlotSignupDto)
        {
            throw new NotImplementedException();
        }
    }
}

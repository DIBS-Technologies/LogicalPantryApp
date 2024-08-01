using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.UserServices
{
    public interface IUserService
    {
        List<UserDto> Get(int tenentId);
        List<UserDto> GetUsersbyTimeSlot(DateTime timeSlot, int tenentId);
        string Post(List<UserDto> user);
       
    }
}

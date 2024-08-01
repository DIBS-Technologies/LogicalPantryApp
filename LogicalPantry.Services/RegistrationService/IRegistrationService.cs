using LogicalPantry.DTOs;
using LogicalPantry.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.RegistrationService
{
    public interface IRegistrationService
    {
        Task<ServiceResponse<bool>> RegisterUser(UserDto user);
    }
}


using LogicalPantry.DTOs.Test;
using LogicalPantry.DTOs.Test.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.Test.RegistrationService
{
    public interface IRegistrationTestService
    {
        ServiceResponse<bool> GetUser(UserDto user);
    }
}

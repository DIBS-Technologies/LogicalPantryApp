using LogicalPantry.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalPantry.Services.AuthenticationService
{
    public interface IAuthService 
    {
        ServiceResponse<bool> IsEmailUsed(string emailId);
    }
}

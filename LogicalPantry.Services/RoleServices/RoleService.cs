using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LogicalPantry.DTOs;
 
using LogicalPantry.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicalPantry.DTOs.Roledtos;

namespace LogicalPantry.Services.RoleServices
{
    public class RoleService : IRoleService
    {
        private readonly ILogger<RoleService> logger;
        private readonly IMapper mapper;
        private readonly ApplicationDataContext dataContext;

        public RoleService(ILogger<RoleService> logger, IMapper mapper, ApplicationDataContext dataContext)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.dataContext = dataContext;
        }


        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            var role = await dataContext.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null) return; // Role does not exist

            var user = await dataContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Remove existing role if it exists
                var existingRole = user.UserRoles.FirstOrDefault();
                if (existingRole != null)
                {
                    dataContext.UserRoles.Remove(existingRole);
                }

                // Add new role
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
                await dataContext.SaveChangesAsync();
            }
        }

        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            var role = await dataContext.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            return role != null ? mapper.Map<RoleDto>(role) : null;
        }


    }
}

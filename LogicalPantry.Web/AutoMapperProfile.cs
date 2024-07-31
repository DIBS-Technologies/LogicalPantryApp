using AutoMapper;
using LogicalPantry.DTOs.Roledtos;
using LogicalPantry.DTOs.TenantDtos;
using LogicalPantry.DTOs.TimeSlotDtos;
using LogicalPantry.DTOs.TimeSlotSignupDtos;
using LogicalPantry.DTOs.UserDtos;
using LogicalPantry.Models.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogicalPantry.Web
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //user table mapping
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<User, UserAllowStatusDto>();
            CreateMap<UserAllowStatusDto, User>();

            //Role Table Mapping 
            CreateMap<Role, RoleDto>();
            CreateMap<RoleDto, Role>();

            //PickupSlot Table Mapping 
            CreateMap<TimeSlot, TimeSlotDto>();
            CreateMap<TimeSlotDto, TimeSlot>();

            //PickUpHistory Table Mapping 
            CreateMap<TimeSlotSignup, TimeSlotSignupDto>();
            CreateMap<TimeSlotSignupDto, TimeSlotSignup>();

            //Tenant Table Mapping 
            CreateMap<Tenant, TenantDto>();
            CreateMap<TenantDto, Tenant>();

        }

    }
}

using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using Inventory.Core.Entities.Users.ApplicationUsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class AccountsAutoMapperProfile : Profile
    {
        AccountsAutoMapperProfile()
        {
            CreateMap<RegisterRequestDto, ApplicationUser>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<PersonalDataRequestDto, ApplicationUser>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<ApplicationUser, UserResponseDto>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())  
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); 

            CreateMap<ApplicationUser, PersonalDataResponseDto>();


            CreateMap<RoleRequestDto, ApplicationRole>();

            CreateMap<ApplicationRole, RoleResponseDto>();
        }
    }
}

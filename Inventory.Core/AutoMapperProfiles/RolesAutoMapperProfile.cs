using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Users.ApplicationRoles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class RolesAutoMapperProfile : Profile
    {
        public RolesAutoMapperProfile()
        {
            CreateMap<RoleRequestDto, ApplicationRole>();

            CreateMap<ApplicationRole, RoleResponseDto>();
        }
    }
}

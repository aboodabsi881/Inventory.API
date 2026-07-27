using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class CategoriesAutoMapperProfile : Profile
    {
        public CategoriesAutoMapperProfile()
        {
            CreateMap<CategoryRequestDto, Category>();

            CreateMap<Category, CategoryResponseDto>();
        }
    }
}
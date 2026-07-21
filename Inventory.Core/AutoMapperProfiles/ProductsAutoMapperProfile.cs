using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class ProductsAutoMapperProfile : Profile
    {
        public ProductsAutoMapperProfile()
        {
            CreateMap<ProductRequestDto, Product>();

            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.PowerTypeName, opt => opt.MapFrom(src => src.PowerType.HasValue ? src.PowerType.Value.ToString() : null));
        }
    }
}

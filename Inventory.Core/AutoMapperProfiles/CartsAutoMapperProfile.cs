using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class CartsAutoMapperProfile : Profile
    {
        public CartsAutoMapperProfile() 
        {
            CreateMap<CartRequestDto, Cart>();

            CreateMap<Cart, CartResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price));
        }
    }
}

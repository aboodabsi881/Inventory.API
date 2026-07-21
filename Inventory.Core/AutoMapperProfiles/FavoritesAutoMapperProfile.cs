using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Favorites;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.AutoMapperProfiles
{
    public class FavoritesAutoMapperProfile : Profile
    {
        public FavoritesAutoMapperProfile()
        {
            CreateMap<FavoriteRequestDto, Favorite>();

            CreateMap<Favorite, FavoriteResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImg, opt => opt.MapFrom(src => src.Product.Img));
        }
    }
}

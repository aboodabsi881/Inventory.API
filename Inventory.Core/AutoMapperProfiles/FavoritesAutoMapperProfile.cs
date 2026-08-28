using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Favorites;

namespace Inventory.Core.AutoMapperProfiles
{
    public class FavoritesAutoMapperProfile : Profile
    {
        public FavoritesAutoMapperProfile()
        {
            CreateMap<FavoriteRequestDto, Favorite>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<Favorite, FavoriteResponseDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0m))
                .ForMember(dest => dest.ProductImg,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Img : "/images/placeholder.png"));
        }
    }
}
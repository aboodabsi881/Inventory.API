using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;

namespace Inventory.Core.AutoMapperProfiles
{
    public class CartsAutoMapperProfile : Profile
    {
        public CartsAutoMapperProfile()
        {
            CreateMap<CartRequestDto, Cart>();

            CreateMap<Cart, CartResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImg, opt => opt.MapFrom(src => src.Product.Img)); 
        }
    }
}
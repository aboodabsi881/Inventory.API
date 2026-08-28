using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Carts;

namespace Inventory.Core.AutoMapperProfiles
{
    public class CartsAutoMapperProfile : Profile
    {
        public CartsAutoMapperProfile()
        {
            CreateMap<CartRequestDto, Cart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<Cart, CartResponseDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductPrice,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0m))
                .ForMember(dest => dest.ProductImg,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Img : "/images/placeholder.png"))
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.Quantity * (src.Product != null ? src.Product.Price : 0m)));
        }
    }
}
using AutoMapper;
using Inventory.Core.DTOs;
using Inventory.Core.Entities.Products;

namespace Inventory.Core.AutoMapperProfiles
{
    public class ProductsAutoMapperProfile : Profile
    {
        public ProductsAutoMapperProfile()
        {
            CreateMap<ProductRequestDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.PowerTypeName, opt => opt.MapFrom(src => src.PowerType.HasValue ? src.PowerType.Value.ToString() : null))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity)); ;
        }
    }
}
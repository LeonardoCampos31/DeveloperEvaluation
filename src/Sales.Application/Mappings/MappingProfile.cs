using AutoMapper;
using Sales.Application.Commands.Product;
using Sales.Application.Commands.Sales;
using Sales.Application.DTOs;
using Sales.Domain.Entities;

namespace Sales.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing(src => new Product(Guid.NewGuid(), src.Title, src.Price, src.Description, src.Category));

        CreateMap<SaleItem, SaleItemDto>().ReverseMap();
        CreateMap<CreateSaleCommand.SaleItemInputDto, Sale.SaleItemInput>();

        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.SaleDate));
    }
}
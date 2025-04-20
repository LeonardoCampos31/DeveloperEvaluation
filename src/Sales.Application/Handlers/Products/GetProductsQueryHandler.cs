using AutoMapper;
using MediatR;
using Sales.Application.Commands.Products;
using Sales.Application.DTOs;
using Sales.Domain.Interfaces;

namespace Sales.Application.Handlers.Products;

public class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<ProductDto>>(products);
    }
}
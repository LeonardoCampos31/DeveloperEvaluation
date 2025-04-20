using MediatR;
using Sales.Application.DTOs;

namespace Sales.Application.Commands.Products;

public record GetProductsQuery() : IRequest<IEnumerable<ProductDto>>;
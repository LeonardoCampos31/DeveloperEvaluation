using MediatR;
using Sales.Application.DTOs;

namespace Sales.Application.Commands.Product;

public record CreateProductCommand(
    string Title,
    decimal Price,
    string Description,
    string Category) : IRequest<ProductDto>;
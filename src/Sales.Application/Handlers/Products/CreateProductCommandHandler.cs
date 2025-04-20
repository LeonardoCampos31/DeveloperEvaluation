using AutoMapper;
using MediatR;
using Sales.Application.Commands.Product;
using Sales.Application.DTOs;
using Sales.Domain.Entities;
using Sales.Domain.Events;
using Sales.Domain.Interfaces;

namespace Sales.Application.Handlers.Products;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IMapper mapper,
    IEventPublisher eventPublisher)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = _mapper.Map<Product>(request);

        await _productRepository.AddAsync(product, cancellationToken);

        await _productRepository.SaveChangesAsync(cancellationToken);

        var productCreatedEvent = new ProductCreatedEvent(product.Id, product.Title, product.Price);
        await eventPublisher.PublishAsync(productCreatedEvent, cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }
}
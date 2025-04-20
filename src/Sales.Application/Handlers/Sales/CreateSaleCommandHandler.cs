using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Application.Commands.Sales;
using Sales.Application.DTOs;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Domain.Events;

namespace Sales.Application.Handlers.Sales;

public class CreateSaleCommandHandler(
    ISaleRepository saleRepository,
    IProductRepository productRepository,
    IMapper mapper,
    ILogger<CreateSaleCommandHandler> logger)
    : IRequestHandler<CreateSaleCommand, SaleDto>
{
    private readonly ISaleRepository _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<CreateSaleCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        await Validator(request, cancellationToken);
        
        var saleItemInputs = _mapper.Map<IEnumerable<Sale.SaleItemInput>>(request.Items);

        var sale = new Sale(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerId,
            request.BranchId,
            saleItemInputs
        );

        await _saleRepository.AddAsync(sale, cancellationToken);

        await _saleRepository.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
        {
            if (domainEvent is SaleCreatedEvent createdEvent)
            {
                 _logger.LogInformation("----- Domain Event Published: {EventName} - SaleId: {SaleId}, Total: {TotalAmount} -----",
                    nameof(SaleCreatedEvent), createdEvent.SaleId, createdEvent.TotalAmount);
            }
        }
        sale.ClearDomainEvents();
        
        return _mapper.Map<SaleDto>(sale);
    }

    private async Task Validator(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var saleExist = await _saleRepository.GetBySaleNumerAsync(request.SaleNumber);
        if (saleExist)
        {
            throw new ValidationException(
                $"Sale Number already exist: {request.SaleNumber}.");
        }
        
        foreach (var itemDto in request.Items)
        {
            var currentProductPrice = await _productRepository.GetProductPriceAsync(itemDto.ProductId, cancellationToken);
            
            var exists = await _productRepository.ProductExistsAsync(itemDto.ProductId, cancellationToken);
            if (!exists)
            {
                throw new NotFoundException(nameof(Product), itemDto.ProductId);
            }
            
            if (!currentProductPrice.HasValue || currentProductPrice.Value != itemDto.UnitPrice)
            {
                throw new ValidationException(
                    $"Product price mismatch for item {itemDto.ProductId}. Expected {currentProductPrice?.ToString() ?? "N/A"} but got {itemDto.UnitPrice}.");
            }
        }
    }
}
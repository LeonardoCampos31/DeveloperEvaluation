using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Application.Commands.Sales;
using Sales.Domain.Interfaces;
using Sales.Domain.Events;
using Sales.Domain.Entities;

namespace Sales.Application.Handlers.Sales;

public class CancelSaleCommandHandler(ISaleRepository saleRepository, ILogger<CancelSaleCommandHandler> logger)
    : IRequestHandler<CancelSaleCommand, bool>
{
    private readonly ISaleRepository _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
    private readonly ILogger<CancelSaleCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);

        if (sale == null)
        {
            throw new NotFoundException(nameof(Sale), request.SaleId);
        }

        if (sale.Cancelled)
        {
            return true;
        }

        sale.CancelSale();

        var success = await _saleRepository.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in sale.DomainEvents)
        {
             if (domainEvent is SaleCancelledEvent cancelledEvent)
             {
                 _logger.LogInformation("----- Domain Event Published: {EventName} - SaleId: {SaleId} -----",
                     nameof(SaleCancelledEvent), cancelledEvent.SaleId);
             }
        }
        sale.ClearDomainEvents();

        return success;
    }
}

public class NotFoundException(string entityName, object key)
    : ApplicationException($"Entity \"{entityName}\" ({key}) was not found.");
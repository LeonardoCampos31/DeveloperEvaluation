using MediatR;

namespace Sales.Application.Commands.Sales;

public record CancelSaleCommand(Guid SaleId) : IRequest<bool>;
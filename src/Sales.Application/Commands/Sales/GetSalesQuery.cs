using MediatR;
using Sales.Application.DTOs;

namespace Sales.Application.Commands.Sales;

public record GetSalesQuery() : IRequest<IEnumerable<SaleDto>>;
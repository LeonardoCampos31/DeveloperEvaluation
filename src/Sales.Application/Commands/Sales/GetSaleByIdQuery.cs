using MediatR;
using Sales.Application.DTOs;

namespace Sales.Application.Commands.Sales;

public record GetSaleByIdQuery(Guid SaleId) : IRequest<SaleDto>;
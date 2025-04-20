using MediatR;
using Sales.Application.DTOs;

namespace Sales.Application.Commands.Sales;

public record CreateSaleCommand(
    string SaleNumber,
    DateTime SaleDate,
    Guid CustomerId,
    Guid BranchId,
    List<CreateSaleCommand.SaleItemInputDto> Items) : IRequest<SaleDto>
{
    public record SaleItemInputDto(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice);
}
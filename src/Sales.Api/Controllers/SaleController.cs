using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.DTOs;
using Sales.Api.Models;
using Sales.Application.Commands.Sales;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SaleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSales(CancellationToken cancellationToken)
    {
        var sales = await _mediator.Send(new GetSalesQuery(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<SaleDto>>.Success(sales));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var saleDto = await _mediator.Send(command, cancellationToken);
        var response = ApiResponse<SaleDto>.Success(saleDto, "Venda criada com sucesso");
        return CreatedAtAction(nameof(GetSaleById), new { id = saleDto.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.SuccessMessage("Sell Cancelled"));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSaleById(Guid id, CancellationToken cancellationToken)
    {
         var sale = await _mediator.Send(new GetSaleByIdQuery(id), cancellationToken);
         return Ok(ApiResponse<SaleDto>.Success(sale));
    }
}
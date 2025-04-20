using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.DTOs;
using Sales.Api.Models;
using Sales.Application.Commands.Product;
using Sales.Application.Commands.Products;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(products));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var productDto = await _mediator.Send(command, cancellationToken);
        var response = ApiResponse<ProductDto>.Success(productDto, "Product create with success");
        return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Success(product));
    }
}
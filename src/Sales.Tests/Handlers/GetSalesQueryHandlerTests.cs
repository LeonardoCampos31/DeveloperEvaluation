using FluentAssertions;
using NSubstitute;
using Sales.Application.Commands.Sales;
using Sales.Application.DTOs;
using Sales.Application.Handlers.Sales;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Tests.Common;

namespace Sales.Tests.Handlers;

public class GetSalesQueryHandlerTests : TestBase
{
    private readonly GetSalesQueryHandler _handler;
    private readonly ISaleRepository _saleRepository;

    public GetSalesQueryHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new GetSalesQueryHandler(_saleRepository, Mapper);
    }

    [Fact]
    public async Task Handle_WhenSalesExist_ShouldReturnMappedSaleDtos()
    {
        // Arrange
        var query = new GetSalesQuery();
        var sales = new List<Sale>
        {
            new Sale("S1", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { new Sale.SaleItemInput(Guid.NewGuid(), 1, 10) }),
            new Sale("S2", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { new Sale.SaleItemInput(Guid.NewGuid(), 2, 20) })
        };

        _saleRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IEnumerable<Sale>>(sales));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        var saleDtos = result as SaleDto[] ?? result.ToArray();
        saleDtos.Should().NotBeNull();
        saleDtos.Should().HaveCount(sales.Count);
        saleDtos.First().SaleNumber.Should().Be(sales.First().SaleNumber);
        saleDtos.Last().SaleNumber.Should().Be(sales.Last().SaleNumber);
        saleDtos.First().Items.Should().NotBeEmpty(); // Verifica se itens foram mapeados
    }

    [Fact]
    public async Task Handle_WhenNoSalesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetSalesQuery();
        var emptyList = Enumerable.Empty<Sale>();

        // Configura mock para retornar lista vazia
        _saleRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(emptyList));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        var saleDtos = result as SaleDto[] ?? result.ToArray();
        saleDtos.Should().NotBeNull();
        saleDtos.Should().BeEmpty();
    }
}
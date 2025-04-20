using FluentAssertions;
using NSubstitute;
using Sales.Domain.Entities;
using Sales.Application.Commands.Sales;
using Sales.Application.Handlers.Sales;
using Sales.Domain.Interfaces;
using Sales.Tests.Common;

namespace Sales.Tests.Handlers;

public class GetSaleByIdQueryHandlerTests : TestBase
{
    private readonly GetSaleByIdQueryHandler _handler;
    private readonly ISaleRepository _saleRepository;

    public GetSaleByIdQueryHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new GetSaleByIdQueryHandler(_saleRepository, Mapper);
    }

    [Fact]
    public async Task Handle_WhenSaleExists_ShouldReturnMappedSaleDto()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var query = new GetSaleByIdQuery(saleId);
        var sale = new Sale("S1", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), [new Sale.SaleItemInput(Guid.NewGuid(), 1, 10)
        ]);
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Sale?>(sale));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());

        result.Should().NotBeNull();
        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSaleDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var query = new GetSaleByIdQuery(saleId);
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Sale?>(null));

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Entity \"Sale\" ({saleId}) was not found.");

        await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
    }
}
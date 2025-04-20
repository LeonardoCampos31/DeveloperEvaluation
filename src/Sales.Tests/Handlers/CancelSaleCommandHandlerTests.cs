using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Sales.Application.Commands.Sales;
using Sales.Application.Handlers.Sales;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Tests.Common;

namespace Sales.Tests.Handlers;

public class CancelSaleCommandHandlerTests : TestBase
{
    private readonly CancelSaleCommandHandler _handler;
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleCommandHandler> _typedLoggerMock;

    public CancelSaleCommandHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _typedLoggerMock = Substitute.For<ILogger<CancelSaleCommandHandler>>();
        _handler = new CancelSaleCommandHandler(_saleRepository, _typedLoggerMock);
    }

    [Fact]
    public async Task Handle_WhenSaleExistsAndNotCancelled_ShouldCancelSaleAndReturnTrue()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand(saleId);
        var sale = new Sale("S456", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), Guid.NewGuid(),
            new[] { new Sale.SaleItemInput(Guid.NewGuid(), 1, 10m) });
        sale.ClearDomainEvents();

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Sale?>(sale));

        // ---> GARANTA QUE ESTA LINHA (ou equivalente na classe base) ESTÁ ATIVA <---
        _saleRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        // ---> FIM DA VERIFICAÇÃO <---

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // ... outras asserções ...

        result.Should().BeTrue(); // Agora deve passar se o mock retornar true
    }

    [Fact]
    public async Task Handle_WhenSaleNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Sale?>(null));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Entity \"Sale\" ({saleId}) was not found.");
        await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _typedLoggerMock.DidNotReceive().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WhenSaleIsAlreadyCancelled_ShouldReturnTrueWithoutSaving()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var command = new CancelSaleCommand(saleId);
        var sale = new Sale("S789", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), Guid.NewGuid(),
            [new Sale.SaleItemInput(Guid.NewGuid(), 1, 10m)]);
        sale.CancelSale();
        sale.ClearDomainEvents();

         _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Sale?>(sale));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        result.Should().BeTrue();
         _typedLoggerMock.DidNotReceive().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }
}
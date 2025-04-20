using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Sales.Domain.Interfaces;
using Sales.Domain.Entities;
using Sales.Application.Mappings;
using Bogus;
using Sales.Application.Commands.Sales;
using Sales.Application.Handlers.Sales;

namespace Sales.Tests.Handlers
{
    public class CreateSaleCommandHandlerTests
    {
        private readonly ISaleRepository _saleRepositoryMock;
        private readonly IProductRepository _productRepositoryMock;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSaleCommandHandler> _loggerMock;
        private readonly Faker _faker;
        private readonly CreateSaleCommandHandler _handler;

        public CreateSaleCommandHandlerTests()
        {
            _saleRepositoryMock = Substitute.For<ISaleRepository>();
            _productRepositoryMock = Substitute.For<IProductRepository>();
            _loggerMock = Substitute.For<ILogger<CreateSaleCommandHandler>>();
            _faker = new Faker();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _handler = new CreateSaleCommandHandler(
                _saleRepositoryMock,
                _productRepositoryMock,
                _mapper,
                _loggerMock
            );

            _saleRepositoryMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
            _productRepositoryMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
            _saleRepositoryMock.AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateSaleAndReturnDto()
        {
            var product1Id = Guid.NewGuid();
            var product2Id = Guid.NewGuid();
            var product1Price = 10.00m;
            var product2Price = 20.00m;

            var command = new CreateSaleCommand(
                _faker.Random.AlphaNumeric(8),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [
                    new CreateSaleCommand.SaleItemInputDto(product1Id, 5, product1Price),
                    new CreateSaleCommand.SaleItemInputDto(product2Id, 15, product2Price)
                ]
            );

            Sale? capturedSale = null;
            await _saleRepositoryMock.AddAsync(Arg.Do<Sale>(s => capturedSale = s), Arg.Any<CancellationToken>());

            _saleRepositoryMock.GetBySaleNumerAsync(command.SaleNumber)
                               .Returns(Task.FromResult(false));
            _productRepositoryMock.ProductExistsAsync(product1Id, Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult(true));
            _productRepositoryMock.ProductExistsAsync(product2Id, Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult(true));
            _productRepositoryMock.GetProductPriceAsync(product1Id, Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult<decimal?>(product1Price));
            _productRepositoryMock.GetProductPriceAsync(product2Id, Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult<decimal?>(product2Price));

            var resultDto = await _handler.Handle(command, CancellationToken.None);

            await _saleRepositoryMock.Received(1).AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            await _saleRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _saleRepositoryMock.Received(1).GetBySaleNumerAsync(command.SaleNumber);
            await _productRepositoryMock.Received(1).ProductExistsAsync(product1Id, Arg.Any<CancellationToken>());
            await _productRepositoryMock.Received(1).ProductExistsAsync(product2Id, Arg.Any<CancellationToken>());
            await _productRepositoryMock.Received(1).GetProductPriceAsync(product1Id, Arg.Any<CancellationToken>());
            await _productRepositoryMock.Received(1).GetProductPriceAsync(product2Id, Arg.Any<CancellationToken>());

            capturedSale.Should().NotBeNull();
            capturedSale?.SaleNumber.Should().Be(command.SaleNumber);
            capturedSale?.CustomerId.Should().Be(command.CustomerId);
            capturedSale?.BranchId.Should().Be(command.BranchId);
            capturedSale?.Cancelled.Should().BeFalse();
            capturedSale?.Items.Should().HaveCount(2);

            var item1 = capturedSale?.Items.FirstOrDefault(i => i.ProductId == product1Id);
            item1.Should().NotBeNull();
            item1!.Quantity.Should().Be(5);
            item1.UnitPrice.Should().Be(product1Price);
            item1.ValueMonetaryTaxApplied.Should().Be(5.00m);
            item1.Total.Should().Be(55.00m);

            var item2 = capturedSale?.Items.FirstOrDefault(i => i.ProductId == product2Id);
            item2.Should().NotBeNull();
            item2!.Quantity.Should().Be(15);
            item2.UnitPrice.Should().Be(product2Price);
            item2.ValueMonetaryTaxApplied.Should().Be(60.00m);
            item2.Total.Should().Be(360.00m);

            capturedSale?.TotalAmount.Should().Be(item1.Total + item2.Total);

            resultDto.Should().NotBeNull();
            resultDto.Id.Should().Be(capturedSale!.Id);
            resultDto.SaleNumber.Should().Be(command.SaleNumber);
            resultDto.TotalAmount.Should().Be(capturedSale.TotalAmount);
            resultDto.Items.Should().HaveCount(2);

            _loggerMock.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("SaleCreatedEvent") && o.ToString()!.Contains(capturedSale.Id.ToString())),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task Handle_WhenSaleNumberExists_ShouldThrowValidationException()
        {
            // Arrange
            var existingSaleNumber = _faker.Random.AlphaNumeric(8);
            var command = new CreateSaleCommand(
                existingSaleNumber,
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [new CreateSaleCommand.SaleItemInputDto(Guid.NewGuid(), 1, 10.00m)]
            );

            var existingSale = new Sale(existingSaleNumber, DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), Guid.NewGuid(),
                [new Sale.SaleItemInput(Guid.NewGuid(), 1, 5m)]);

            _saleRepositoryMock.GetBySaleNumerAsync(existingSaleNumber)
                .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None); // Linha 146

            // Assert
            await act.Should().ThrowAsync<FluentValidation.ValidationException>() // <-- CORRIGIDO
                .WithMessage($"Sale Number already exist: {existingSaleNumber}."); // Linha 148

            await _saleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            await _saleRepositoryMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenProductDoesNotExist_ShouldThrowNotFoundException()
        {
            var nonExistentProductId = Guid.NewGuid();
            var command = new CreateSaleCommand(
                 _faker.Random.AlphaNumeric(8),
                 DateTime.UtcNow,
                 Guid.NewGuid(),
                 Guid.NewGuid(),
                 [new CreateSaleCommand.SaleItemInputDto(nonExistentProductId, 1, 10.00m)]
            );

            _saleRepositoryMock.GetBySaleNumerAsync(command.SaleNumber).Returns(Task.FromResult(false));
            _productRepositoryMock.ProductExistsAsync(nonExistentProductId, Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult(false));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage($"Entity \"Product\" ({nonExistentProductId}) was not found.");
            await _saleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            await _saleRepositoryMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenProductPriceMismatch_ShouldThrowValidationException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var priceInCommand = 10.00m;
            var actualDbPrice = 12.50m;
            var command = new CreateSaleCommand(
                _faker.Random.AlphaNumeric(8),
                DateTime.UtcNow,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [new CreateSaleCommand.SaleItemInputDto(productId, 1, priceInCommand)]
            );

            _saleRepositoryMock.GetBySaleNumerAsync(command.SaleNumber).Returns(Task.FromResult<bool>(false));
            _productRepositoryMock.ProductExistsAsync(productId, Arg.Any<CancellationToken>()).Returns(true);
            _productRepositoryMock.GetProductPriceAsync(productId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<decimal?>(actualDbPrice));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None); // Linha 197

            // Assert
            // Use o tipo explícito FluentValidation.ValidationException
            await act.Should().ThrowAsync<FluentValidation.ValidationException>() // <-- CORRIGIDO
                .WithMessage($"Product price mismatch for item {productId}. Expected {actualDbPrice} but got {priceInCommand}."); // Linha 199

            await _saleRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
            await _saleRepositoryMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
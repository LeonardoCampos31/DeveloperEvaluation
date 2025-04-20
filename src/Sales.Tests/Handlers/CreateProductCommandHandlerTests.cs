using FluentAssertions;
using NSubstitute;
using Sales.Application.Commands.Product;
using Sales.Application.Handlers.Products;
using Sales.Domain.Entities;
using Sales.Domain.Interfaces;
using Sales.Tests.Common; 
using Sales.Domain.Events;

namespace Sales.Tests.Handlers;

public class CreateProductCommandHandlerTests : TestBase
{
    private readonly CreateProductCommandHandler _handler;
    private readonly IProductRepository _productRepositoryMock;
    private readonly IEventPublisher _eventPublisherMock;

    public CreateProductCommandHandlerTests()
    {
        _eventPublisherMock = Substitute.For<IEventPublisher>();
        _productRepositoryMock = Substitute.For<IProductRepository>();
        
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock,
            Mapper,
            _eventPublisherMock
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddProductAndReturnDto()
    {
        // Arrange
        var command = new CreateProductCommand(
            Faker.Commerce.ProductName(),
            Faker.Random.Decimal(1, 1000),
            Faker.Lorem.Sentence(),
            Faker.Commerce.Categories(1)[0]
        );

        Product? capturedProduct = null;
        await _productRepositoryMock.AddAsync(Arg.Do<Product>(p => capturedProduct = p), Arg.Any<CancellationToken>());
        _productRepositoryMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));


        // Act
        var resultDto = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _productRepositoryMock.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
        await _productRepositoryMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        capturedProduct.Should().NotBeNull();
        capturedProduct?.Title.Should().Be(command.Title);
        capturedProduct?.Price.Should().Be(command.Price);
        // ... assert other properties

        resultDto.Should().NotBeNull();
        resultDto.Id.Should().Be(capturedProduct!.Id);
        resultDto.Title.Should().Be(command.Title);
        
        await _eventPublisherMock.Received(1).PublishAsync(Arg.Any<ProductCreatedEvent>(), Arg.Any<CancellationToken>());
    }
}
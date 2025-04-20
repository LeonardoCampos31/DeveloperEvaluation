using Bogus;
using FluentValidation.TestHelper;
using NSubstitute;
using Sales.Application.Commands.Sales;
using Sales.Application.Validators;
using Sales.Domain.Interfaces;
using Sales.Tests.Common;

namespace Sales.Tests.Validators;

public class CreateSaleCommandValidatorTests : TestBase
{
    private readonly CreateSaleCommandValidator _validator = new();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly Faker _faker = new();

    private CreateSaleCommand CreateValidCommand(List<CreateSaleCommand.SaleItemInputDto>? items = null)
    {
        items ??= [new CreateSaleCommand.SaleItemInputDto(Guid.NewGuid(), 5, 10m)];
        return new CreateSaleCommand(
            Faker.Random.AlphaNumeric(8),
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            items
        );
    }

    [Fact]
    public async Task Should_HaveError_When_SaleNumber_IsEmpty()
    {
        var command = CreateValidCommand();
        var invalidCommand = command with { SaleNumber = string.Empty }; // Usa record 'with' expression
        var result = await _validator.TestValidateAsync(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.SaleNumber);
    }

    [Fact]
    public async Task Should_NotHaveError_When_Command_IsValid()
    {
        // Arrange
        var command = CreateValidCommand();
        var productIdInCommand = command.Items[0].ProductId;
        var unitPriceInCommand = command.Items[0].UnitPrice;
        _productRepository.ProductExistsAsync(productIdInCommand, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        _productRepository.GetProductPriceAsync(productIdInCommand, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<decimal?>(unitPriceInCommand));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_HaveError_When_CustomerId_IsEmpty()
    {
        var command = CreateValidCommand();
        var invalidCommand = command with { CustomerId = Guid.Empty };
        var result = await _validator.TestValidateAsync(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.CustomerId);
    }

     [Fact]
    public async Task Should_HaveError_When_BranchId_IsEmpty()
    {
        var command = CreateValidCommand();
        var invalidCommand = command with { BranchId = Guid.Empty };
        var result = await _validator.TestValidateAsync(invalidCommand);
        result.ShouldHaveValidationErrorFor(c => c.BranchId);
    }

     [Fact]
    public async Task Should_HaveError_When_Items_IsEmptyList()
    {
        var command = CreateValidCommand(new List<CreateSaleCommand.SaleItemInputDto>()); // Lista vazia
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task Should_HaveError_When_ItemQuantity_IsOutOfRange(int invalidQuantity)
    {
        var items = new List<CreateSaleCommand.SaleItemInputDto>
        {
            new(Guid.NewGuid(), invalidQuantity, 10m)
        };
        var command = CreateValidCommand(items);
         _productRepository.ProductExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity") // Acessa a propriedade do item
              .WithErrorMessage("You cannot buy less than 1 or more than 20 pieces of the same item.");
    }

    [Fact]
    public async Task Should_HaveError_When_ItemProductId_IsEmpty()
    {
         var items = new List<CreateSaleCommand.SaleItemInputDto>
        {
            new(Guid.Empty, 5, 10m) // ID do produto vazio
        };
        var command = CreateValidCommand(items);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public async Task Should_HaveError_When_ItemUnitPrice_IsNotPositive(decimal invalidPrice)
    {
         var items = new List<CreateSaleCommand.SaleItemInputDto>
        {
            new(Guid.NewGuid(), 5, invalidPrice)
        };
        var command = CreateValidCommand(items);
         _productRepository.ProductExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }   
}
using FluentAssertions;
using Sales.Domain.Entities;

namespace Sales.Tests.Entities;

public class SaleItemTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_CalledBySale_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var unitPrice = 10m;
        var saleInput = new Sale.SaleItemInput(productId, invalidQuantity, unitPrice);

        // Act
        Action act = () => new Sale("S123", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { saleInput });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Quantity must be positive.*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.5)]
    public void Constructor_CalledBySale_WithInvalidUnitPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 1;
        var saleInput = new Sale.SaleItemInput(productId, quantity, invalidPrice);

        // Act
        Action act = () => new Sale("S123", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { saleInput });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Unit price must be positive.*");
    }

     [Fact]
    public void Constructor_CalledBySale_WithEmptyProductId_ShouldThrowArgumentException()
    {
        // Arrange
        var productId = Guid.Empty;
        var quantity = 1;
         var unitPrice = 10m;
        var saleInput = new Sale.SaleItemInput(productId, quantity, unitPrice);

        // Act
        Action act = () => new Sale("S123", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { saleInput });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Product ID cannot be empty.*");
    }

    [Theory]
    [InlineData(3, 10.00, 0.00, 30.00)]  // Qtd <= 4 -> Tax Free [source: 42]
    [InlineData(4, 10.00, 0.00, 40.00)]  // Qtd <= 4 -> Tax Free [source: 42]
    [InlineData(5, 10.00, 5.00, 55.00)]  // 4 < Qtd < 10 -> IVA 10% [source: 42]
    [InlineData(9, 10.00, 9.00, 99.00)]  // 4 < Qtd < 10 -> IVA 10% [source: 42]
    [InlineData(10, 10.00, 20.00, 120.00)] // 10 <= Qtd <= 20 -> SPECIAL IVA 20% [source: 42]
    [InlineData(20, 10.00, 40.00, 240.00)] // 10 <= Qtd <= 20 -> SPECIAL IVA 20% [source: 42]
    public void CalculateTaxAndTotal_CalledDuringSaleCreation_ShouldCalculateCorrectTaxAndTotal(int quantity, decimal unitPrice, decimal expectedTax, decimal expectedTotal)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var saleInput = new Sale.SaleItemInput(productId, quantity, unitPrice);

        // Act
        var sale = new Sale("S123", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { saleInput });
        var item = sale.Items.First();

        // Assert
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(unitPrice);
        item.ValueMonetaryTaxApplied.Should().Be(expectedTax);
        item.Total.Should().Be(expectedTotal);
        item.IsCancelled.Should().BeFalse();
    }
}
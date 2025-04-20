using FluentAssertions;
using Sales.Domain.Entities;
using Bogus;
using Sales.Domain.Events;

namespace Sales.Tests.Entities;

public class SaleTests
{
    private readonly Faker _faker = new();

    private Sale.SaleItemInput CreateValidItemInput(int quantity = 1)
    {
        return new Sale.SaleItemInput(Guid.NewGuid(), quantity, _faker.Random.Decimal(1, 100));
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateSaleAndCalculateTotal()
    {
        // Arrange
        var saleNumber = "SN001";
        var saleDate = DateTime.UtcNow;
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var itemInput1 = new Sale.SaleItemInput(Guid.NewGuid(), 2, 10m); // Total = 20
        var itemInput2 = new Sale.SaleItemInput(Guid.NewGuid(), 5, 20m); // Total = 110 (IVA 10%)
        var items = new[] { itemInput1, itemInput2 };
        var expectedTotal = 20m + 110m;

        // Act
        var sale = new Sale(saleNumber, saleDate, customerId, branchId, items);

        // Assert
        sale.Id.Should().NotBeEmpty();
        sale.SaleNumber.Should().Be(saleNumber);
        sale.SaleDate.Should().Be(saleDate);
        sale.CustomerId.Should().Be(customerId);
        sale.BranchId.Should().Be(branchId);
        sale.Cancelled.Should().BeFalse();
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(expectedTotal);

        sale.DomainEvents.Should().HaveCount(1);
        sale.DomainEvents.First().Should().BeOfType<SaleCreatedEvent>();
        var createdEvent = sale.DomainEvents.First() as SaleCreatedEvent;
        createdEvent?.SaleId.Should().Be(sale.Id);
        createdEvent?.TotalAmount.Should().Be(expectedTotal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidSaleNumber_ShouldThrowArgumentException(string invalidSaleNumber)
    {
        // Arrange
        var item = CreateValidItemInput();

        // Act
        Action act = () => new Sale(invalidSaleNumber, DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { item });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Sale number cannot be empty.*");
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        // Arrange
        var item = CreateValidItemInput();

        // Act
        Action act = () => new Sale("SN001", DateTime.UtcNow, Guid.Empty, Guid.NewGuid(), new[] { item });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Customer ID cannot be empty.*");
    }

     [Fact]
    public void Constructor_WithEmptyBranchId_ShouldThrowArgumentException()
    {
         // Arrange
        var item = CreateValidItemInput();

        // Act
        Action act = () => new Sale("SN001", DateTime.UtcNow, Guid.NewGuid(), Guid.Empty, new[] { item });

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Branch ID cannot be empty.*");
    }

    [Fact]
    public void Constructor_WithNullItems_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new Sale("SN001", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithItemQuantityGreaterThan20_ShouldThrowDomainException()
    {
         // Arrange
        var itemInput = new Sale.SaleItemInput(Guid.NewGuid(), 21, 10m);

        // Act
        Action act = () => new Sale("S123", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { itemInput });

        // Assert
        act.Should().Throw<DomainException>().WithMessage("Cannot add more than 20 pieces of the same item to a sale.");
    }

    [Fact]
    public void CancelSale_WhenSaleIsNotCancelled_ShouldMarkAsCancelledAndRecalculateTotalAndRaiseEvent()
    {
         // Arrange
        var itemInput = new Sale.SaleItemInput(Guid.NewGuid(), 2, 10m); // Total = 20
        var sale = new Sale("SN001", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), Guid.NewGuid(), new[] { itemInput });
        sale.ClearDomainEvents();

        // Act
        sale.CancelSale();
        var cancelDate = sale.SaleDate;

        // Assert
        sale.Cancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(0m);
        sale.SaleDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        sale.DomainEvents.Should().HaveCount(1);
        sale.DomainEvents.First().Should().BeOfType<SaleCancelledEvent>();
        var cancelledEvent = sale.DomainEvents.First() as SaleCancelledEvent;
        cancelledEvent?.SaleId.Should().Be(sale.Id);
        cancelledEvent?.CancellationDate.Should().Be(cancelDate);
    }

    [Fact]
    public void CancelSale_WhenSaleIsAlreadyCancelled_ShouldDoNothing()
    {
         // Arrange
        var itemInput = CreateValidItemInput();
        var sale = new Sale("SN001", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), Guid.NewGuid(), new[] { itemInput });
        sale.CancelSale();
        var firstCancelDate = sale.SaleDate;
        sale.ClearDomainEvents();

        // Act
        sale.CancelSale();

        // Assert
        sale.Cancelled.Should().BeTrue();
        sale.SaleDate.Should().Be(firstCancelDate);
        sale.TotalAmount.Should().Be(0m);
        sale.DomainEvents.Should().BeEmpty();
    }
}
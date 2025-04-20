using FluentAssertions;
using Sales.Domain.Entities;

namespace Sales.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Product";
        var price = 10.50m;
        var description = "Description";
        var category = "Category";

        // Act
        var product = new Product(id, title, price, description, category);

        // Assert
        product.Id.Should().Be(id);
        product.Title.Should().Be(title);
        product.Price.Should().Be(price);
        product.Description.Should().Be(description);
        product.Category.Should().Be(category);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10.0)]
    public void Constructor_WithInvalidPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Product";

        // Act
        Action act = () => new Product(id, title, invalidPrice, "Desc", "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Price must be positive.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var id = Guid.NewGuid();
        var price = 10m;

        // Act
        Action act = () => new Product(id, invalidTitle, price, "Desc", "Cat");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Title cannot be empty.*");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
         // Arrange
        var product = new Product(Guid.NewGuid(), "Title", 10m, "D", "C");
        var newPrice = 15.50m;

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5.0)]
    public void UpdatePrice_WithInvalidPrice_ShouldThrowArgumentException(decimal invalidPrice)
    {
         // Arrange
        var product = new Product(Guid.NewGuid(), "Title", 10m, "D", "C");

        // Act
        Action act = () => product.UpdatePrice(invalidPrice);

        // Assert
         act.Should().Throw<ArgumentException>().WithMessage("Price must be positive.*");
    }
}
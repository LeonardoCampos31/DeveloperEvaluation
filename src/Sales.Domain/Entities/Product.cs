namespace Sales.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    private Product() { }

    public Product(Guid id, string title, decimal price, string description, string category)
    {
        if (price <= 0) throw new ArgumentException("Price must be positive.", nameof(price));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = title;
        Price = price;
        Description = description ?? string.Empty;
        Category = category ?? string.Empty;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentException("Price must be positive.", nameof(newPrice));
        Price = newPrice;
    }
}
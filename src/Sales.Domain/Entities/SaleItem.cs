namespace Sales.Domain.Entities;

public class SaleItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; } // External Identity
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; } // Price at the time of sale
    public decimal ValueMonetaryTaxApplied { get; private set; }
    public decimal Total { get; private set; }
    public Guid SaleId { get; private set; } // Foreign Key
    public virtual Sale Sale { get; private set; } = null!; // Navigation Property
    public bool IsCancelled { get; private set; } // Usually items aren't cancelled individually, sale is

    // Private constructor for EF Core
    private SaleItem() { }

    // Internal constructor called by Sale entity
    internal SaleItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitPrice <= 0) throw new ArgumentException("Unit price must be positive.", nameof(unitPrice));
        if (productId == Guid.Empty) throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        IsCancelled = false;
        CalculateTaxAndTotal();
    }

    internal void CalculateTaxAndTotal()
    {
        decimal taxRate = 0m;
        if (Quantity > 4 && Quantity < 10)
        {
            taxRate = 0.10m;
        }
        else if (Quantity >= 10 && Quantity <= 20)
        {
            taxRate = 0.20m;
        }

        ValueMonetaryTaxApplied = (UnitPrice * Quantity) * taxRate;
        Total = (UnitPrice * Quantity) + ValueMonetaryTaxApplied;
    }

    internal void MarkAsCancelled()
    {
        IsCancelled = true;
    }
}
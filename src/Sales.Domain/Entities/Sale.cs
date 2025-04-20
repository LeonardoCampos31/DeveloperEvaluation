using Sales.Domain.Events;

namespace Sales.Domain.Entities;

public class Sale
{
    public Guid Id { get; private set; }
    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid BranchId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    private Sale() { }

    public Sale(string saleNumber, DateTime saleDate, Guid customerId, Guid branchId, IEnumerable<SaleItemInput> items)
    {
        if (string.IsNullOrWhiteSpace(saleNumber)) throw new ArgumentException("Sale number cannot be empty.", nameof(saleNumber));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));
        if (branchId == Guid.Empty) throw new ArgumentException("Branch ID cannot be empty.", nameof(branchId));

        Id = Guid.NewGuid();
        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        BranchId = branchId;
        Cancelled = false;

        ArgumentNullException.ThrowIfNull(items);

        foreach (var itemInput in items)
        {
            AddItem(itemInput.ProductId, itemInput.Quantity, itemInput.UnitPrice);
        }

        RecalculateTotalAmount();

        _domainEvents.Add(new SaleCreatedEvent(Id, SaleDate, CustomerId, TotalAmount));
    }

    private void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity > 20)
            throw new DomainException("Cannot add more than 20 pieces of the same item to a sale.");

        var newItem = new SaleItem(productId, quantity, unitPrice);
        _items.Add(newItem);
    }

    public void CancelSale()
    {
        if (Cancelled)
        {
            return;
        }
        Cancelled = true;
        SaleDate = DateTime.UtcNow;

        RecalculateTotalAmount();

        _domainEvents.Add(new SaleCancelledEvent(Id, SaleDate));
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = Cancelled ? 0 : _items.Sum(item => item.Total);
        // Or if cancelled sales should retain their original total:
        // if (!Cancelled) { TotalAmount = _items.Sum(item => item.Total); }
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // Helper record for constructor input
    public record SaleItemInput(Guid ProductId, int Quantity, decimal UnitPrice);
}

// Custom Domain Exception
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
namespace Sales.Domain.Events;

public record SaleCreatedEvent(Guid SaleId, DateTime SaleDate, Guid CustomerId, decimal TotalAmount);
public record SaleCancelledEvent(Guid SaleId, DateTime CancellationDate);
public record ProductCreatedEvent(Guid ProductId, string Title, decimal Price);
namespace Sales.Application.DTOs;

public class SaleItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
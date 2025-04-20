namespace Sales.Application.DTOs;

public class CreateSaleRequest
{
    public List<SaleItemRequest> Items { get; set; } = new();
}
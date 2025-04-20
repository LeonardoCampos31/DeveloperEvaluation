// Sales.Application/DTOs/SaleDto.cs
namespace Sales.Application.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; } 
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}
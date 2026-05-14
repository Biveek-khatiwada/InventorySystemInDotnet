namespace Inventory.ViewModels;

public class PurchaseVm
{
    public int VendorId { get; set; }
    public List<PurchaseItemVm> Items { get; set; } = new();
}
public class PurchaseItemVm
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }
}

public class SaleVm
{
    public int CustomerId { get; set; }
    public List<SaleItemVm> Items { get; set; } = new();
}
public class SaleItemVm
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

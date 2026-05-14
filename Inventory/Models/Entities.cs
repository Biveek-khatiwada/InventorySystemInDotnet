using System.ComponentModel.DataAnnotations;

namespace Inventory.Models;

public enum Status { Inactive = 0, Active = 1 }

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [Required, MaxLength(160)] public string Email { get; set; } = "";
    [MaxLength(40)] public string? MobileNo { get; set; }
    [Required] public string PasswordHash { get; set; } = "";
    public Status Status { get; set; } = Status.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public abstract class AuditedEntity
{
    public int Id { get; set; }
    public Status Status { get; set; } = Status.Active;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductGroup : AuditedEntity
{
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
}

public class UnitOfMeasure : AuditedEntity
{
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string Code { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
}

public class Product : AuditedEntity
{
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public int? ProductGroupId { get; set; }
    public ProductGroup? ProductGroup { get; set; }
    public decimal SalesPrice { get; set; }
}

public class Vendor : AuditedEntity
{
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
}

public class Customer : AuditedEntity
{
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
}

public class Purchase
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User? User { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseItem> Items { get; set; } = new();
}

public class PurchaseItem
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class Sale
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User? User { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal LineTotal { get; set; }
}

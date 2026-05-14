using Inventory.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<UnitOfMeasure>().HasIndex(u => u.Code).IsUnique();

        foreach (var t in new[] {
            typeof(PurchaseItem), typeof(SaleItem), typeof(Purchase), typeof(Sale), typeof(Product)
        })
        {
            // decimal precision
        }

        b.Entity<Product>().Property(p => p.SalesPrice).HasPrecision(18, 2);
        b.Entity<Purchase>().Property(p => p.TotalAmount).HasPrecision(18, 2);
        b.Entity<PurchaseItem>().Property(p => p.Quantity).HasPrecision(18, 3);
        b.Entity<PurchaseItem>().Property(p => p.PurchasePrice).HasPrecision(18, 2);
        b.Entity<PurchaseItem>().Property(p => p.SalesPrice).HasPrecision(18, 2);
        b.Entity<PurchaseItem>().Property(p => p.LineTotal).HasPrecision(18, 2);
        b.Entity<Sale>().Property(p => p.TotalAmount).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(p => p.Quantity).HasPrecision(18, 3);
        b.Entity<SaleItem>().Property(p => p.Price).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(p => p.LineTotal).HasPrecision(18, 2);

        b.Entity<Purchase>().HasMany(p => p.Items).WithOne(i => i.Purchase!).HasForeignKey(i => i.PurchaseId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Sale>().HasMany(s => s.Items).WithOne(i => i.Sale!).HasForeignKey(i => i.SaleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Name = "Admin",
                Email = "admin@inventory.local",
                MobileNo = "0000000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Status = Status.Active
            });
            db.SaveChanges();
        }
    }
}

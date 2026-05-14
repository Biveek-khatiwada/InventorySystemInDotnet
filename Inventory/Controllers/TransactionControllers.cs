using Inventory.Data;
using Inventory.Models;
using Inventory.Services;
using Inventory.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers;

[Authorize]
public class PurchasesController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public PurchasesController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }

    public async Task<IActionResult> Index() =>
        View(await _db.Purchases.Include(p => p.Vendor).Include(p => p.User).OrderByDescending(p => p.Date).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Purchases.Include(x => x.Vendor).Include(x => x.User)
            .Include(x => x.Items).ThenInclude(i => i.Product).ThenInclude(pr => pr!.UnitOfMeasure)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Vendors = await _db.Vendors.Where(v => v.Status == Status.Active).OrderBy(v => v.Name).ToListAsync();
        ViewBag.Products = await _db.Products.Include(p => p.UnitOfMeasure)
            .Where(p => p.Status == Status.Active).OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, Unit = p.UnitOfMeasure!.Code, p.SalesPrice })
            .ToListAsync();
    }

    [HttpGet] public async Task<IActionResult> Create() { await LoadLookupsAsync(); return View(new PurchaseVm()); }

    [HttpPost]
    public async Task<IActionResult> Create(PurchaseVm vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
        { ModelState.AddModelError("", "Add at least one item"); await LoadLookupsAsync(); return View(vm); }

        var purchase = new Purchase { VendorId = vm.VendorId, UserId = _cu.Id, Date = DateTime.UtcNow };
        foreach (var i in vm.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
        {
            var line = i.Quantity * i.PurchasePrice;
            purchase.Items.Add(new PurchaseItem
            {
                ProductId = i.ProductId, Quantity = i.Quantity,
                PurchasePrice = i.PurchasePrice, SalesPrice = i.SalesPrice, LineTotal = line
            });
            // update product sales price right then
            var prod = await _db.Products.FindAsync(i.ProductId);
            if (prod != null) prod.SalesPrice = i.SalesPrice;
        }
        purchase.TotalAmount = purchase.Items.Sum(x => x.LineTotal);
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = purchase.Id });
    }
}

[Authorize]
public class SalesController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public SalesController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }

    public async Task<IActionResult> Index() =>
        View(await _db.Sales.Include(s => s.Customer).Include(s => s.User).OrderByDescending(s => s.Date).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var s = await _db.Sales.Include(x => x.Customer).Include(x => x.User)
            .Include(x => x.Items).ThenInclude(i => i.Product).ThenInclude(pr => pr!.UnitOfMeasure)
            .FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? NotFound() : View(s);
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Customers = await _db.Customers.Where(c => c.Status == Status.Active).OrderBy(c => c.Name).ToListAsync();
        // For sales: only show products that have stock > 0 (computed)
        var stock = await StockController.ComputeStockAsync(_db);
        ViewBag.Products = await _db.Products.Include(p => p.UnitOfMeasure)
            .Where(p => p.Status == Status.Active).OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, Unit = p.UnitOfMeasure!.Code, p.SalesPrice })
            .ToListAsync();
        ViewBag.Stock = stock.ToDictionary(s => s.ProductId, s => s.Quantity);
    }

    [HttpGet] public async Task<IActionResult> Create() { await LoadLookupsAsync(); return View(new SaleVm()); }

    [HttpPost]
    public async Task<IActionResult> Create(SaleVm vm)
    {
        if (vm.Items == null || vm.Items.Count == 0)
        { ModelState.AddModelError("", "Add at least one item"); await LoadLookupsAsync(); return View(vm); }

        // Validate stock
        var stock = (await StockController.ComputeStockAsync(_db)).ToDictionary(s => s.ProductId, s => s.Quantity);
        foreach (var i in vm.Items.Where(i => i.ProductId > 0))
        {
            var available = stock.TryGetValue(i.ProductId, out var q) ? q : 0;
            if (i.Quantity > available)
            { ModelState.AddModelError("", $"Insufficient stock for product #{i.ProductId} (available {available})");
              await LoadLookupsAsync(); return View(vm); }
        }

        var sale = new Sale
        {
            CustomerId = vm.CustomerId == 0 ? null : vm.CustomerId,
            UserId = _cu.Id, Date = DateTime.UtcNow
        };
        foreach (var i in vm.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
        {
            sale.Items.Add(new SaleItem
            {
                ProductId = i.ProductId, Quantity = i.Quantity,
                Price = i.Price, LineTotal = i.Quantity * i.Price
            });
        }
        sale.TotalAmount = sale.Items.Sum(x => x.LineTotal);
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = sale.Id });
    }
}

[Authorize]
public class StockController : Controller
{
    private readonly AppDbContext _db;
    public StockController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index() => View(await ComputeStockAsync(_db));

    public record StockRow(int ProductId, string ProductName, string Unit,
                           decimal Quantity, decimal AverageRate, decimal TotalAmount);

    public static async Task<List<StockRow>> ComputeStockAsync(AppDbContext db)
    {
        var products = await db.Products.Include(p => p.UnitOfMeasure).ToListAsync();
        var purchases = await db.PurchaseItems.GroupBy(p => p.ProductId)
            .Select(g => new {
                ProductId = g.Key,
                Qty = g.Sum(x => x.Quantity),
                Amt = g.Sum(x => x.Quantity * x.PurchasePrice)
            }).ToListAsync();
        var sales = await db.SaleItems.GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) }).ToListAsync();

        var rows = new List<StockRow>();
        foreach (var p in products)
        {
            var pu = purchases.FirstOrDefault(x => x.ProductId == p.Id);
            var sa = sales.FirstOrDefault(x => x.ProductId == p.Id);
            decimal pQty = pu?.Qty ?? 0; decimal pAmt = pu?.Amt ?? 0;
            decimal sQty = sa?.Qty ?? 0;
            decimal qty = pQty - sQty;
            decimal avg = pQty > 0 ? Math.Round(pAmt / pQty, 2) : 0;
            // Total = remaining qty * avg rate (matches average-rate accounting)
            decimal total = Math.Round(qty * avg, 2);
            rows.Add(new StockRow(p.Id, p.Name, p.UnitOfMeasure?.Code ?? "", qty, avg, total));
        }
        return rows.OrderBy(r => r.ProductName).ToList();
    }
}

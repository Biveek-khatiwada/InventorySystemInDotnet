using Inventory.Data;
using Inventory.Models;
using Inventory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index() => View(await _db.Users.OrderBy(u => u.Name).ToListAsync());

    [HttpGet] public IActionResult Create() => View(new User());

    [HttpPost]
    public async Task<IActionResult> Create(User m, string password)
    {
        if (!ModelState.IsValid) return View(m);
        m.PasswordHash = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrWhiteSpace(password) ? "changeme" : password);
        _db.Users.Add(m); await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet] public async Task<IActionResult> Edit(int id) => View(await _db.Users.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Edit(int id, User m, string? password)
    {
        var u = await _db.Users.FindAsync(id); if (u == null) return NotFound();
        u.Name = m.Name; u.Email = m.Email; u.MobileNo = m.MobileNo; u.Status = m.Status;
        if (!string.IsNullOrWhiteSpace(password)) u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

[Authorize]
public class ProductGroupsController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public ProductGroupsController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }

    public async Task<IActionResult> Index() => View(await _db.ProductGroups.Include(g => g.User).OrderBy(g => g.Name).ToListAsync());
    [HttpGet] public IActionResult Create() => View(new ProductGroup());
    [HttpPost] public async Task<IActionResult> Create(ProductGroup m)
    { if (!ModelState.IsValid) return View(m); m.UserId = _cu.Id; _db.Add(m); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) => View(await _db.ProductGroups.FindAsync(id));
    [HttpPost] public async Task<IActionResult> Edit(int id, ProductGroup m)
    { var e = await _db.ProductGroups.FindAsync(id); if (e == null) return NotFound();
      e.Name = m.Name; e.Description = m.Description; e.Status = m.Status; await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

[Authorize]
public class UnitsController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public UnitsController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }
    public async Task<IActionResult> Index() => View(await _db.UnitOfMeasures.Include(u => u.User).OrderBy(u => u.Name).ToListAsync());
    [HttpGet] public IActionResult Create() => View(new UnitOfMeasure());
    [HttpPost] public async Task<IActionResult> Create(UnitOfMeasure m)
    { if (!ModelState.IsValid) return View(m); m.UserId = _cu.Id; _db.Add(m); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) => View(await _db.UnitOfMeasures.FindAsync(id));
    [HttpPost] public async Task<IActionResult> Edit(int id, UnitOfMeasure m)
    { var e = await _db.UnitOfMeasures.FindAsync(id); if (e == null) return NotFound();
      e.Name = m.Name; e.Code = m.Code; e.Description = m.Description; e.Status = m.Status;
      await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

[Authorize]
public class ProductsController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public ProductsController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }

    public async Task<IActionResult> Index() =>
        View(await _db.Products.Include(p => p.UnitOfMeasure).Include(p => p.ProductGroup).Include(p => p.User).OrderBy(p => p.Name).ToListAsync());

    private async Task LoadLookupsAsync()
    {
        ViewBag.Units = await _db.UnitOfMeasures.Where(u => u.Status == Status.Active).OrderBy(u => u.Name).ToListAsync();
        ViewBag.Groups = await _db.ProductGroups.Where(g => g.Status == Status.Active).OrderBy(g => g.Name).ToListAsync();
    }

    [HttpGet] public async Task<IActionResult> Create() { await LoadLookupsAsync(); return View(new Product()); }
    [HttpPost] public async Task<IActionResult> Create(Product m)
    { if (!ModelState.IsValid) { await LoadLookupsAsync(); return View(m); }
      m.UserId = _cu.Id; _db.Add(m); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }

    [HttpGet] public async Task<IActionResult> Edit(int id) { await LoadLookupsAsync(); return View(await _db.Products.FindAsync(id)); }
    [HttpPost] public async Task<IActionResult> Edit(int id, Product m)
    { var e = await _db.Products.FindAsync(id); if (e == null) return NotFound();
      e.Name = m.Name; e.Description = m.Description; e.UnitOfMeasureId = m.UnitOfMeasureId;
      e.ProductGroupId = m.ProductGroupId; e.SalesPrice = m.SalesPrice; e.Status = m.Status;
      await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

[Authorize]
public class VendorsController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public VendorsController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }
    public async Task<IActionResult> Index() => View(await _db.Vendors.Include(v => v.User).OrderBy(v => v.Name).ToListAsync());
    [HttpGet] public IActionResult Create() => View(new Vendor());
    [HttpPost] public async Task<IActionResult> Create(Vendor m)
    { if (!ModelState.IsValid) return View(m); m.UserId = _cu.Id; _db.Add(m); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) => View(await _db.Vendors.FindAsync(id));
    [HttpPost] public async Task<IActionResult> Edit(int id, Vendor m)
    { var e = await _db.Vendors.FindAsync(id); if (e == null) return NotFound();
      e.Name = m.Name; e.Description = m.Description; e.Status = m.Status; await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

[Authorize]
public class CustomersController : Controller
{
    private readonly AppDbContext _db; private readonly ICurrentUser _cu;
    public CustomersController(AppDbContext db, ICurrentUser cu) { _db = db; _cu = cu; }
    public async Task<IActionResult> Index() => View(await _db.Customers.Include(c => c.User).OrderBy(c => c.Name).ToListAsync());
    [HttpGet] public IActionResult Create() => View(new Customer());
    [HttpPost] public async Task<IActionResult> Create(Customer m)
    { if (!ModelState.IsValid) return View(m); m.UserId = _cu.Id; _db.Add(m); await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
    [HttpGet] public async Task<IActionResult> Edit(int id) => View(await _db.Customers.FindAsync(id));
    [HttpPost] public async Task<IActionResult> Edit(int id, Customer m)
    { var e = await _db.Customers.FindAsync(id); if (e == null) return NotFound();
      e.Name = m.Name; e.Description = m.Description; e.Status = m.Status; await _db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}

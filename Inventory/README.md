# Inventory Management System

ASP.NET Core 8 (MVC) + Entity Framework Core + MySQL (Pomelo provider).
Server-rendered Razor views with plain HTML/CSS/JavaScript.

## Features

- **Users**: CRUD, Active/Inactive (inactive cannot login), BCrypt password hashing, cookie auth.
- **Product Groups, Units of Measure, Products**: full CRUD with creator user tracking.
- **Vendors / Customers**: full CRUD.
- **Purchases**: select vendor, multiple line items in one entry, set purchase price + sales price per line. Sales price updates the product master immediately. Entries are read-only after save.
- **Sales**: optional customer (or walk-in), multiple line items, editable price, stock validation. Read-only after save.
- **Current Stock**: per product — quantity (purchases − sales), weighted-average purchase rate, accurate total amount = remaining qty × avg rate.

## Prerequisites

- .NET 8 SDK
- MySQL 8.x running locally (or update connection string)

## Setup

1. Create the database (or let EF do it):
   ```sql
   CREATE DATABASE inventory_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

2. Edit `appsettings.json` → `ConnectionStrings:Default` with your MySQL credentials.

3. Restore packages and create the schema:
   ```bash
   dotnet restore
   dotnet tool install --global dotnet-ef    # if not installed
   dotnet ef migrations add Initial
   dotnet ef database update
   ```
   (Or just run the app — `db.Database.Migrate()` applies migrations on startup once you've created the initial migration.)

4. Run:
   ```bash
   dotnet run
   ```
   Open http://localhost:5080

## Default login

- Email: `admin@inventory.local`
- Password: `admin123`

(Seeded automatically on first run.)

## Project layout

```
Inventory/
├── Program.cs               # DI, auth, EF, routing
├── appsettings.json         # MySQL connection string
├── Models/Entities.cs       # User, Product, Purchase, Sale, etc.
├── Data/AppDbContext.cs     # EF Core context + seeder
├── Services/CurrentUser.cs  # Resolves logged-in user id from claims
├── ViewModels/Vms.cs        # Purchase / Sale form view-models
├── Controllers/
│   ├── AccountController.cs        # login / logout
│   ├── MasterControllers.cs        # Users, Groups, Units, Products, Vendors, Customers
│   └── TransactionControllers.cs   # Purchases, Sales, Stock
├── Views/                    # Razor (.cshtml)
└── wwwroot/
    ├── css/site.css
    └── js/site.js            # Line-items add/remove + totals
```

## Notes

- Purchases and Sales are append-only (no edit/delete actions are exposed) — matches the spec.
- The Stock screen computes everything on the fly from PurchaseItems / SaleItems; no separate stock table is required, so quantities are always consistent.
- To reset the schema during development: `dotnet ef database drop -f && dotnet ef database update`.

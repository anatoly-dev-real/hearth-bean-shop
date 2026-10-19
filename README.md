# Hearth & Bean

A bilingual portfolio bakery shop built with ASP.NET Core MVC, .NET 10, EF Core and SQL Server. The interface starts in English; use the EN / RU switch in the header to change the language. All customer, order and payment data is fictional. There is no real payment processing.

## Features

- Responsive bakery storefront, product details, search, category filters, sorting and pagination.
- Guest cart stored in session; customer cart stored in SQL Server and merged on sign-in.
- ASP.NET Core Identity registration, sign-in, sign-out, Customer and Admin roles.
- Checkout validates phone, delivery address and available stock. Prices are read from the database, never from the form.
- Order history is scoped to the signed-in user. Customers can cancel their own Pending orders; inventory is restored for valid cancellations.
- Admin dashboard, product/category visibility, order workflow, customer list and three reports.
- English/Russian UI selection persisted in a culture cookie.
- Local image assets, keyboard-visible focus styles and responsive layouts (breakpoints at 1050, 767 and 380 px).

## Technology

- .NET 10 SDK, ASP.NET Core MVC, Razor
- Entity Framework Core 10, ASP.NET Core Identity
- SQL Server Express / Developer / LocalDB
- xUnit

## Run locally

Prerequisites: .NET 10 SDK and SQL Server. The checked-in connection string targets `SQLEXPRESS01`; change `ConnectionStrings:DefaultConnection` in `src/HearthBean.Web/appsettings.json` if your SQL Server instance has another name.

```powershell
dotnet tool restore
dotnet restore HearthBean.slnx
dotnet user-secrets set "DemoAccounts:CustomerPassword" "BakerDemo2026!" --project src/HearthBean.Web
dotnet user-secrets set "DemoAccounts:AdminEmail" "admin@example.com" --project src/HearthBean.Web
dotnet user-secrets set "DemoAccounts:AdminPassword" "HearthAdmin2026!" --project src/HearthBean.Web
dotnet tool run dotnet-ef database update --project src/HearthBean.Web
dotnet run --project src/HearthBean.Web
```

Open the HTTPS URL printed by `dotnet run`. On first app start, the idempotent `DataSeeder` adds five categories, 30 products, 12 customers and 40 demo orders, then applies the SQL objects. No demo password is stored in the code or in the configuration files of this repository: you set the passwords yourself with `dotnet user-secrets` (commands above).

Demo accounts (demonstration only): `admin@example.com` / `HearthAdmin2026!`; `customer01@example.com` / `BakerDemo2026!`. These values are examples for a local portfolio database; do not reuse them elsewhere.

## Database

The EF model is configured in `src/HearthBean.Web/Data/AppDbContext.cs`; the generated initial migration is under `src/HearthBean.Web/Data/Migrations`. `database/schema.sql` is generated from the migrations. `database/objects.sql` defines `vw_OrderSummary`, `vw_TopProducts`, `sp_PlaceOrder` and `sp_MonthlyRevenue`. `sp_PlaceOrder` locks cart/product rows, verifies stock, snapshots unit prices, creates the order and items, decrements stock, empties the cart, and rolls the entire transaction back with `THROW` on failure.

Tables are in 3NF around Identity users, categories, products, carts, orders and order items. `OrderItems.UnitPrice` intentionally duplicates the current product price: it is the immutable price snapshot at purchase time, so later catalog edits cannot rewrite order history. Unique slugs, unique `(UserId, ProductId)` cart rows, price/quantity/stock checks, and the requested product/category and order/customer/date indexes are configured in EF.

The ER diagram is available as [`database/er-diagram.png`](database/er-diagram.png), with editable Mermaid source in [`database/er-diagram.mmd`](database/er-diagram.mmd). Ten commented reporting queries (JOIN, GROUP BY, HAVING, subqueries and window functions) are in [`database/queries.sql`](database/queries.sql). `database/seed.sql` documents the idempotent category seed; the complete demo catalog and Identity users are created by the C# seeder.

## Tests

Run `dotnet test HearthBean.slnx`.

- **Unit tests** (`DomainRulesTests`, 6 test methods / 13 cases, no database needed): cart quantity limits, order totals, stock restoration on cancellation, valid order-status transitions and the "customer may cancel only their own Pending order" rule.
- **Integration tests** (`StoredProcedureTests`, 3 tests) run `dbo.sp_PlaceOrder` against a real SQL Server: a successful order creates the order and items, snapshots prices, decrements stock and empties the cart; insufficient stock rolls the whole transaction back; an empty cart is rejected. Each test creates and drops its own temporary database from the EF migrations plus `database/objects.sql`. They are opt-in and are reported as *skipped* unless a connection string is provided (the login must be allowed to create and drop databases):

```powershell
$env:HEARTHBEAN_TEST_SQL = "Server=.\SQLEXPRESS01;Trusted_Connection=True;TrustServerCertificate=True"
dotnet test HearthBean.slnx
```

Order ownership and admin authorization are enforced in the controllers and queries and are not covered by automated tests yet.

## Screenshots and video

Screenshots show the desktop layout with demo data. A shot-by-shot script for the demo video is in [`docs/demo-video-script.md`](docs/demo-video-script.md); the finished video link belongs in [`docs/demo-video-link.md`](docs/demo-video-link.md).

| | |
| --- | --- |
| <a href="docs/screenshots/01-home.jpg"><img src="docs/screenshots/01-home.jpg" width="420" alt="Home page"></a><br>**Home**: hero, navigation, EN / RU switch | <a href="docs/screenshots/02-menu.jpg"><img src="docs/screenshots/02-menu.jpg" width="420" alt="Menu page"></a><br>**Menu**: search, category filter, sorting, product cards |
| <a href="docs/screenshots/03-product.jpg"><img src="docs/screenshots/03-product.jpg" width="420" alt="Product page"></a><br>**Product**: details, stock, quantity | <a href="docs/screenshots/04-cart.jpg"><img src="docs/screenshots/04-cart.jpg" width="420" alt="Shopping bag"></a><br>**Bag**: lines, quantities, subtotal |
| <a href="docs/screenshots/05-checkout.jpg"><img src="docs/screenshots/05-checkout.jpg" width="420" alt="Checkout page"></a><br>**Checkout**: pickup or delivery, validation, demo notice | <a href="docs/screenshots/06-order-history.jpg"><img src="docs/screenshots/06-order-history.jpg" width="420" alt="Order history"></a><br>**Order history**: own orders, cancelling a Pending order |
| <a href="docs/screenshots/07-admin-overview.jpg"><img src="docs/screenshots/07-admin-overview.jpg" width="420" alt="Admin overview"></a><br>**Admin overview**: key figures for the shop | <a href="docs/screenshots/08-admin-reports.jpg"><img src="docs/screenshots/08-admin-reports.jpg" width="420" alt="Admin reports"></a><br>**Admin reports**: monthly revenue and top products |

## Security and limitations

- Password hashing and role checks use ASP.NET Core Identity. Admin routes require the Admin role; state-changing forms use anti-forgery tokens, and a global `AutoValidateAntiforgeryToken` filter acts as a safety net for new actions.
- Customer order queries include the authenticated `UserId`; IDs are not trusted as authorization.
- EF queries are parameterized. The stored procedure is called through ADO.NET (`DbCommand` with explicit, typed parameters), never with string concatenation.
- Demo secrets belong in user-secrets, not appsettings or source control. No real payments, email delivery or production hosting are configured. The contact form is a validated UI mock.
- The image files are local assets; see [`docs/ASSETS.md`](docs/ASSETS.md) for source notes. Every product has its own original illustration in `wwwroot/images/products/<slug>.jpg` (see [`docs/PRODUCT_PHOTOS.md`](docs/PRODUCT_PHOTOS.md)); a product without a file falls back to one photo per category. The illustrations are stylised, not real product photos.
- The stylesheet is written desktop-first (`max-width` breakpoints).
- Automated tests do not cover controller authorization yet (see Tests).

## About this project

This is an educational portfolio project for a fictional bakery. AI assistance was used while building and reviewing parts of the code. All names, addresses, accounts and order history are synthetic.
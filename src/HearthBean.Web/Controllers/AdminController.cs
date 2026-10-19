using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Models.ViewModels;
using HearthBean.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("Admin")]
public sealed class AdminController(AppDbContext db, OrderService orders) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new AdminDashboardViewModel
        {
            ProductCount = await db.Products.CountAsync(product => product.IsAvailable, cancellationToken),
            OrderCount = await db.Orders.CountAsync(cancellationToken),
            CustomerCount = await db.UserRoles.Join(db.Roles, link => link.RoleId, role => role.Id, (link, role) => new { link.UserId, role.Name })
                .Where(item => item.Name == "Customer").Select(item => item.UserId).Distinct().CountAsync(cancellationToken),
            PendingRevenue = await db.Orders.Where(order => order.Status != OrderStatuses.Cancelled)
                .SumAsync(order => (decimal?)order.TotalAmount, cancellationToken) ?? 0,
            RecentOrders = await db.Orders.Include(order => order.User).OrderByDescending(order => order.OrderDate).Take(8).ToListAsync(cancellationToken)
        };
        return View(model);
    }

    [HttpGet("Products")]
    public async Task<IActionResult> Products(string? search, CancellationToken cancellationToken)
    {
        var query = db.Products.Include(product => product.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product => product.Name.Contains(search) || product.Slug.Contains(search));
        }

        ViewData["Search"] = search;
        return View(await query.OrderBy(product => product.Name).ToListAsync(cancellationToken));
    }

    [HttpGet("Products/New")]
    public async Task<IActionResult> NewProduct(CancellationToken cancellationToken) => View("ProductForm", await BuildProductModelAsync(null, cancellationToken));

    [HttpGet("Products/{id:int}/Edit")]
    public async Task<IActionResult> EditProduct(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        return product is null ? NotFound() : View("ProductForm", await BuildProductModelAsync(product, cancellationToken));
    }

    [HttpPost("Products/Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct(ProductEditViewModel model, CancellationToken cancellationToken)
    {
        model.Categories = await db.Categories.Where(category => category.IsAvailable).OrderBy(category => category.SortOrder).ToListAsync(cancellationToken);
        if (!ModelState.IsValid || !await db.Categories.AnyAsync(category => category.Id == model.CategoryId && category.IsAvailable, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Choose an available category.");
            return View("ProductForm", model);
        }

        var existing = model.Id == 0 ? null : await db.Products.FindAsync([model.Id], cancellationToken);
        if (model.Id != 0 && existing is null)
        {
            return NotFound();
        }

        var duplicateSlug = await db.Products.AnyAsync(product => product.Slug == model.Slug && product.Id != model.Id, cancellationToken);
        if (duplicateSlug)
        {
            ModelState.AddModelError(nameof(model.Slug), "This slug is already in use.");
            return View("ProductForm", model);
        }

        var product = existing ?? new Product();
        product.Name = model.Name.Trim();
        product.Slug = model.Slug.Trim();
        product.Description = model.Description.Trim();
        product.Price = model.Price;
        product.StockQuantity = model.StockQuantity;
        product.CategoryId = model.CategoryId;
        product.ImageFileName = Path.GetFileName(model.ImageFileName);
        product.IsVegan = model.IsVegan;
        product.IsAvailable = true;
        if (existing is null)
        {
            db.Products.Add(product);
        }

        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("Products/{id:int}/Hide")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HideProduct(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.IsAvailable = false;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken) =>
        View(await db.Categories.OrderBy(category => category.SortOrder).ToListAsync(cancellationToken));

    [HttpGet("Categories/New")]
    public IActionResult NewCategory() => View("CategoryForm", new CategoryEditViewModel());

    [HttpGet("Categories/{id:int}/Edit")]
    public async Task<IActionResult> EditCategory(int id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        return category is null ? NotFound() : View("CategoryForm", new CategoryEditViewModel
        {
            Id = category.Id, Name = category.Name, Slug = category.Slug, SortOrder = category.SortOrder
        });
    }

    [HttpPost("Categories/Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryEditViewModel model, CancellationToken cancellationToken)
    {
        if (await db.Categories.AnyAsync(category => category.Slug == model.Slug && category.Id != model.Id, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Slug), "This slug is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View("CategoryForm", model);
        }

        var category = model.Id == 0 ? new Category() : await db.Categories.FindAsync([model.Id], cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = model.Name.Trim();
        category.Slug = model.Slug.Trim();
        category.SortOrder = model.SortOrder;
        category.IsAvailable = true;
        if (model.Id == 0)
        {
            db.Categories.Add(category);
        }

        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("Categories/{id:int}/Toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategory(int id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        category.IsAvailable = !category.IsAvailable;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("Orders")]
    public async Task<IActionResult> Orders(string? status, CancellationToken cancellationToken)
    {
        var query = db.Orders.Include(order => order.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && OrderStatuses.All.Contains(status, StringComparer.Ordinal))
        {
            query = query.Where(order => order.Status == status);
        }

        ViewData["StatusFilter"] = status;
        return View(await query.OrderByDescending(order => order.OrderDate).Take(100).ToListAsync(cancellationToken));
    }

    [HttpPost("Orders/{id:int}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetOrderStatus(int id, string status, CancellationToken cancellationToken)
    {
        var changed = await orders.SetStatusAsync(id, status, cancellationToken);
        TempData[changed ? "Notice" : "Error"] = changed ? "Order status updated." : "That status transition is not allowed.";
        return RedirectToAction(nameof(Orders));
    }

    [HttpGet("Customers")]
    public async Task<IActionResult> Customers(CancellationToken cancellationToken)
    {
        var customerIds = db.UserRoles.Join(db.Roles, link => link.RoleId, role => role.Id, (link, role) => new { link.UserId, role.Name })
            .Where(item => item.Name == "Customer").Select(item => item.UserId);
        return View(await db.Users.Where(user => customerIds.Contains(user.Id)).OrderBy(user => user.FullName).ToListAsync(cancellationToken));
    }

    [HttpGet("Reports")]
    public async Task<IActionResult> Reports(CancellationToken cancellationToken)
    {
        var revenueRows = await db.Orders.Where(order => order.Status != OrderStatuses.Cancelled)
            .GroupBy(order => new { order.OrderDate.Year, order.OrderDate.Month })
            .Select(group => new { group.Key.Year, group.Key.Month, Revenue = group.Sum(order => order.TotalAmount), Orders = group.Count() })
            .OrderByDescending(item => item.Year).ThenByDescending(item => item.Month).Take(12).ToListAsync(cancellationToken);
        var revenue = revenueRows.Select(item => new RevenueReportRow(item.Year, item.Month, item.Revenue, item.Orders)).ToList();
        var topProductRows = await db.OrderItems.Where(item => item.Order.Status != OrderStatuses.Cancelled)
            .GroupBy(item => item.Product.Name)
            .Select(group => new { Name = group.Key, Quantity = group.Sum(item => item.Quantity), Revenue = group.Sum(item => item.UnitPrice * item.Quantity) })
            .OrderByDescending(item => item.Quantity).Take(10).ToListAsync(cancellationToken);
        var topProducts = topProductRows.Select(item => new TopProductReportRow(item.Name, item.Quantity, item.Revenue)).ToList();
        var lowStock = await db.Products.Include(product => product.Category).Where(product => product.IsAvailable && product.StockQuantity <= 5)
            .OrderBy(product => product.StockQuantity).ToListAsync(cancellationToken);
        return View(new ReportsViewModel { Revenue = revenue, TopProducts = topProducts, LowStockProducts = lowStock });
    }

    private async Task<ProductEditViewModel> BuildProductModelAsync(Product? product, CancellationToken cancellationToken) => new()
    {
        Id = product?.Id ?? 0,
        Name = product?.Name ?? string.Empty,
        Slug = product?.Slug ?? string.Empty,
        Description = product?.Description ?? string.Empty,
        Price = product?.Price ?? 0,
        StockQuantity = product?.StockQuantity ?? 0,
        CategoryId = product?.CategoryId ?? 0,
        ImageFileName = product?.ImageFileName ?? "bread.jpg",
        IsVegan = product?.IsVegan ?? false,
        Categories = await db.Categories.Where(category => category.IsAvailable).OrderBy(category => category.SortOrder).ToListAsync(cancellationToken)
    };
}
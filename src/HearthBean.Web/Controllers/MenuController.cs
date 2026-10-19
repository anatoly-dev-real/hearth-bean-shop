using HearthBean.Web.Data;
using HearthBean.Web.Localization;
using HearthBean.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Controllers;

[Route("Menu")]
public sealed class MenuController(AppDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int? categoryId, string sort = "featured", int page = 1, CancellationToken cancellationToken = default)
    {
        var categories = await db.Categories.Where(category => category.IsAvailable).OrderBy(category => category.SortOrder).ToListAsync(cancellationToken);
        var query = db.Products.Include(product => product.Category)
            .Where(product => product.IsAvailable && product.Category.IsAvailable);
        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        var products = await query.ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products.Where(product => product.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || product.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                || product.Slug.Contains(search, StringComparison.OrdinalIgnoreCase)
                || CatalogText.Name(product).Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        products = sort switch
        {
            "price-low" => products.OrderBy(product => product.Price).ToList(),
            "price-high" => products.OrderByDescending(product => product.Price).ToList(),
            "newest" => products.OrderByDescending(product => product.CreatedAt).ToList(),
            _ => products.OrderBy(product => product.Category.SortOrder).ThenBy(product => product.Name).ToList()
        };

        const int pageSize = 9;
        var pageCount = (int)Math.Ceiling(products.Count / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(pageCount, 1));
        var cards = products.Skip((page - 1) * pageSize).Take(pageSize).Select(product => new ProductCardViewModel(
            product, CatalogText.Name(product), CatalogText.Description(product), CatalogText.CategoryName(product.Category))).ToList();

        return View(new MenuViewModel
        {
            Products = cards, Categories = categories, Search = search, CategoryId = categoryId,
            Sort = sort, Page = page, PageCount = pageCount, TotalCount = products.Count
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var product = await db.Products.Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Slug == slug && item.IsAvailable && item.Category.IsAvailable, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return View(new ProductCardViewModel(product, CatalogText.Name(product), CatalogText.Description(product), CatalogText.CategoryName(product.Category)));
    }
}
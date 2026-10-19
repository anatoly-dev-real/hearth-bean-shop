using System.Diagnostics;
using HearthBean.Web.Data;
using HearthBean.Web.Localization;
using HearthBean.Web.Models;
using HearthBean.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Controllers;

public sealed class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await db.Products.Include(product => product.Category)
            .Where(product => product.IsAvailable && product.Category.IsAvailable && product.StockQuantity > 0)
            .OrderByDescending(product => product.CreatedAt).Take(4).ToListAsync(cancellationToken);
        return View(new HomeViewModel
        {
            FeaturedProducts = products.Select(product => new ProductCardViewModel(
                product, CatalogText.Name(product), CatalogText.Description(product), CatalogText.CategoryName(product.Category))).ToList()
        });
    }

    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Contact() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email) || string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError(string.Empty, "Please complete all fields with a valid email address.");
            return View();
        }

        TempData["Notice"] = "Thank you. This demo form does not send email.";
        return RedirectToAction(nameof(Contact));
    }

    [IgnoreAntiforgeryToken]
    public IActionResult Forbidden() => StatusCode(403);

    [IgnoreAntiforgeryToken]
    public IActionResult StatusCodePage(int code)
    {
        Response.StatusCode = code;
        var page = code switch { 403 => "403", 404 => "404", _ => "500" };
        return View($"~/Views/Errors/{page}.cshtml");
    }

    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View("~/Views/Errors/500.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

using HearthBean.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HearthBean.Web.Controllers;

[Route("Cart")]
public sealed class CartController(CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await cart.GetAsync(User, HttpContext.Session, cancellationToken));

    [HttpPost("Add/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var error = await cart.AddAsync(User, HttpContext.Session, productId, quantity, cancellationToken);
        if (error is not null)
        {
            TempData["Error"] = error;
        }
        else
        {
            TempData["Notice"] = "Added to your bag.";
        }

        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost("Update/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity, CancellationToken cancellationToken)
    {
        var error = await cart.SetQuantityAsync(User, HttpContext.Session, productId, quantity, cancellationToken);
        if (error is not null)
        {
            TempData["Error"] = error;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Remove/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        await cart.RemoveAsync(User, HttpContext.Session, productId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
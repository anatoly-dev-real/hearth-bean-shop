using System.Security.Claims;
using HearthBean.Web.Data;
using HearthBean.Web.Models.ViewModels;
using HearthBean.Web.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Controllers;

[Authorize(Roles = "Customer")]
[Route("Checkout")]
public sealed class CheckoutController(AppDbContext db, CartService cart, OrderService orders) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await cart.GetAsync(User, HttpContext.Session, cancellationToken);
        if (model.Lines.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await db.Users.FindAsync([userId], cancellationToken);
        return View(new CheckoutPageViewModel
        {
            Cart = model,
            Checkout = new CheckoutViewModel { Phone = user?.PhoneNumber ?? string.Empty, DeliveryAddress = user?.Address }
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutPageViewModel page, CancellationToken cancellationToken)
    {
        if (page.Checkout.DeliveryMethod is not ("Pickup" or "Delivery"))
        {
            ModelState.AddModelError("Checkout.DeliveryMethod", "Choose pickup or delivery.");
        }

        if (page.Checkout.DeliveryMethod == "Delivery" && string.IsNullOrWhiteSpace(page.Checkout.DeliveryAddress))
        {
            ModelState.AddModelError("Checkout.DeliveryAddress", "A delivery address is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await db.Users.FindAsync([userId], cancellationToken);
        var cartModel = await cart.GetAsync(User, HttpContext.Session, cancellationToken);
        if (cartModel.Lines.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        page.Cart = cartModel;
        if (!ModelState.IsValid)
        {
            return View(page);
        }

        user!.PhoneNumber = page.Checkout.Phone;
        if (page.Checkout.DeliveryMethod == "Delivery")
        {
            user.Address = page.Checkout.DeliveryAddress;
        }

        await db.SaveChangesAsync(cancellationToken);
        try
        {
            var orderId = await orders.PlaceAsync(userId, page.Checkout, cancellationToken);
            cart.ClearGuestCart(HttpContext.Session);
            TempData["OrderId"] = orderId;
            return RedirectToAction(nameof(Complete));
        }
        catch (Exception exception) when (exception is DbUpdateException or SqlException)
        {
            ModelState.AddModelError(string.Empty, "A product sold out while you were checking out. Please review your bag.");
            HttpContext.RequestServices.GetRequiredService<ILogger<CheckoutController>>()
                .LogWarning(exception, "Order creation failed for customer {UserId}", userId);
            page.Cart = await cart.GetAsync(User, HttpContext.Session, cancellationToken);
            return View(page);
        }
    }

    [HttpGet("Complete")]
    public IActionResult Complete() => View();
}
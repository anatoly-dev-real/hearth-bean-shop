using System.Security.Claims;
using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Controllers;

[Authorize(Roles = "Customer")]
[Route("Orders")]
public sealed class OrdersController(AppDbContext db, OrderService orders) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var items = await db.Orders.Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate).ToListAsync(cancellationToken);
        return View(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await db.Orders.Include(item => item.Items).ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost("{id:int}/Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cancelled = await orders.CancelByCustomerAsync(id, userId, cancellationToken);
        TempData[cancelled ? "Notice" : "Error"] = cancelled
            ? "Order cancelled. Available stock has been restored."
            : "This order cannot be cancelled.";
        return RedirectToAction(nameof(Index));
    }
}
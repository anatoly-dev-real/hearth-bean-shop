using System.Security.Claims;
using System.Text.Json;
using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Localization;
using HearthBean.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Services;

public sealed class CartService(AppDbContext db)
{
    private const string SessionKey = "guest-cart";

    public async Task<CartViewModel> GetAsync(ClaimsPrincipal principal, ISession session, CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var items = await db.CartItems.Include(item => item.Product)
                .Where(item => item.UserId == userId && item.Product.IsAvailable && item.Product.Category.IsAvailable)
                .ToListAsync(cancellationToken);
            return Build(items.Select(item => (item.Product, item.Quantity)));
        }

        var guestItems = ReadGuestCart(session);
        var ids = guestItems.Keys.ToArray();
        var products = await db.Products.Where(product => ids.Contains(product.Id) && product.IsAvailable && product.Category.IsAvailable)
            .ToListAsync(cancellationToken);
        return Build(products.Select(product => (product, guestItems[product.Id])));
    }

    public async Task<string?> AddAsync(ClaimsPrincipal principal, ISession session, int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == productId && item.IsAvailable && item.Category.IsAvailable, cancellationToken);
        if (product is null)
        {
            return "ProductNotAvailable";
        }

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await db.CartItems.SingleOrDefaultAsync(row => row.UserId == userId && row.ProductId == productId, cancellationToken);
            var nextQuantity = (item?.Quantity ?? 0) + quantity;
            if (!DomainRules.IsValidCartQuantity(nextQuantity, product.StockQuantity))
            {
                return "CartLimit";
            }

            if (item is null)
            {
                db.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = nextQuantity });
            }
            else
            {
                item.Quantity = nextQuantity;
            }

            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var guest = ReadGuestCart(session);
        var guestQuantity = guest.GetValueOrDefault(productId) + quantity;
        if (!DomainRules.IsValidCartQuantity(guestQuantity, product.StockQuantity))
        {
            return "CartLimit";
        }

        guest[productId] = guestQuantity;
        WriteGuestCart(session, guest);
        return null;
    }

    public async Task<string?> SetQuantityAsync(ClaimsPrincipal principal, ISession session, int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == productId && item.IsAvailable && item.Category.IsAvailable, cancellationToken);
        if (product is null || !DomainRules.IsValidCartQuantity(quantity, product.StockQuantity))
        {
            return "CartLimit";
        }

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await db.CartItems.SingleOrDefaultAsync(row => row.UserId == userId && row.ProductId == productId, cancellationToken);
            if (item is null)
            {
                return "ProductNotAvailable";
            }

            item.Quantity = quantity;
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var guest = ReadGuestCart(session);
            if (!guest.ContainsKey(productId))
            {
                return "ProductNotAvailable";
            }

            guest[productId] = quantity;
            WriteGuestCart(session, guest);
        }

        return null;
    }

    public async Task RemoveAsync(ClaimsPrincipal principal, ISession session, int productId, CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated == true)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await db.CartItems.Where(item => item.UserId == userId && item.ProductId == productId)
                .ExecuteDeleteAsync(cancellationToken);
        }
        else
        {
            var guest = ReadGuestCart(session);
            guest.Remove(productId);
            WriteGuestCart(session, guest);
        }
    }

    public async Task MergeGuestCartAsync(string userId, ISession session, CancellationToken cancellationToken = default)
    {
        var guest = ReadGuestCart(session);
        if (guest.Count == 0)
        {
            return;
        }

        foreach (var (productId, quantity) in guest)
        {
            var product = await db.Products.SingleOrDefaultAsync(item => item.Id == productId && item.IsAvailable, cancellationToken);
            if (product is null || product.StockQuantity == 0)
            {
                continue;
            }

            var item = await db.CartItems.SingleOrDefaultAsync(row => row.UserId == userId && row.ProductId == productId, cancellationToken);
            var merged = Math.Min(item?.Quantity + quantity ?? quantity, Math.Min(20, product.StockQuantity));
            if (item is null)
            {
                db.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = merged });
            }
            else
            {
                item.Quantity = merged;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        session.Remove(SessionKey);
    }

    public void ClearGuestCart(ISession session) => session.Remove(SessionKey);

    private static CartViewModel Build(IEnumerable<(Product Product, int Quantity)> items)
    {
        var lines = items.Select(item => new CartLineViewModel(
            item.Product.Id, item.Product.Slug, CatalogText.Name(item.Product), item.Product.ImageFileName,
            item.Product.Price, item.Quantity, item.Product.StockQuantity)).ToList();
        return new CartViewModel { Lines = lines };
    }

    private static Dictionary<int, int> ReadGuestCart(ISession session)
    {
        var json = session.GetString(SessionKey);
        return json is null ? [] : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? [];
    }

    private static void WriteGuestCart(ISession session, Dictionary<int, int> items) =>
        session.SetString(SessionKey, JsonSerializer.Serialize(items));
}
using System.Text.RegularExpressions;
using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Models.ViewModels;
using HearthBean.Web.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HearthBean.Tests;

/// <summary>
/// Integration tests for dbo.sp_PlaceOrder against a real SQL Server.
/// Every test gets its own throw-away database (created from the EF migrations, then
/// database/objects.sql is applied) which is dropped afterwards.
/// </summary>
public sealed class StoredProcedureTests : IAsyncLifetime
{
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        var builder = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable(SqlServerFactAttribute.EnvironmentVariable)!)
        {
            InitialCatalog = $"HearthBeanTests_{Guid.NewGuid():N}"
        };
        _connectionString = builder.ConnectionString;

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await ApplySqlObjectsAsync(db);
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext();
        await db.Database.EnsureDeletedAsync();
    }

    [SqlServerFact]
    public async Task PlaceOrder_CreatesOrderAndItems_DecrementsStock_AndClearsCart()
    {
        await using var db = CreateContext();
        var (userId, bread, cake) = await SeedAsync(db);
        db.CartItems.AddRange(
            new CartItem { UserId = userId, ProductId = bread.Id, Quantity = 2 },
            new CartItem { UserId = userId, ProductId = cake.Id, Quantity = 1 });
        await db.SaveChangesAsync();

        var service = new OrderService(db, NullLogger<OrderService>.Instance);
        var orderId = await service.PlaceAsync(userId, new CheckoutViewModel { DeliveryMethod = "Pickup", Phone = "+1-555-0100" });

        await using var verify = CreateContext();
        var order = await verify.Orders.Include(item => item.Items).SingleAsync(item => item.Id == orderId);
        Assert.Equal(OrderStatuses.Pending, order.Status);
        Assert.Equal(15.00m, order.TotalAmount);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(4.25m, order.Items.Single(item => item.ProductId == bread.Id).UnitPrice);
        Assert.Equal(3, (await verify.Products.SingleAsync(item => item.Id == bread.Id)).StockQuantity);
        Assert.Equal(1, (await verify.Products.SingleAsync(item => item.Id == cake.Id)).StockQuantity);
        Assert.Empty(await verify.CartItems.Where(item => item.UserId == userId).ToListAsync());
    }

    [SqlServerFact]
    public async Task PlaceOrder_WithInsufficientStock_RollsBackEverything()
    {
        await using var db = CreateContext();
        var (userId, bread, cake) = await SeedAsync(db);
        db.CartItems.AddRange(
            new CartItem { UserId = userId, ProductId = bread.Id, Quantity = 2 },
            new CartItem { UserId = userId, ProductId = cake.Id, Quantity = 3 }); // only 2 cakes in stock
        await db.SaveChangesAsync();

        var service = new OrderService(db, NullLogger<OrderService>.Instance);
        var error = await Assert.ThrowsAsync<SqlException>(() =>
            service.PlaceAsync(userId, new CheckoutViewModel { DeliveryMethod = "Pickup", Phone = "+1-555-0100" }));

        Assert.Contains("no longer available", error.Message);
        await using var verify = CreateContext();
        Assert.Empty(await verify.Orders.ToListAsync());
        Assert.Equal(5, (await verify.Products.SingleAsync(item => item.Id == bread.Id)).StockQuantity);
        Assert.Equal(2, (await verify.Products.SingleAsync(item => item.Id == cake.Id)).StockQuantity);
        Assert.Equal(2, await verify.CartItems.CountAsync(item => item.UserId == userId));
    }

    [SqlServerFact]
    public async Task PlaceOrder_WithEmptyCart_IsRejected()
    {
        await using var db = CreateContext();
        var (userId, _, _) = await SeedAsync(db);

        var service = new OrderService(db, NullLogger<OrderService>.Instance);
        var error = await Assert.ThrowsAsync<SqlException>(() =>
            service.PlaceAsync(userId, new CheckoutViewModel { DeliveryMethod = "Pickup", Phone = "+1-555-0100" }));

        Assert.Contains("cart is empty", error.Message);
        await using var verify = CreateContext();
        Assert.Empty(await verify.Orders.ToListAsync());
    }

    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_connectionString).Options);

    private static async Task<(string UserId, Product Bread, Product Cake)> SeedAsync(AppDbContext db)
    {
        var category = new Category { Name = "Test", Slug = "test" };
        var bread = new Product { Category = category, Name = "Test loaf", Slug = "test-loaf", Description = "Test product", Price = 4.25m, StockQuantity = 5 };
        var cake = new Product { Category = category, Name = "Test cake", Slug = "test-cake", Description = "Test product", Price = 6.50m, StockQuantity = 2 };
        var user = new ApplicationUser { UserName = "test@example.com", Email = "test@example.com", FullName = "Test Customer" };

        db.Users.Add(user);
        db.Products.AddRange(bread, cake);
        await db.SaveChangesAsync();
        return (user.Id, bread, cake);
    }

    private static async Task ApplySqlObjectsAsync(AppDbContext db)
    {
        var script = await File.ReadAllTextAsync(FindObjectsScript());
        var batches = Regex.Split(script, @"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (var batch in batches.Where(batch => !string.IsNullOrWhiteSpace(batch)))
        {
            await db.Database.ExecuteSqlRawAsync(batch);
        }
    }

    private static string FindObjectsScript()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "database", "objects.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("database/objects.sql was not found above the test output directory.");
    }
}

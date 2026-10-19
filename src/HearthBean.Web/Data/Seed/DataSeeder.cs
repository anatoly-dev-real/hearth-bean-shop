using HearthBean.Web.Data.Entities;
using HearthBean.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HearthBean.Web.Data.Seed;

public static class DataSeeder
{
    private static readonly (string Slug, string Name)[] CategoryData =
    [
        ("bread", "Bread"), ("pastries", "Pastries"), ("cakes", "Cakes"),
        ("coffee", "Coffee"), ("seasonal", "Seasonal")
    ];

    private static readonly (string Slug, string Name, string Description, decimal Price, int Stock, string Image, bool Vegan)[] ProductData =
    [
        ("country-sourdough", "Country sourdough", "Naturally leavened country loaf with a deep crust.", 8.50m, 18, "bread.jpg", true),
        ("seeded-rye", "Seeded rye", "Dark rye with toasted seeds and a tender crumb.", 7.00m, 14, "bread.jpg", true),
        ("olive-rosemary-focaccia", "Olive & rosemary focaccia", "Olive oil-rich focaccia with rosemary.", 6.50m, 10, "bread.jpg", true),
        ("whole-wheat-loaf", "Whole-wheat loaf", "Stone-ground whole-wheat flour, slow fermented.", 7.50m, 12, "bread.jpg", true),
        ("baguette", "Baguette", "A crisp classic with a tender centre.", 4.00m, 20, "bread.jpg", true),
        ("milk-bread", "Milk bread", "Pillowy lightly sweet bread made with butter.", 6.00m, 10, "bread.jpg", false),
        ("everything-bun", "Everything bun", "Soft roll with our toasted seed blend.", 3.50m, 16, "bread.jpg", true),
        ("croissant", "Butter croissant", "Hand-laminated and baked until deeply golden.", 4.25m, 16, "pastry.jpg", false),
        ("pain-au-chocolat", "Pain au chocolat", "Dark chocolate wrapped in crisp pastry.", 4.75m, 12, "pastry.jpg", false),
        ("almond-croissant", "Almond croissant", "Croissant filled with almond cream.", 5.50m, 8, "pastry.jpg", false),
        ("morning-bun", "Morning bun", "Laminated swirl rolled in cinnamon sugar.", 4.50m, 12, "pastry.jpg", false),
        ("cardamom-knot", "Cardamom knot", "Fragrant bun with freshly ground cardamom.", 4.50m, 10, "pastry.jpg", false),
        ("blueberry-scone", "Blueberry scone", "Cream scone with berries and a crisp edge.", 4.00m, 12, "pastry.jpg", false),
        ("lemon-poppy-muffin", "Lemon poppy muffin", "Lemon zest and poppy seed in a soft crumb.", 3.75m, 12, "pastry.jpg", false),
        ("cheese-twist", "Cheese twist", "Flaky pastry twisted with aged cheddar.", 4.25m, 10, "pastry.jpg", false),
        ("apple-turnover", "Apple turnover", "Seasonal apples folded into buttery pastry.", 4.50m, 10, "pastry.jpg", false),
        ("cinnamon-roll", "Cinnamon roll", "Soft dough, warm cinnamon and cream-cheese glaze.", 4.75m, 10, "pastry.jpg", false),
        ("olive-oil-cake", "Olive oil cake", "Tender citrus cake made with good olive oil.", 6.00m, 8, "cake.jpg", false),
        ("vanilla-bean-cake", "Vanilla bean cake", "Vanilla sponge with a light whipped cream.", 7.50m, 6, "cake.jpg", false),
        ("carrot-cake", "Carrot cake", "Spiced carrot layers with cream-cheese frosting.", 6.50m, 8, "cake.jpg", false),
        ("chocolate-tart", "Chocolate tart", "Dark chocolate ganache in a cocoa shell.", 6.75m, 8, "cake.jpg", false),
        ("cheesecake-slice", "Cheesecake slice", "Baked vanilla cheesecake with a crisp base.", 6.25m, 8, "cake.jpg", false),
        ("pear-almond-tart", "Pear & almond tart", "Ripe pear and almond frangipane in short crust.", 6.50m, 6, "cake.jpg", false),
        ("espresso", "House espresso", "A balanced double shot of seasonal coffee.", 3.00m, 50, "coffee.jpg", true),
        ("flat-white", "Flat white", "Double espresso with silky steamed milk.", 4.25m, 50, "coffee.jpg", false),
        ("filter-coffee", "Batch brew", "A clean bright cup brewed fresh all day.", 3.25m, 50, "coffee.jpg", true),
        ("chai", "House chai", "Black tea simmered with warming spices and milk.", 4.00m, 24, "coffee.jpg", false),
        ("apple-galette", "Apple galette", "A rustic tart with local apples and spice.", 5.75m, 8, "seasonal.jpg", false),
        ("pumpkin-loaf", "Pumpkin loaf", "Warmly spiced loaf made with roasted pumpkin.", 5.00m, 10, "seasonal.jpg", false),
        ("cranberry-orange-scone", "Cranberry orange scone", "Tart cranberries and fresh orange.", 4.25m, 10, "seasonal.jpg", false)
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        foreach (var (slug, name) in CategoryData)
        {
            if (!await db.Categories.AnyAsync(category => category.Slug == slug))
            {
                db.Categories.Add(new Category { Slug = slug, Name = name, SortOrder = Array.FindIndex(CategoryData, item => item.Slug == slug) });
            }
        }

        var legacyExtra = await db.Products.SingleOrDefaultAsync(product => product.Slug == "chocolate-chip-cookie");
        if (legacyExtra is not null)
        {
            legacyExtra.IsAvailable = false;
        }

        await db.SaveChangesAsync();
        var categoryIds = await db.Categories.ToDictionaryAsync(category => category.Slug, category => category.Id);

        for (var index = 0; index < ProductData.Length; index++)
        {
            var item = ProductData[index];
            if (await db.Products.AnyAsync(product => product.Slug == item.Slug))
            {
                continue;
            }

            var categorySlug = index < 7 ? "bread" : index < 17 ? "pastries" : index < 23 ? "cakes" : index < 27 ? "coffee" : "seasonal";
            db.Products.Add(new Product
            {
                CategoryId = categoryIds[categorySlug], Name = item.Name, Slug = item.Slug,
                Description = item.Description, Price = item.Price, StockQuantity = item.Stock,
                ImageFileName = item.Image, IsVegan = item.Vegan
            });
        }

        await db.SaveChangesAsync();
        await SeedIdentityAsync(services);
        await SeedOrdersAsync(services);
        await ApplySqlObjectsAsync(db);
    }

    private static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Customer", "Admin" })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var customerPassword = configuration["DemoAccounts:CustomerPassword"];
        if (!string.IsNullOrWhiteSpace(customerPassword))
        {
            for (var index = 1; index <= 12; index++)
            {
                var email = $"customer{index:00}@example.com";
                if (await users.FindByEmailAsync(email) is not null)
                {
                    continue;
                }

                var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = $"Test Customer {index:00}" };
                if ((await users.CreateAsync(user, customerPassword)).Succeeded)
                {
                    await users.AddToRoleAsync(user, "Customer");
                }
            }
        }

        var adminEmail = configuration["DemoAccounts:AdminEmail"];
        var adminPassword = configuration["DemoAccounts:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await users.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FullName = "Demo Administrator" };
                if (!(await users.CreateAsync(admin, adminPassword)).Succeeded)
                {
                    return;
                }
            }

            if (!await users.IsInRoleAsync(admin, "Admin"))
            {
                await users.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    private static async Task SeedOrdersAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Orders.AnyAsync())
        {
            return;
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var customers = await users.GetUsersInRoleAsync("Customer");
        if (customers.Count < 12)
        {
            return;
        }

        var products = await db.Products.OrderBy(product => product.Id).ToListAsync();
        var statuses = new[]
        {
            OrderStatuses.Pending, OrderStatuses.Confirmed, OrderStatuses.Preparing,
            OrderStatuses.Ready, OrderStatuses.Completed, OrderStatuses.Cancelled
        };

        for (var index = 0; index < 40; index++)
        {
            var first = products[(index * 3) % products.Count];
            var second = products[(index * 3 + 7) % products.Count];
            var quantity = 1 + index % 3;
            var items = new List<OrderItem>
            {
                new() { Product = first, Quantity = quantity, UnitPrice = first.Price },
                new() { Product = second, Quantity = 1, UnitPrice = second.Price }
            };
            db.Orders.Add(new Order
            {
                UserId = customers[index % customers.Count].Id,
                OrderDate = DateTime.UtcNow.AddDays(-((index * 4) % 180)),
                Status = statuses[index % statuses.Length],
                DeliveryMethod = index % 3 == 0 ? "Delivery" : "Pickup",
                DeliveryAddress = index % 3 == 0 ? $"{100 + index} Example Street, Brooklyn, NY" : null,
                Phone = $"+1-555-0100-{index:00}",
                Note = index % 4 == 0 ? "Demo seed order" : null,
                Items = items,
                TotalAmount = DomainRules.CalculateTotal(items.Select(item => (item.UnitPrice, item.Quantity)))
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task ApplySqlObjectsAsync(AppDbContext db)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "database", "objects.sql");
        var script = await File.ReadAllTextAsync(path);
        var batches = Regex.Split(script, @"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (var batch in batches.Where(batch => !string.IsNullOrWhiteSpace(batch)))
        {
            await db.Database.ExecuteSqlRawAsync(batch);
        }
    }
}
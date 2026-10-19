using HearthBean.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HearthBean.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().Property(user => user.FullName).HasMaxLength(120);
        builder.Entity<ApplicationUser>().Property(user => user.Address).HasMaxLength(300);

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(category => category.Slug).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.Property(category => category.Slug).HasMaxLength(100).IsRequired();
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.Slug).IsUnique();
            entity.HasIndex(product => product.CategoryId);
            entity.Property(product => product.Name).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Slug).HasMaxLength(140).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(1200).IsRequired();
            entity.Property(product => product.Price).HasPrecision(10, 2);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Products_Price", "[Price] >= 0");
                table.HasCheckConstraint("CK_Products_StockQuantity", "[StockQuantity] >= 0");
            });
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
            entity.ToTable(table => table.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));
            entity.HasOne(item => item.Product).WithMany(product => product.CartItems).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasIndex(order => new { order.UserId, order.OrderDate });
            entity.Property(order => order.TotalAmount).HasPrecision(10, 2);
            entity.Property(order => order.Status).HasMaxLength(20).IsRequired();
            entity.Property(order => order.DeliveryMethod).HasMaxLength(20).IsRequired();
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasPrecision(10, 2);
            entity.ToTable(table => table.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0"));
            entity.HasOne(item => item.Product).WithMany(product => product.OrderItems).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
using System.ComponentModel.DataAnnotations;
using HearthBean.Web.Data.Entities;

namespace HearthBean.Web.Models.ViewModels;

public sealed class AdminDashboardViewModel
{
    public int ProductCount { get; init; }
    public int OrderCount { get; init; }
    public int CustomerCount { get; init; }
    public decimal PendingRevenue { get; init; }
    public IReadOnlyList<Order> RecentOrders { get; init; } = [];
}

public sealed class ProductEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression("^[a-z0-9-]+$"), StringLength(140)]
    public string Slug { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    [Required, RegularExpression("^[a-z0-9-]+\\.jpg$")]
    public string ImageFileName { get; set; } = "bread.jpg";

    public bool IsVegan { get; set; }
    public IReadOnlyList<Category> Categories { get; set; } = [];
}

public sealed record RevenueReportRow(int Year, int Month, decimal Revenue, int Orders);
public sealed record TopProductReportRow(string Name, int Quantity, decimal Revenue);

public sealed class ReportsViewModel
{
    public IReadOnlyList<RevenueReportRow> Revenue { get; init; } = [];
    public IReadOnlyList<TopProductReportRow> TopProducts { get; init; } = [];
    public IReadOnlyList<Product> LowStockProducts { get; init; } = [];
}

public sealed class CategoryEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression("^[a-z0-9-]+$"), StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int SortOrder { get; set; }
}
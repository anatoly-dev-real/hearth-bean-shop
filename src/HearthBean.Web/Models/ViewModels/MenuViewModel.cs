using HearthBean.Web.Data.Entities;

namespace HearthBean.Web.Models.ViewModels;

public sealed class MenuViewModel
{
    public IReadOnlyList<ProductCardViewModel> Products { get; init; } = [];
    public IReadOnlyList<Category> Categories { get; init; } = [];
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public string Sort { get; init; } = "featured";
    public int Page { get; init; }
    public int PageCount { get; init; }
    public int TotalCount { get; init; }
}
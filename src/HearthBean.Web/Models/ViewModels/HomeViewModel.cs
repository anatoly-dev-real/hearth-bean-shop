namespace HearthBean.Web.Models.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; init; } = [];
}
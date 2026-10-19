using System.ComponentModel.DataAnnotations;

namespace HearthBean.Web.Models.ViewModels;

public sealed record CartLineViewModel(int ProductId, string Slug, string Name, string ImageFileName, decimal Price, int Quantity, int StockQuantity)
{
    public decimal LineTotal => Price * Quantity;
}

public sealed class CartViewModel
{
    public IReadOnlyList<CartLineViewModel> Lines { get; init; } = [];
    public decimal Total => Lines.Sum(line => line.LineTotal);
}

public sealed class CheckoutPageViewModel
{
    public CheckoutViewModel Checkout { get; set; } = new();
    public CartViewModel Cart { get; set; } = new();
}

public sealed class CheckoutViewModel
{
    [Required, StringLength(20)]
    public string DeliveryMethod { get; set; } = "Pickup";

    [StringLength(300)]
    public string? DeliveryAddress { get; set; }

    [Required, Phone, StringLength(32)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; set; }
}

public sealed class RegisterViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
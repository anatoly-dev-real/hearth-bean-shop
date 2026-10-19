using HearthBean.Web.Data.Entities;

namespace HearthBean.Web.Services;

public static class DomainRules
{
    public static bool IsValidCartQuantity(int quantity, int stock) => quantity is >= 1 and <= 20 && quantity <= stock;

    public static decimal CalculateTotal(IEnumerable<(decimal UnitPrice, int Quantity)> lines) =>
        lines.Sum(line => line.UnitPrice * line.Quantity);

    public static void RestoreStock(IEnumerable<OrderItem> items)
    {
        foreach (var item in items)
        {
            item.Product.StockQuantity += item.Quantity;
        }
    }

    public static bool CanCustomerCancel(Order order, string userId) =>
        order.UserId == userId && order.Status == OrderStatuses.Pending;

    public static bool CanTransition(string current, string next) => OrderStatuses.CanTransition(current, next);
}
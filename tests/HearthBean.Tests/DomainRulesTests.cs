using HearthBean.Web.Data.Entities;
using HearthBean.Web.Services;

namespace HearthBean.Tests;

public sealed class DomainRulesTests
{
    [Fact]
    public void CartQuantity_AcceptsOneThroughTwentyWithinStock()
    {
        Assert.True(DomainRules.IsValidCartQuantity(1, 1));
        Assert.True(DomainRules.IsValidCartQuantity(20, 20));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(21, 30)]
    [InlineData(5, 4)]
    public void CartQuantity_RejectsOutOfRangeOrInsufficientStock(int quantity, int stock)
    {
        Assert.False(DomainRules.IsValidCartQuantity(quantity, stock));
    }

    [Fact]
    public void CalculateTotal_UsesCapturedUnitPrices()
    {
        var total = DomainRules.CalculateTotal([(4.25m, 2), (3.50m, 1)]);

        Assert.Equal(12.00m, total);
    }

    [Fact]
    public void RestoreStock_ReturnsCancelledOrderQuantities()
    {
        var product = new Product { StockQuantity = 3 };
        var items = new[] { new OrderItem { Product = product, Quantity = 2 } };

        DomainRules.RestoreStock(items);

        Assert.Equal(5, product.StockQuantity);
    }

    [Theory]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Confirmed, true)]
    [InlineData(OrderStatuses.Confirmed, OrderStatuses.Preparing, true)]
    [InlineData(OrderStatuses.Preparing, OrderStatuses.Ready, true)]
    [InlineData(OrderStatuses.Ready, OrderStatuses.Completed, true)]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Completed, false)]
    [InlineData(OrderStatuses.Completed, OrderStatuses.Cancelled, false)]
    public void StatusTransition_FollowsAllowedWorkflow(string current, string next, bool expected)
    {
        Assert.Equal(expected, DomainRules.CanTransition(current, next));
    }

    [Fact]
    public void CustomerCanCancelOnlyOwnPendingOrder()
    {
        var order = new Order { UserId = "customer-1", Status = OrderStatuses.Pending };

        Assert.True(DomainRules.CanCustomerCancel(order, "customer-1"));
        Assert.False(DomainRules.CanCustomerCancel(order, "customer-2"));
        order.Status = OrderStatuses.Confirmed;
        Assert.False(DomainRules.CanCustomerCancel(order, "customer-1"));
    }
}

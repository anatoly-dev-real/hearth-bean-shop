namespace HearthBean.Web.Data.Entities;

public sealed class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = OrderStatuses.Pending;
    public string DeliveryMethod { get; set; } = "Pickup";
    public string? DeliveryAddress { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
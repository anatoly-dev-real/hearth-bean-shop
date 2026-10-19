namespace HearthBean.Web.Data.Entities;

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsAvailable { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
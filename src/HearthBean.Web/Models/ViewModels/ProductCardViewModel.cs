using HearthBean.Web.Data.Entities;

namespace HearthBean.Web.Models.ViewModels;

public sealed record ProductCardViewModel(Product Product, string Name, string Description, string CategoryName);
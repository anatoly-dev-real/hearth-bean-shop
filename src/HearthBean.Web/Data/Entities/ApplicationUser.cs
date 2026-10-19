using Microsoft.AspNetCore.Identity;

namespace HearthBean.Web.Data.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
}
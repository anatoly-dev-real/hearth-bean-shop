using System.ComponentModel.DataAnnotations;

namespace HearthBean.Web.Models.ViewModels;

public sealed class ProfileViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Phone, StringLength(32)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300)]
    public string Address { get; set; } = string.Empty;
}
using System.Text.RegularExpressions;

namespace HearthBean.Web.Services;

/// <summary>
/// Picks the picture for a product: wwwroot/images/products/{slug}.(jpg|jpeg|png|webp) when such a
/// file exists, otherwise the category photo stored in Product.ImageFileName.
/// </summary>
public sealed partial class ProductImages(IWebHostEnvironment environment)
{
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    public string FileFor(string slug, string fallbackFileName)
    {
        if (!SafeSlug().IsMatch(slug))
        {
            return fallbackFileName;
        }

        foreach (var extension in Extensions)
        {
            var relativePath = $"products/{slug}{extension}";
            if (environment.WebRootFileProvider.GetFileInfo($"images/{relativePath}").Exists)
            {
                return relativePath;
            }
        }

        return fallbackFileName;
    }

    [GeneratedRegex("^[a-z0-9-]+$")]
    private static partial Regex SafeSlug();
}

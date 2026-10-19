using System.Security.Claims;
using HearthBean.Web.Data;
using HearthBean.Web.Data.Entities;
using HearthBean.Web.Models.ViewModels;
using HearthBean.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HearthBean.Web.Controllers;

[Route("Account")]
public sealed class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AppDbContext db, CartService cart) : Controller
{
    [HttpGet("Register")]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await users.AddToRoleAsync(user, "Customer");
        await signIn.SignInAsync(user, isPersistent: false);
        await cart.MergeGuestCartAsync(user.Id, HttpContext.Session, cancellationToken);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel());

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signIn.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var user = await users.FindByEmailAsync(model.Email);
            if (user is not null && await users.IsInRoleAsync(user, "Customer"))
            {
                await cart.MergeGuestCartAsync(user.Id, HttpContext.Session, cancellationToken);
            }

            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid sign-in attempt.");
        return View(model);
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await db.Users.FindAsync([userId], cancellationToken);
        return View(new ProfileViewModel { FullName = user!.FullName, Phone = user.PhoneNumber ?? string.Empty, Address = user.Address ?? string.Empty });
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("Profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await db.Users.FindAsync([userId], cancellationToken);
        user!.FullName = model.FullName;
        user.PhoneNumber = model.Phone;
        user.Address = model.Address;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Notice"] = "Profile saved.";
        return RedirectToAction(nameof(Profile));
    }
}
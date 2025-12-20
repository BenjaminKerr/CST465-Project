using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CST465_project.Models;

namespace CST465_project.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public UserController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var model = new EmailChangeDto { NewEmail = user.Email ?? string.Empty };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EmailChangeDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var currentEmail = user.Email ?? string.Empty;
        if (string.Equals(currentEmail, model.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["StatusMessage"] = "No changes made.";
            return RedirectToAction(nameof(Index));
        }

        // Ensure email is not used by another account
        var existing = await _userManager.FindByEmailAsync(model.NewEmail);
        if (existing != null && existing.Id != user.Id)
        {
            ModelState.AddModelError(nameof(EmailChangeDto.NewEmail), "That email is already in use.");
            return View(model);
        }

        var setEmailResult = await _userManager.SetEmailAsync(user, model.NewEmail);
        var setUserNameResult = await _userManager.SetUserNameAsync(user, model.NewEmail);

        if (setEmailResult.Succeeded && setUserNameResult.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Email updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        var errs = setEmailResult.Errors.Concat(setUserNameResult.Errors).Select(e => e.Description);
        ModelState.AddModelError(string.Empty, string.Join("; ", errs));
        return View(model);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CST465_project.Models;

namespace CST465_project.Controllers;

[Authorize(Roles = "Admin")]
[Route("[controller]")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new AdminUserDto { Id = u.Id, Email = u.Email ?? u.UserName ?? "", IsAdmin = roles.Contains("Admin") });
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var isInRole = await _userManager.IsInRoleAsync(user, "Admin");
        IdentityResult result;
        if (isInRole)
        {
            result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded) TempData["StatusMessage"] = $"Removed Admin from {user.Email}";
        }
        else
        {
            // ensure role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded) TempData["StatusMessage"] = $"Granted Admin to {user.Email}";
        }

        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }
}

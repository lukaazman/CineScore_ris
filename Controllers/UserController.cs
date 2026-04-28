using CineScore.Data;
using CineScore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Controllers;

public class UserController : Controller
{
    private readonly CineScoreContext _context;
    private readonly UserManager<User> _userManager;

    public UserController(CineScoreContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? id)
    {
        string? userId = id ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Ratings)
                .ThenInclude(r => r.Movie)
            .Include(u => u.Comments)
                .ThenInclude(c => c.Movie)
            .Include(u => u.WatchlistItems)
                .ThenInclude(w => w.Movie)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        return View(new UserProfileEditViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? ""
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(UserProfileEditViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.UserName = model.UserName.Trim();
        user.Email = model.Email.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        TempData["ProfileSuccess"] = "Profil je uspešno posodobljen.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromWatchlist(int id)
    {
        var userId = _userManager.GetUserId(User);
        var item = await _context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item == null)
        {
            return NotFound();
        }

        _context.WatchlistItems.Remove(item);
        await _context.SaveChangesAsync();

        TempData["ProfileSuccess"] = "Film je odstranjen iz seznama za ogled.";
        return RedirectToAction(nameof(Index));
    }
}

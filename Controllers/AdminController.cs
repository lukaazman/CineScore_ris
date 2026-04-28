using CineScore.Data;
using CineScore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Controllers;

public class AdminController : Controller
{
    private readonly CineScoreContext _context;
    private readonly UserManager<User> _userManager;

    public AdminController(CineScoreContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.OrderBy(u => u.UserName).ToListAsync();
        return View(users);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Roles = await _userManager.GetRolesAsync(user);
        return View(user);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return View(new AdminUserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "User"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(AdminUserEditViewModel model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
        if (user == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.UserName = model.UserName.Trim();
        user.Email = model.Email.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var existingRoles = await _userManager.GetRolesAsync(user);
        if (existingRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, existingRoles);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        return RedirectToAction(nameof(Users));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserName == User.Identity?.Name)
        {
            ModelState.AddModelError(string.Empty, "Trenutno prijavljenega administratorja ni mogoče izbrisati.");
            return View("Delete", user);
        }

        var commentIds = await _context.Comments
            .Where(c => c.UserId == id)
            .Select(c => c.Id)
            .ToListAsync();

        var reactions = await _context.CommentReactions
            .Where(r => r.UserId == id || commentIds.Contains(r.CommentId))
            .ToListAsync();

        var watchlistItems = await _context.WatchlistItems.Where(w => w.UserId == id).ToListAsync();
        var comments = await _context.Comments.Where(c => c.UserId == id).ToListAsync();
        var ratings = await _context.Ratings.Where(r => r.UserId == id).ToListAsync();

        _context.CommentReactions.RemoveRange(reactions);
        _context.WatchlistItems.RemoveRange(watchlistItems);
        _context.Comments.RemoveRange(comments);
        _context.Ratings.RemoveRange(ratings);
        await _context.SaveChangesAsync();

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            foreach (var error in deleteResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Delete", user);
        }

        return RedirectToAction(nameof(Users));
    }
}

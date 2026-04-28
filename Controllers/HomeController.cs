using System.Diagnostics;
using CineScore.Data;
using CineScore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Controllers;

public class HomeController : Controller
{
    private readonly CineScoreContext _context;

    public HomeController(CineScoreContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Admin_dashboard()
    {
        return View();
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 12;
        var totalMovies = await _context.Movies.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalMovies / (double)pageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);

        var movies = await _context.Movies
            .Include(m => m.Statistika)
            .OrderBy(m => m.Title)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        ViewData["Title"] = "Domov";
        ViewData["Heading"] = "Pregled filmov";
        ViewData["Lead"] = "Prebrskaj lokalno zbirko filmov in odpri podrobnosti posameznega naslova.";

        return View(new PagedMoviesResult(movies, currentPage, totalPages));
    }

    public async Task<IActionResult> Search(string? query, int page = 1)
    {
        const int pageSize = 12;
        var trimmedQuery = query?.Trim() ?? string.Empty;
        var normalizedQuery = trimmedQuery.ToLower();

        ViewData["Title"] = "Iskanje";
        ViewData["Heading"] = string.IsNullOrWhiteSpace(trimmedQuery)
            ? "Iskanje filmov"
            : $"Rezultati za \"{trimmedQuery}\"";
        ViewData["Lead"] = string.IsNullOrWhiteSpace(trimmedQuery)
            ? "Išči po naslovu, žanru ali letu filma."
            : "Prikazani so filmi, ki ustrezajo iskalnemu nizu.";
        ViewData["SearchQuery"] = trimmedQuery;

        var queryable = _context.Movies
            .Include(m => m.Statistika)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(trimmedQuery))
        {
            queryable = queryable.Where(movie =>
                movie.Title.ToLower().Contains(normalizedQuery) ||
                movie.Genre.ToLower().Contains(normalizedQuery) ||
                movie.Year.ToString().Contains(normalizedQuery));
        }

        var totalMovies = await queryable.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalMovies / (double)pageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);

        var movies = await queryable
            .OrderBy(movie => movie.Title)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedMoviesResult(movies, currentPage, totalPages, trimmedQuery));
    }

    public async Task<IActionResult> TopRated(int page = 1)
    {
        const int pageSize = 12;
        var movies = await _context.Movies
            .Include(m => m.Statistika)
            .AsNoTracking()
            .ToListAsync();

        var orderedMovies = movies
            .OrderByDescending(m => m.Statistika?.PovprecjeOcen ?? 0)
            .ThenBy(m => m.Title)
            .ToList();

        var totalPages = Math.Max(1, (int)Math.Ceiling(orderedMovies.Count / (double)pageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);
        var pagedMovies = orderedMovies
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewData["Title"] = "Najbolje ocenjeni";
        ViewData["Heading"] = "Najbolje ocenjeni filmi";
        ViewData["Lead"] = "Filmi so razvrščeni glede na povprečno oceno uporabnikov.";

        return View(new PagedMoviesResult(pagedMovies, currentPage, totalPages));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

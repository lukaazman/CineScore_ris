using CineScore.Data;
using CineScore.Models;
using CineScore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CineScoreContext _context;
        private readonly UserManager<User> _userManager;
        private readonly K_OceniFilm _oceniFilm;

        public MoviesController(
            CineScoreContext context,
            UserManager<User> userManager,
            K_OceniFilm oceniFilm)
        {
            _context = context;
            _userManager = userManager;
            _oceniFilm = oceniFilm;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details_user), new { id = comment.MovieId });
        }

        public async Task<IActionResult> Review(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Statistika)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, string text, int rating)
        {
            var movie = await _oceniFilm.pridobiFilm(id);
            if (movie == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (!_oceniFilm.preveriPrijavo(userId))
            {
                TempData["RatingError"] = "Za ocenjevanje morate biti prijavljeni.";
                return RedirectToAction(nameof(Details_user), new { id });
            }

            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError(nameof(rating), "Ocena mora biti med 1 in 5.");
                return View(movie);
            }

            var ocena = _oceniFilm.ustvariOceno(rating, userId!, id);
            await _oceniFilm.shraniOceno(ocena);
            await _oceniFilm.posodobiPovprecje(id);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var existingComment = await _context.Comments
                    .FirstOrDefaultAsync(c => c.MovieId == id && c.UserId == userId);

                if (existingComment == null)
                {
                    _context.Comments.Add(new Comment
                    {
                        MovieId = id,
                        Text = text.Trim(),
                        UserId = userId!,
                        CreatedAt = DateTime.Now,
                        Rating = rating
                    });
                }
                else
                {
                    existingComment.Text = text.Trim();
                    existingComment.Rating = rating;
                    existingComment.CreatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            TempData["RatingSuccess"] = "Ocena je bila uspešno shranjena.";
            return RedirectToAction(nameof(Details_user), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToWatchlist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["WatchlistError"] = "Za dodajanje v seznam za ogled morate biti prijavljeni.";
                return RedirectToAction(nameof(Details_user), new { id });
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            var existingItem = await _context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == id);

            if (existingItem != null)
            {
                TempData["WatchlistInfo"] = "Film je že v seznamu za ogled.";
                return RedirectToAction(nameof(Details_user), new { id });
            }

            _context.WatchlistItems.Add(new WatchlistItem
            {
                UserId = userId,
                MovieId = id,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["WatchlistSuccess"] = "Film je dodan v seznam za ogled.";
            return RedirectToAction(nameof(Details_user), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ReactToComment(int commentId, int movieId, bool isLike)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["RatingError"] = "Za odziv na komentar morate biti prijavljeni.";
                return RedirectToAction(nameof(Details_user), new { id = movieId });
            }

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var reaction = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

            if (reaction == null)
            {
                _context.CommentReactions.Add(new CommentReaction
                {
                    CommentId = commentId,
                    UserId = userId,
                    IsLike = isLike,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                reaction.IsLike = isLike;
                reaction.CreatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details_user), new { id = movieId });
        }

        [AllowAnonymous]
        public IActionResult Back()
        {
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details_user(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Statistika)
                .Include(m => m.Ratings)
                    .ThenInclude(r => r.User)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.User)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.Reactions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.Include(m => m.Statistika).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Statistika)
                .Include(m => m.Ratings)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.User)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.Reactions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Year,Genre,Description,PosterUrl,BannerUrl")] Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            _context.Add(movie);
            await _context.SaveChangesAsync();
            await _oceniFilm.posodobiPovprecje(movie.Id);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Year,Genre,Description,PosterUrl,BannerUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            try
            {
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}

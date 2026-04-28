using CineScore.Data;
using CineScore.Models;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Services
{
    public class K_OceniFilm
    {
        private readonly CineScoreContext _context;

        public K_OceniFilm(CineScoreContext context)
        {
            _context = context;
        }

        public bool preveriPrijavo(string? uporabnikId)
        {
            return !string.IsNullOrWhiteSpace(uporabnikId);
        }

        public async Task<Movie?> pridobiFilm(int filmId)
        {
            return await _context.Movies
                .Include(f => f.Statistika)
                .FirstOrDefaultAsync(f => f.Id == filmId);
        }

        public Rating ustvariOceno(int vrednost, string uporabnikId, int filmId)
        {
            return new Rating
            {
                Score = vrednost,
                UserId = uporabnikId,
                MovieId = filmId,
                CreatedAt = DateTime.Now
            };
        }

        public async Task<Rating> shraniOceno(Rating ocena)
        {
            var obstojecaOcena = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == ocena.UserId && r.MovieId == ocena.MovieId);

            if (obstojecaOcena == null)
            {
                _context.Ratings.Add(ocena);
                await _context.SaveChangesAsync();
                return ocena;
            }

            obstojecaOcena.Score = ocena.Score;
            obstojecaOcena.CreatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return obstojecaOcena;
        }

        public async Task<StatistikaFilma> posodobiPovprecje(int filmId)
        {
            var ocene = await _context.Ratings
                .Where(r => r.MovieId == filmId)
                .ToListAsync();

            var statistika = await _context.Statistics
                .FirstOrDefaultAsync(s => s.MovieId == filmId);

            if (statistika == null)
            {
                statistika = new StatistikaFilma { MovieId = filmId };
                _context.Statistics.Add(statistika);
            }

            var steviloOcen = ocene.Count;
            var povprecje = steviloOcen == 0 ? 0f : (float)ocene.Average(r => r.Score);
            statistika.posodobiStatistiko(steviloOcen, povprecje);
            await _context.SaveChangesAsync();

            return statistika;
        }
    }
}

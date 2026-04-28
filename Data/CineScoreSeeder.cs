using CineScore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Data
{
    public static class CineScoreSeeder
    {
        public static async Task SeedAsync(
            CineScoreContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await EnsureExtendedSchemaAsync(context);

            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUser = await EnsureUserAsync(userManager, "admin", "admin@example.com", "Admin123!", "Admin");
            var demoUser = await EnsureUserAsync(userManager, "demo", "demo@example.com", "Demo123!", "User");

            if (!await context.Movies.AnyAsync())
            {
                context.Movies.AddRange(
                    new Movie
                    {
                        Title = "Inception",
                        Year = 2010,
                        Genre = "Sci-Fi, Thriller",
                        Description = "Tat vstopa v sanje drugih ljudi in skuša izvesti najbolj tvegan miselni rop.",
                        PosterUrl = "/images/official/inception-poster.jpg",
                        BannerUrl = "/images/official/inception-banner.jpg"
                    },
                    new Movie
                    {
                        Title = "Interstellar",
                        Year = 2014,
                        Genre = "Sci-Fi, Drama",
                        Description = "Skupina astronavtov išče nov dom za človeštvo, medtem ko Zemlja postaja nevzdržna.",
                        PosterUrl = "/images/official/interstellar-poster.jpg",
                        BannerUrl = "/images/official/interstellar-banner.jpg"
                    },
                    new Movie
                    {
                        Title = "Parasite",
                        Year = 2019,
                        Genre = "Drama, Thriller",
                        Description = "Družina z roba družbe se postopno infiltrira v življenje bogate družine z nepredvidljivimi posledicami.",
                        PosterUrl = "/images/official/parasite-poster.jpg",
                        BannerUrl = "/images/official/parasite-banner.jpg"
                    });
                await context.SaveChangesAsync();
            }

            await SyncLocalMovieAssetsAsync(context);
            await SyncSeedReviewsAsync(context, demoUser, adminUser);
            await SyncSeedWatchlistAsync(context, demoUser);
            await SyncSeedReactionsAsync(context, demoUser, adminUser);

            var movies = await context.Movies.ToListAsync();
            foreach (var movie in movies)
            {
                var ratings = await context.Ratings.Where(r => r.MovieId == movie.Id).ToListAsync();
                var stats = await context.Statistics.FirstOrDefaultAsync(s => s.MovieId == movie.Id);
                if (stats == null)
                {
                    stats = new StatistikaFilma { MovieId = movie.Id };
                    context.Statistics.Add(stats);
                }

                var count = ratings.Count;
                var average = count == 0 ? 0f : (float)ratings.Average(r => r.Score);
                stats.posodobiStatistiko(count, average);
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureExtendedSchemaAsync(CineScoreContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "WatchlistItems" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_WatchlistItems" PRIMARY KEY AUTOINCREMENT,
                    "UserId" TEXT NOT NULL,
                    "MovieId" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_WatchlistItems_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_WatchlistItems_Movies_MovieId" FOREIGN KEY ("MovieId") REFERENCES "Movies" ("Id") ON DELETE CASCADE
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_WatchlistItems_UserId_MovieId"
                ON "WatchlistItems" ("UserId", "MovieId");
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "CommentReactions" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_CommentReactions" PRIMARY KEY AUTOINCREMENT,
                    "UserId" TEXT NOT NULL,
                    "CommentId" INTEGER NOT NULL,
                    "IsLike" INTEGER NOT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_CommentReactions_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_CommentReactions_Comments_CommentId" FOREIGN KEY ("CommentId") REFERENCES "Comments" ("Id") ON DELETE CASCADE
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_CommentReactions_UserId_CommentId"
                ON "CommentReactions" ("UserId", "CommentId");
                """);
        }

        private static async Task SyncLocalMovieAssetsAsync(CineScoreContext context)
        {
            var assetMap = new Dictionary<string, (string Poster, string Banner)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Inception"] = ("/images/official/inception-poster.jpg", "/images/official/inception-banner.jpg"),
                ["Interstellar"] = ("/images/official/interstellar-poster.jpg", "/images/official/interstellar-banner.jpg"),
                ["Parasite"] = ("/images/official/parasite-poster.jpg", "/images/official/parasite-banner.jpg")
            };

            var movies = await context.Movies.ToListAsync();
            var hasChanges = false;

            foreach (var movie in movies)
            {
                if (!assetMap.TryGetValue(movie.Title, out var assetPaths))
                {
                    continue;
                }

                if (!string.Equals(movie.PosterUrl, assetPaths.Poster, StringComparison.Ordinal) ||
                    !string.Equals(movie.BannerUrl, assetPaths.Banner, StringComparison.Ordinal))
                {
                    movie.PosterUrl = assetPaths.Poster;
                    movie.BannerUrl = assetPaths.Banner;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task SyncSeedReviewsAsync(CineScoreContext context, User demoUser, User adminUser)
        {
            var now = DateTime.Now;

            await UpsertSeedReviewAsync(
                context,
                "Inception",
                demoUser.Id,
                5,
                "Napeta ideja o sanjah v sanjah, vizualno zelo močan film z odličnim tempom.",
                now.AddDays(-2));

            await UpsertSeedReviewAsync(
                context,
                "Interstellar",
                adminUser.Id,
                4,
                "Močna znanstvenofantastična drama z zelo dobro atmosfero in čustvenim zaključkom.",
                now.AddDays(-1));

            await UpsertSeedReviewAsync(
                context,
                "Parasite",
                demoUser.Id,
                5,
                "Odličen družbeni komentar z napetim stopnjevanjem in zelo nepozabnim razpletom.",
                now.AddHours(-12));
        }

        private static async Task SyncSeedWatchlistAsync(CineScoreContext context, User demoUser)
        {
            var movie = await context.Movies.FirstAsync(m => m.Title == "Interstellar");
            var item = await context.WatchlistItems
                .FirstOrDefaultAsync(w => w.UserId == demoUser.Id && w.MovieId == movie.Id);

            if (item == null)
            {
                context.WatchlistItems.Add(new WatchlistItem
                {
                    UserId = demoUser.Id,
                    MovieId = movie.Id,
                    CreatedAt = DateTime.Now.AddDays(-1)
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task SyncSeedReactionsAsync(CineScoreContext context, User demoUser, User adminUser)
        {
            var interstellarComment = await context.Comments
                .FirstOrDefaultAsync(c => c.UserId == adminUser.Id && c.Movie!.Title == "Interstellar");

            if (interstellarComment != null)
            {
                await UpsertReactionAsync(context, interstellarComment.Id, demoUser.Id, true);
            }

            var inceptionComment = await context.Comments
                .FirstOrDefaultAsync(c => c.UserId == demoUser.Id && c.Movie!.Title == "Inception");

            if (inceptionComment != null)
            {
                await UpsertReactionAsync(context, inceptionComment.Id, adminUser.Id, true);
            }
        }

        private static async Task UpsertSeedReviewAsync(
            CineScoreContext context,
            string movieTitle,
            string userId,
            int score,
            string text,
            DateTime createdAt)
        {
            var movie = await context.Movies.FirstAsync(m => m.Title == movieTitle);

            var rating = await context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == movie.Id && r.UserId == userId);

            if (rating == null)
            {
                context.Ratings.Add(new Rating
                {
                    MovieId = movie.Id,
                    UserId = userId,
                    Score = score,
                    CreatedAt = createdAt
                });
            }
            else
            {
                rating.Score = score;
                rating.CreatedAt = createdAt;
            }

            var comment = await context.Comments
                .FirstOrDefaultAsync(c => c.MovieId == movie.Id && c.UserId == userId);

            if (comment == null)
            {
                context.Comments.Add(new Comment
                {
                    MovieId = movie.Id,
                    UserId = userId,
                    Rating = score,
                    Text = text,
                    CreatedAt = createdAt
                });
            }
            else
            {
                comment.Rating = score;
                comment.Text = text;
                comment.CreatedAt = createdAt;
            }

            await context.SaveChangesAsync();
        }

        private static async Task UpsertReactionAsync(
            CineScoreContext context,
            int commentId,
            string userId,
            bool isLike)
        {
            var reaction = await context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

            if (reaction == null)
            {
                context.CommentReactions.Add(new CommentReaction
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

            await context.SaveChangesAsync();
        }

        private static async Task<User> EnsureUserAsync(
            UserManager<User> userManager,
            string username,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Uporabnika {email} ni bilo mogoče ustvariti: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }
    }
}

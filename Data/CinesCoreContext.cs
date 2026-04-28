using CineScore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CineScore.Data
{
    public class CineScoreContext : IdentityDbContext<User>
    {
        public CineScoreContext(DbContextOptions<CineScoreContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<StatistikaFilma> Statistics { get; set; }
        public DbSet<WatchlistItem> WatchlistItems { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Movie>()
                .HasOne(m => m.Statistika)
                .WithOne(s => s.Movie)
                .HasForeignKey<StatistikaFilma>(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rating>()
                .HasIndex(r => new { r.UserId, r.MovieId })
                .IsUnique();

            builder.Entity<WatchlistItem>()
                .HasIndex(w => new { w.UserId, w.MovieId })
                .IsUnique();

            builder.Entity<CommentReaction>()
                .HasIndex(r => new { r.UserId, r.CommentId })
                .IsUnique();
        }
    }
}

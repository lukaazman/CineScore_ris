using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace CineScore.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public string Genre { get; set; } = "";
        public string Description { get; set; } = "";
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }

        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
        public StatistikaFilma? Statistika { get; set; }
    }
}

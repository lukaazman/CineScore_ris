using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace CineScore.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public required string UserId { get; set; }
        public int MovieId { get; set; }

        public User? User { get; set; }
        public Movie? Movie { get; set; }
    }
}

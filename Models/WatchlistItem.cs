namespace CineScore.Models
{
    public class WatchlistItem
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
        public Movie? Movie { get; set; }
    }
}

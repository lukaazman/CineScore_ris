namespace CineScore.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public required string UserId { get; set; }
        public int MovieId { get; set; }

        public User? User { get; set; }
        public Movie? Movie { get; set; }
        public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
    }
}

namespace CineScore.Models
{
    public class CommentReaction
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int CommentId { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? User { get; set; }
        public Comment? Comment { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;

namespace CineScore.Models
{
    public class User : IdentityUser
    {
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
        public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    }
}

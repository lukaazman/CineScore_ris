namespace CineScore.Models
{
    public class PagedMoviesResult
    {
        public PagedMoviesResult(IEnumerable<Movie> movies, int currentPage, int totalPages, string? searchQuery = null)
        {
            Movies = movies;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            SearchQuery = searchQuery;
        }

        public IEnumerable<Movie> Movies { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public string? SearchQuery { get; }
    }
}

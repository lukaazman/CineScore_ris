using System;

namespace CineScore.Models
{
    public class MovieCardViewModel
    {
        public MovieCardViewModel(Movie movie, double? averageRating, bool showAverageRating)
        {
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));
            AverageRating = averageRating;
            ShowAverageRating = showAverageRating;
        }

        public Movie Movie { get; }
        public double? AverageRating { get; }
        public bool ShowAverageRating { get; }
    }
}

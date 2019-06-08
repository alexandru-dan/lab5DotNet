using Lab1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1.ViewModels
{
    public class MoviePostModel
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Movie title must be at least 2 characters!")]
        public string Title { get; set; }
        public string Description { get; set; }
        public string MovieGenre { get; set; }
        public int DurationInMinutes { get; set; }
        public int ReleseYear { get; set; }
        public string Director { get; set; }
        public DateTime DateAdded { get; set; }
        [Range(1, 10)]
        public double Rating { get; set; }
        public string WasWatched { get; set; }
        public List<CommentGetModel> Comments { get; set; }

        public static MoviePostModel FromMovie(Movie movie)
        {
            string genre = "";

            if (movie.MovieGenre == Models.Genre.Comedy)
            {
                genre = "Comedy";
            }
            else if (movie.MovieGenre == Models.Genre.Horror)
            {
                genre = "Horror";
            }
            else if (movie.MovieGenre == Models.Genre.Thriller)
            {
                genre = "Thriller";
            }

            string watched = "";

            if (movie.WasWatched == Models.Watched.YES)
            {
                watched = "YES";
            }
            else
            {
                watched = "NO";
            }

            return new MoviePostModel
            {
                Title = movie.Title,
                Description = movie.Description,
                MovieGenre = genre,
                DurationInMinutes = movie.DurationInMinutes,
                ReleseYear = movie.ReleseYear,
                Director = movie.Director,
                DateAdded = movie.DateAdded,
                Rating = movie.Rating,
                WasWatched = watched,
                Comments = CommentGetModel.FromComments(movie.Comments)
            };
        }

        public static Movie ToMovie(MoviePostModel movie)
        {
            Models.Genre genre = new Models.Genre();

            if (movie.MovieGenre == "Comedy")
            {
                genre = Genre.Comedy;
            }
            else if (movie.MovieGenre == "Horror")
            {
                genre = Genre.Horror;
            }
            else if (movie.MovieGenre == "Thriller")
            {
                genre = Genre.Thriller;
            }

            Models.Watched watched = new Models.Watched();

            if (movie.WasWatched == "YES")
            {
                watched = Watched.YES;
            }
            else
            {
                watched = Watched.NO;
            }

            return new Movie
            {
                Title = movie.Title,
                Description = movie.Description,
                MovieGenre = genre,
                DurationInMinutes = movie.DurationInMinutes,
                ReleseYear = movie.ReleseYear,
                Director = movie.Director,
                DateAdded = movie.DateAdded,
                Rating = movie.Rating,
                WasWatched = watched,
                Comments = CommentGetModel.ToComments(movie.Comments)
            };
        }

        public static Movie ToUpdateMovie(MoviePostModel moviePostModel, Movie movie)
        {
            Models.Genre genre = new Models.Genre();

            if (moviePostModel.MovieGenre == "Comedy")
            {
                genre = Genre.Comedy;
            }
            else if (moviePostModel.MovieGenre == "Horror")
            {
                genre = Genre.Horror;
            }
            else if (moviePostModel.MovieGenre == "Thriller")
            {
                genre = Genre.Thriller;
            }

            Models.Watched watched = new Models.Watched();

            if (moviePostModel.WasWatched == "YES")
            {
                watched = Watched.YES;
            }
            else
            {
                watched = Watched.NO;
            }

            movie.Title = moviePostModel.Title;
            movie.Description = moviePostModel.Description;
            movie.MovieGenre = genre;
            movie.DurationInMinutes = moviePostModel.DurationInMinutes;
            movie.ReleseYear = moviePostModel.ReleseYear;
            movie.Director = moviePostModel.Director;
            movie.DateAdded = moviePostModel.DateAdded;
            movie.Rating = moviePostModel.Rating;
            movie.WasWatched = watched;
            movie.Comments = CommentGetModel.ToComments(moviePostModel.Comments);

            return movie;
        }
    }
}

using Lab1.Models;
using Lab1.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1.Services
{
    public interface IMovieService
    {
        PaginatedList<MovieGetModel> GetAllMovies(int page, DateTime? from, DateTime? to);
        MoviePostModel GetById(int id);
        Movie Create(MoviePostModel movie);
        Movie Upsert(int id, MoviePostModel movie);
        Movie Delete(int id);
    }

    public class MovieService : IMovieService
    {
        private DataDbContext Context;

        /// <summary>
        /// Constructor for Service
        /// </summary>
        /// <param name="Context">Repository</param>
        public MovieService(DataDbContext Context)
        {
            this.Context = Context;
        }

        /// <summary>
        /// Create a new Movie, using MoviePostModel
        /// </summary>
        /// <param name="movie">New Movie object</param>
        /// <returns></returns>
        public Movie Create(MoviePostModel movie)
        {
            Movie convertedMovie = MoviePostModel.ToMovie(movie);

            if (convertedMovie == null)
            {
                return null;
            }

            Context.Movies.Add(convertedMovie);
            Context.SaveChanges();
            return convertedMovie;
        }

        /// <summary>
        /// Delete an movie using id
        /// </summary>
        /// <param name="id">Id of the movie we want to delete</param>
        /// <returns></returns>
        public Movie Delete(int id)
        {
            Movie foundMovie = Context.Movies.Include(comment => comment.Comments).FirstOrDefault(movie => movie.Id == id);
            if (foundMovie == null)
            {
                return null;
            }
            foreach (var item in foundMovie.Comments)
            {
                Context.Comments.Remove(item);
            }

            Context.Movies.Remove(foundMovie);
            Context.SaveChanges();

            return foundMovie;
        }

        /// <summary>
        /// Return all movies from db
        /// </summary>
        /// <param name="page">The page we want to see</param>
        /// <param name="from">Optional, filter by min date</param>
        /// <param name="to">Optional, filter by max date</param>
        /// <returns>List of movies with/without filters</returns>
        public PaginatedList<MovieGetModel> GetAllMovies(int page, DateTime? from, DateTime? to)
        {
            IQueryable<Movie> result = Context.Movies.Include(movie => movie.Comments).OrderByDescending(movie => movie.ReleseYear);
            PaginatedList<MovieGetModel> paginatedResult = new PaginatedList<MovieGetModel>();
            paginatedResult.CurrentPage = page;

            if (from != null)
            {
                result = result.Where(movie => movie.DateAdded > from);
            }
            if (to != null)
            {
                result = result.Where(movie => movie.DateAdded < to);
            }

            paginatedResult.NumberOfPages = (result.Count() - 1) / PaginatedList<MovieGetModel>.EntriesPerPage + 1;
            result = result
                .Skip((page - 1) * PaginatedList<MovieGetModel>.EntriesPerPage)
                .Take(PaginatedList<MovieGetModel>.EntriesPerPage);
            paginatedResult.Entries = result.Select(f => MovieGetModel.FromMovie(f)).ToList();


            return paginatedResult;
        }



        public MoviePostModel GetById(int id)
        {
            var res = Context.Movies
                .Include(e => e.Comments)
                .FirstOrDefault(f => f.Id == id);

            return MoviePostModel.FromMovie(res);
        }

        /// <summary>
        /// Update or Add
        /// </summary>
        /// <param name="id">Id for update</param>
        /// <param name="movies">New Movie object we want to add</param>
        /// <returns></returns>
        public Movie Upsert(int id, MoviePostModel movies)
        {
            var existing = Context.Movies.AsNoTracking().FirstOrDefault(c => c.Id == id);

            if (existing == null)
            {
                var movie = Context.Movies.Add(MoviePostModel.ToMovie(movies));
                Context.SaveChanges();
                return movie.Entity;
            }
            var res = MoviePostModel.ToUpdateMovie(movies, existing);

            Context.SaveChanges();

            return res;
        }
    }
}

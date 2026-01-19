using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Core;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Services.Contracts.Get;
using MvcMovie.Services.Interfaces;

namespace MvcMovie.Services;

public class MovieService(MvcMovieContext context) : IMovieService
{
    public async Task<GetMoviesPageResponse> GetPage(
        GetMoviesPageRequest request,
        CancellationToken cancellationToken
    )
    {
        IQueryable<Movie> moviesQuery = context.Movie;

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            moviesQuery = moviesQuery.Where(m => m.Title.Contains(request.SearchString));
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            moviesQuery = moviesQuery.Where(m => m.Genre == request.Genre);
        }

        if (request.SortOrder == "desc")
        {
            moviesQuery = moviesQuery.OrderByDescending(GetSortProperty(request));
        }
        else
        {
            moviesQuery = moviesQuery.OrderBy(GetSortProperty(request));
        }

        int count = await moviesQuery.CountAsync(cancellationToken);

        Movie[] moviesPage = await moviesQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync(cancellationToken);

        PagedList<Movie> pagedList = new(moviesPage, request.Page, request.PageSize, count);

        return new GetMoviesPageResponse(
            pagedList.Page,
            pagedList.PageSize,
            pagedList.TotalCount,
            pagedList.TotalPages,
            pagedList.HasPreviousPage,
            pagedList.HasNextPage,
            pagedList.Items
        );
    }

    private static Expression<Func<Movie, object>> GetSortProperty(GetMoviesPageRequest request)
    {
        return request.SortColumn switch
        {
            "title" => movie => movie.Title,
            "release_date" => movie => movie.ReleaseDate,
            "genre" => movie => movie.Genre,
            "price" => movie => movie.Price,
            "rating" => movie => movie.Rating,
            _ => movie => movie.Title,
        };
    }
}

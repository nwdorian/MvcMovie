using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Core.Primitives;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Services.Contracts.Create;
using MvcMovie.Services.Contracts.Get;
using MvcMovie.Services.Contracts.GetById;
using MvcMovie.Services.Interfaces;
using MvcMovie.ViewModels.Movies;
using MvcMovie.ViewModels.Movies.Projections;
using MvcMovie.ViewModels.Shared;

namespace MvcMovie.Controllers;

public class MoviesController(IMovieService movieService, MvcMovieContext context) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string sortColumn = "title",
        string sortOrder = "asc",
        string? genre = null,
        string? searchString = null,
        CancellationToken cancellationToken = default
    )
    {
        GetMoviesPageRequest getMoviesRequest = new(
            page,
            pageSize,
            sortColumn,
            sortOrder,
            searchString,
            genre
        );

        GetMoviesPageResponse getMoviesResponse = await movieService.GetPage(
            getMoviesRequest,
            cancellationToken
        );

        var movieGenreVM = new MovieIndexViewModel
        {
            Movies = getMoviesResponse
                .Movies.Select(m => new MovieDisplay(
                    m.Id,
                    m.Title,
                    m.ReleaseDate,
                    m.Genre,
                    m.Price,
                    m.Rating
                ))
                .ToList(),
            PagingMetadata = new PagingMetadata(
                getMoviesResponse.Page,
                getMoviesResponse.PageSize,
                getMoviesResponse.TotalCount,
                getMoviesResponse.TotalPages,
                getMoviesResponse.HasPreviousPage,
                getMoviesResponse.HasNextPage
            ),
            Genres = new SelectList(
                await context
                    .Movie.OrderBy(m => m.Genre)
                    .Select(m => m.Genre)
                    .Distinct()
                    .ToListAsync(cancellationToken),
                selectedValue: genre
            ),
            SortColumns = new SelectList(
                new[]
                {
                    new { Value = "title", Text = "Title" },
                    new { Value = "release_date", Text = "Release Date" },
                    new { Value = "genre", Text = "Genre" },
                    new { Value = "price", Text = "Price" },
                    new { Value = "rating", Text = "Rating" },
                },
                "Value",
                "Text",
                selectedValue: sortColumn
            ),
            SortOrders = new SelectList(
                new[]
                {
                    new { Value = "asc", Text = "Ascending" },
                    new { Value = "desc", Text = "Descending" },
                },
                "Value",
                "Text",
                selectedValue: sortOrder
            ),
            Genre = genre,
            SortColumn = sortColumn,
            SortOrder = sortOrder,
            SearchString = searchString,
        };

        return View(movieGenreVM);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            return NotFound();
        }

        GetMovieByIdRequest getMovieRequest = new(id.Value);
        Result<GetMovieByIdResponse> getMovieResponse = await movieService.GetById(
            getMovieRequest,
            cancellationToken
        );

        if (getMovieResponse.IsFailure)
        {
            return View(
                new MovieDetailsViewModel { Errors = new() { getMovieResponse.Error.Description } }
            );
        }

        MovieDetails movie = new(
            getMovieResponse.Value.Id,
            getMovieResponse.Value.Title,
            getMovieResponse.Value.ReleaseDate,
            getMovieResponse.Value.Genre,
            getMovieResponse.Value.Price,
            getMovieResponse.Value.Rating
        );

        return View(new MovieDetailsViewModel { Movie = movie });
    }

    public IActionResult Create()
    {
        return View(new MovieCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieCreate movie, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(new MovieCreateViewModel() { Movie = movie });
        }

        CreateMovieRequest createMovieRequest = new(
            movie.Title,
            movie.ReleaseDate,
            movie.Genre,
            movie.Price,
            movie.Rating
        );
        Result createMovieResponse = await movieService.Create(
            createMovieRequest,
            cancellationToken
        );

        if (createMovieResponse.IsFailure)
        {
            return View(
                new MovieCreateViewModel()
                {
                    Errors = new() { createMovieResponse.Error.Description },
                }
            );
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await context.Movie.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }
        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie
    )
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(movie);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(movie);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await context.Movie.FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await context.Movie.FindAsync(id);
        if (movie != null)
        {
            context.Movie.Remove(movie);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int id)
    {
        return context.Movie.Any(e => e.Id == id);
    }
}

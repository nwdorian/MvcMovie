using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Services.Contracts.Get;
using MvcMovie.Services.Interfaces;
using MvcMovie.ViewModels;

namespace MvcMovie.Controllers;

public class MoviesController(IMovieService movieService, MvcMovieContext context) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 8,
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

        var movieGenreVM = new MovieGenreViewModel
        {
            Genres = new SelectList(
                await context
                    .Movie.OrderBy(m => m.Genre)
                    .Select(m => m.Genre)
                    .Distinct()
                    .ToListAsync(cancellationToken)
            ),
            Movies = getMoviesResponse.Movies,
        };

        return View(movieGenreVM);
    }

    public async Task<IActionResult> Details(int? id)
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

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie
    )
    {
        if (ModelState.IsValid)
        {
            context.Add(movie);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(movie);
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

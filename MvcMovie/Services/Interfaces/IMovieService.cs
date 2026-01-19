using MvcMovie.Services.Contracts.Get;

namespace MvcMovie.Services.Interfaces;

public interface IMovieService
{
    Task<GetMoviesPageResponse> GetPage(
        GetMoviesPageRequest request,
        CancellationToken cancellationToken
    );
}

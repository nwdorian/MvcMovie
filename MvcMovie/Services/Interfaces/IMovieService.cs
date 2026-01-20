using MvcMovie.Core.Primitives;
using MvcMovie.Services.Contracts.Create;
using MvcMovie.Services.Contracts.Get;
using MvcMovie.Services.Contracts.GetById;

namespace MvcMovie.Services.Interfaces;

public interface IMovieService
{
    Task<GetMoviesPageResponse> GetPage(
        GetMoviesPageRequest request,
        CancellationToken cancellationToken
    );
    Task<Result<GetMovieByIdResponse>> GetById(
        GetMovieByIdRequest request,
        CancellationToken cancellationToken
    );
    Task<Result> Create(CreateMovieRequest request, CancellationToken cancellationToken);
}

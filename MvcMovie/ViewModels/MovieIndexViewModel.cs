using Microsoft.AspNetCore.Mvc.Rendering;

namespace MvcMovie.ViewModels;

public class MovieIndexViewModel
{
    public IReadOnlyList<MovieDisplay> Movies { get; set; } = [];
    public PagingMetadata PagingMetadata { get; set; } = default!;
    public SelectList Genres { get; set; } = default!;
    public SelectList SortColumns { get; set; } = default!;
    public SelectList SortOrders { get; set; } = default!;
    public string? Genre { get; set; }
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
    public string? SearchString { get; set; }
}

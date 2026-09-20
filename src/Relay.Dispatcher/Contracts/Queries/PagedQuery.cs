namespace Relay.Dispatcher.Contracts.Queries;

/// <summary>
/// Represents a page-based query that retrieves a page of results using a page number and page size.
/// </summary>
/// <typeparam name="TResult">The type of result returned for each item.</typeparam>
public record PagedQuery<TResult> : IQuery<PagedQueryResult<TResult>>
    where TResult : class
{
    /// <summary>
    /// Gets or sets the maximum number of items to return in the page.
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// Gets or sets the current page number. The first page starts at 1.
    /// </summary>
    public int CurrentPage { get; set; }
}
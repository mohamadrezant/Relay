namespace Relay.Dispatcher.Contracts.Queries;

/// <summary>
/// Represents the result of a paginated query, containing the requested items and the total number of available items.
/// </summary>
/// <typeparam name="TResult">The type of result returned for each item.</typeparam>
public record PagedQueryResult<TResult>
    where TResult : class
{
    /// <summary>
    /// Gets or sets the items returned for the requested page.
    /// </summary>
    public IEnumerable<TResult> Results { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items available across all pages.
    /// </summary>
    public int TotalCount { get; set; }
}
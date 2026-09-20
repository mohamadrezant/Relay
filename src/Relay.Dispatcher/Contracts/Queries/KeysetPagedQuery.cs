namespace Relay.Dispatcher.Contracts.Queries;

/// <summary>
/// Represents a keyset-paginated query that retrieves a page of results based on the last seen key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to determine the position in the result set.</typeparam>
/// <typeparam name="TResult">The type of result returned for each item.</typeparam>
public record KeysetPagedQuery<TKey, TResult> : IQuery<PagedQueryResult<TResult>>
    where TKey : IComparable<TKey>
    where TResult : class
{
    /// <summary>
    /// Gets or sets the maximum number of items to return in the page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the key of the last item seen in the previous page.
    /// </summary>
    public TKey LastSeenKey { get; set; }
}
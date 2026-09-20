namespace Relay.Dispatcher.Contracts.Queries;

/// <summary>
/// Defines a handler responsible for processing a query and producing its result.
/// </summary>
/// <typeparam name="TQuery">The type of query handled by the implementation.</typeparam>
/// <typeparam name="TResult">The type of result produced by the handler.</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IHandler
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Executes the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to process.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation and containing the query result.</returns>
    Task<TResult> InvokeAsync(TQuery query, CancellationToken cancellationToken);
}
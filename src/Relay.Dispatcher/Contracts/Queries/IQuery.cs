namespace Relay.Dispatcher.Contracts.Queries;

/// <summary>
/// Represents a query that produces a result of the specified type.
/// </summary>
/// <typeparam name="TResult">The type of result produced by the query.</typeparam>
public interface IQuery<TResult>
    where TResult : notnull
{
}
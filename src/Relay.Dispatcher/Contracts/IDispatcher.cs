namespace Relay.Dispatcher.Contracts;

/// <summary>
/// Defines the main contract for dispatching queries, commands, notifications, and events.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a query to its corresponding handler and returns the query result.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to dispatch.</typeparam>
    /// <typeparam name="TResult">The type of result produced by the query.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation and containing the query result.</returns>
    Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
        where TResult : notnull;

    /// <summary>
    /// Sends a command to its corresponding handler and returns the command response.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to dispatch.</typeparam>
    /// <typeparam name="TResponse">The type of response produced by the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation and containing the command response.</returns>
    Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
        where TResponse : notnull;

    /// <summary>
    /// Publishes a notification to its registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish.</typeparam>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous notification publishing operation.</returns>
    Task PublishNotificationAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;

    /// <summary>
    /// Raises an event and dispatches it to its registered listeners.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to raise.</typeparam>
    /// <param name="raisedEvent">The event to raise.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous event dispatch operation.</returns>
    Task RaiseEventAsync<TEvent>(TEvent raisedEvent, CancellationToken cancellationToken)
        where TEvent : class;
}
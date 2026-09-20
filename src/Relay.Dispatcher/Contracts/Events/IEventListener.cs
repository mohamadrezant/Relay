namespace Relay.Dispatcher.Contracts.Events;

/// <summary>
/// Defines a listener responsible for handling a raised event.
/// </summary>
/// <typeparam name="TEvent">The type of event handled by the listener.</typeparam>
public interface IEventListener<in TEvent> : IHandler, IParallelHandler
    where TEvent : class
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="raisedEvent">The event to handle.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    Task InvokeAsync(TEvent raisedEvent, CancellationToken cancellationToken);
}
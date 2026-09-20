namespace Relay.Dispatcher.Contracts.Notifications;

/// <summary>
/// Defines a handler responsible for processing a published notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification handled by the implementation.</typeparam>
public interface INotificationHandler<in TNotification> : IHandler, IParallelHandler
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous notification handling operation.</returns>
    Task InvokeAsync(TNotification notification, CancellationToken cancellationToken);
}
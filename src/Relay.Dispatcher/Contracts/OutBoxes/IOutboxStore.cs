namespace Relay.Dispatcher.Contracts.OutBoxes;

/// <summary>
/// Defines a storage abstraction for persisting and retrieving Outbox messages.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Persists an Outbox context to the underlying store.
    /// </summary>
    /// <param name="context">The Outbox context to persist.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    Task PushAsync(OutboxContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the next available Outbox context from the underlying store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous retrieval operation and containing the retrieved Outbox context.</returns>
    Task<OutboxContext> PopAsync(CancellationToken cancellationToken);
}
namespace Relay.Dispatcher.Contracts.OutBoxes;

/// <summary>
/// Represents a message stored in the Outbox, containing either an event or a notification
/// together with its creation and processing metadata.
/// </summary>
public record OutboxContext
{
    /// <summary>
    /// Initializes a new Outbox context and generates a unique identifier for it.
    /// </summary>
    private OutboxContext()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Creates an Outbox context from the specified payload.
    /// </summary>
    /// <typeparam name="T">The type of the payload.</typeparam>
    /// <param name="payload">The event or notification to store in the Outbox.</param>
    /// <returns>A new <see cref="OutboxContext"/> containing the specified payload.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is <see langword="null"/>.</exception>
    public static OutboxContext Create<T>(T payload)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var context = new OutboxContext
        {
            CreatedAtUtc = DateTime.UtcNow,
            ProcessedAtUtc = null,
        };

        if (payload is INotification notification)
        {
            context.Event = null;
            context.Notification = notification;
        }
        else // payload is event
        {
            context.Event = payload;
            context.Notification = null;
        }

        return context;
    }

    /// <summary>
    /// Gets the unique identifier of the Outbox context.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the UTC date and time when the Outbox context was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the event payload stored in the Outbox context, when the payload represents an event.
    /// </summary>
    public object Event { get; private set; }

    /// <summary>
    /// Gets the notification payload stored in the Outbox context, when the payload represents a notification.
    /// </summary>
    public INotification Notification { get; private set; }

    /// <summary>
    /// Gets the UTC date and time when the Outbox context was marked as processed.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; private set; }

    /// <summary>
    /// Marks the Outbox context as processed and records the current UTC date and time.
    /// </summary>
    public void SetProcessed()
        => ProcessedAtUtc = DateTime.UtcNow;
}
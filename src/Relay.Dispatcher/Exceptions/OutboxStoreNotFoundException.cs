namespace Relay.Dispatcher.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a required Outbox store cannot be found.
/// </summary>
public sealed class OutboxStoreNotFoundException()
    : Exception(@"Your outbox store was not found.
        Make sure you have registered a outbox store handler that implements IOutboxStore or change your target IParallelHandler's InvocationMode.")
{
}
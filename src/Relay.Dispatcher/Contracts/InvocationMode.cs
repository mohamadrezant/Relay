namespace Relay.Dispatcher.Contracts;

/// <summary>
/// Specifies how a series of operations or tasks should be executed.
/// </summary>
public enum InvocationMode
{
    /// <summary>
    /// Executes operations sequentially and in their registration order.
    /// Use when operations must execute in order or depend on shared, non-thread-safe resources.
    /// </summary>
    Sequential = 0,
    /// <summary>
    /// Executes operations concurrently.
    /// Use when operations are independent and do not access shared, non-thread-safe resources such as <c>DbContext</c>.
    /// </summary>
    Parallel = 1,

    /// <summary>
    /// Persists operations into an Outbox store for reliable asynchronous processing.
    /// Use when at-least-once delivery and eventual consistency are required.
    /// </summary>
    OutBox = 2,
}
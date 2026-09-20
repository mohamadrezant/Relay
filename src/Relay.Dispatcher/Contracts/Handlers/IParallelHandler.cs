namespace Relay.Dispatcher.Contracts.Handlers;

/// <summary>
/// Defines a handler that specifies how its invocation should be executed.
/// </summary>
public interface IParallelHandler : IHandler
{
    /// <summary>
    /// Gets the invocation mode that determines how the handler is executed.
    /// </summary>
    InvocationMode InvocationMode { get; }
}
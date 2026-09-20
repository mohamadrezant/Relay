namespace Relay.Dispatcher.Exceptions;

/// <summary>
/// Represents an exception that is thrown when no handler is found for a specified request and response type.
/// </summary>
/// <typeparam name="TRequest">The type of request for which a handler could not be found.</typeparam>
/// <typeparam name="TResponse">The type of response expected from the handler.</typeparam>
public sealed class HandlerNotFoundException<TRequest, TResponse>() : Exception(
    $"""
    Handler for request of type '{typeof(TRequest).Name}' returning '{typeof(TResponse).Name}' was not found.
    Make sure you have registered a handler that implements IHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}>. 
    """);
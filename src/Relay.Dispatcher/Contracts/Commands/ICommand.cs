namespace Relay.Dispatcher.Contracts.Commands;

/// <summary>
/// Represents a command that produces a response of the specified type.
/// </summary>
/// <typeparam name="TResponse">The type of response produced by the command.</typeparam>
public interface ICommand<TResponse>
    where TResponse : notnull
{
}
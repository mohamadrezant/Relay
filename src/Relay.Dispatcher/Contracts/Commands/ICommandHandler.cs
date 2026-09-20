namespace Relay.Dispatcher.Contracts.Commands;

/// <summary>
/// Defines a handler responsible for processing a command and producing its response.
/// </summary>
/// <typeparam name="TCommand">The type of command handled by the implementation.</typeparam>
/// <typeparam name="TResponse">The type of response produced by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IHandler
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation and containing the command response.</returns>
    Task<TResponse> InvokeAsync(TCommand command, CancellationToken cancellationToken);
}
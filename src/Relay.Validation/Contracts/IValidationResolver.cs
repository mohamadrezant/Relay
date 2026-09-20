namespace Relay.Validation.Contracts;

/// <summary>
/// Resolves the validator associated with a command.
/// </summary>
/// <remarks>
/// A validation resolver is responsible for locating the appropriate
/// <see cref="IValidator{TCommand}"/> for a given command.
///
/// The resolver does not perform validation itself; it only provides
/// the validator that is responsible for validating the command.
/// </remarks>
public interface IValidationResolver
{
    /// <summary>
    /// Resolves the validator associated with the specified command.
    /// </summary>
    /// <typeparam name="TCommand">
    /// The type of command for which the validator should be resolved.
    /// </typeparam>
    /// <param name="command">
    /// The command whose corresponding validator should be resolved.
    /// </param>
    /// <returns>
    /// The validator associated with <typeparamref name="TCommand"/>.
    /// </returns>
    /// <remarks>
    /// Implementations may use different mechanisms to resolve validators,
    /// such as dependency injection, reflection, or an external service.
    ///
    /// The command instance is provided to allow custom implementations
    /// to make resolution decisions based on the command when necessary.
    /// </remarks>
    IValidator<TCommand> Resolve<TCommand>(TCommand command)
        where TCommand : notnull;
}
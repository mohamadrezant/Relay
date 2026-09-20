using Microsoft.Extensions.DependencyInjection;
using Relay.Validation.Contracts;

namespace Relay.Validation.Adapters.InMemory;

/// <summary>
/// Resolves validators from the application's dependency injection container.
/// </summary>
/// <param name="serviceProvider">
/// The service provider used to resolve validators.
/// </param>
/// <remarks>
/// This implementation uses the built-in dependency injection container
/// to locate an <see cref="IValidator{TCommand}"/> for the specified command type.
///
/// Validators must be registered in the dependency injection container
/// before they can be resolved.
/// </remarks>
internal sealed class InMemoryValidationResolverAdapter(IServiceProvider serviceProvider)
    : IValidationResolver
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Resolves the validator associated with the specified command.
    /// </summary>
    /// <typeparam name="TCommand">
    /// The type of command to validate.
    /// </typeparam>
    /// <param name="command">
    /// The command for which the validator is being resolved.
    /// </param>
    /// <returns>
    /// The registered validator for <typeparamref name="TCommand"/>,
    /// or <see langword="null"/> if no matching validator is registered.
    /// </returns>
    /// <remarks>
    /// The command parameter is used to infer the generic command type.
    /// The current implementation does not use the command instance itself
    /// during resolution.
    /// </remarks>
    public IValidator<TCommand> Resolve<TCommand>(TCommand command)
    {
        var validator = _serviceProvider.GetService<IValidator<TCommand>>();

        return validator;
    }
}
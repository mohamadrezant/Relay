using Relay.Validation.Contracts;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.Validation;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Defines a validator for a specific command type.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command that is being validated.
/// </typeparam>
/// <remarks>
/// Implementations of this interface contain the validation rules for
/// <typeparamref name="TCommand"/> and return the result of the validation
/// operation.
///
/// A validator is responsible only for evaluating the command and producing
/// a <see cref="ValidationResult"/>. How validators are discovered and
/// resolved is handled separately by <see cref="IValidationResolver"/>.
/// </remarks>
public interface IValidator<TCommand>
    where TCommand : notnull
{
    /// <summary>
    /// Validates the specified command.
    /// </summary>
    /// <param name="command">
    /// The command to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the validation operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationResult"/> containing the outcome of the
    /// validation operation and any validation errors.
    /// </returns>
    Task<ValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken);
}
using Relay.Validation.Exceptions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.Validation;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents the outcome of a validation operation.
/// </summary>
/// <remarks>
/// A validation result can represent either a successful validation or
/// a validation failure containing a property name, one or more error
/// messages, and an associated <see cref="ValidationException"/>.
///
/// Use <see cref="Success"/> to create a successful result and the
/// provided factory methods such as <see cref="NotNull"/> and
/// <see cref="NotFound"/> for common validation failures.
/// </remarks>
public sealed record ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class
    /// representing a successful validation result.
    /// </summary>
    private ValidationResult() { }

    /// <summary>
    /// Initializes a validation result with a property name and validation errors.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property associated with the validation errors.
    /// </param>
    /// <param name="errors">
    /// The validation error messages.
    /// </param>
    /// <remarks>
    /// If <paramref name="propertyName"/> is empty or
    /// <paramref name="errors"/> does not contain any errors,
    /// the result remains valid.
    /// </remarks>
    public ValidationResult(string propertyName, params string[] errors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        if (errors is null || errors.Length == 0)
            throw new ArgumentNullException(nameof(errors));

        PropertyName = propertyName;
        Errors = errors;
        Exception = new ValidationException(propertyName, errors);
    }

    /// <summary>
    /// Initializes a validation result with an inner exception,
    /// property name, and validation errors.
    /// </summary>
    /// <param name="innerException">
    /// The exception that caused or contributed to the validation failure.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property associated with the validation errors.
    /// </param>
    /// <param name="errors">
    /// The validation error messages.
    /// </param>
    public ValidationResult(Exception innerException, string propertyName, params string[] errors)
    {
        ArgumentNullException.ThrowIfNull(innerException, nameof(innerException));
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

        PropertyName = propertyName;
        Errors = errors;
        Exception = new ValidationException(innerException, propertyName, errors);
    }

    /// <summary>
    /// Initializes a validation result from an existing
    /// <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="exception">
    /// The validation exception representing the failure.
    /// </param>
    public ValidationResult(ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        Exception = exception;
    }

    /// <summary>
    /// Gets a value indicating whether the validation has failed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> when the result contains a validation
    /// exception, property name, or validation errors; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsFailure =>
        Exception is not null ||
        !string.IsNullOrWhiteSpace(PropertyName) ||
        Errors is not null && Errors.Length != 0;

    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    /// <value>
    /// <see langword="true"/> when the result does not represent
    /// a validation failure; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsValid => !IsFailure;

    /// <summary>
    /// Gets the name of the property associated with the validation errors.
    /// </summary>
    public string PropertyName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the validation error messages.
    /// </summary>
    public string[] Errors { get; private set; } = [];

    /// <summary>
    /// Gets the exception associated with the validation failure.
    /// </summary>
    /// <value>
    /// The associated <see cref="ValidationException"/>,
    /// or <see langword="null"/> when the validation was successful.
    /// </value>
    public ValidationException Exception { get; private set; }

    /// <summary>
    /// Throws the validation exception when the validation has failed.
    /// </summary>
    /// <exception cref="ValidationException">
    /// Thrown when the result represents a validation failure and contains
    /// a validation exception.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result represents a failure but does not contain
    /// a validation exception.
    /// </exception>
    [System.Diagnostics.DebuggerNonUserCode]
    public void ThrowOnFailure()
    {
        if (IsValid)
            return;

        if (Exception != null)
            throw Exception;

        throw new InvalidOperationException();
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>
    /// A <see cref="ValidationResult"/> representing successful validation.
    /// </returns>
    public static ValidationResult Success()
        => new();

    /// <summary>
    /// Creates a validation result indicating that the specified property
    /// is required.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the required property.
    /// </param>
    /// <returns>
    /// A failed <see cref="ValidationResult"/> containing a required-property
    /// validation error.
    /// </returns>
    public static ValidationResult NotNull(string propertyName)
    {
        return new()
        {
            Errors = ["Is Required."],
            PropertyName = propertyName,
            Exception = ValidationException.NotNull(propertyName),
        };
    }

    /// <summary>
    /// Creates a validation result indicating that an entity could not be found.
    /// </summary>
    /// <param name="keyPropertyName">
    /// The name of the property used as the entity key.
    /// </param>
    /// <param name="errors">
    /// Additional error messages describing the failure.
    /// </param>
    /// <returns>
    /// A failed <see cref="ValidationResult"/> representing a not-found error.
    /// </returns>
    public static ValidationResult NotFound(string keyPropertyName, params string[] errors)
        => new()
        {
            PropertyName = keyPropertyName,
            Exception = ValidationException.NotFound(keyPropertyName, errors),
        };
}
using System.Collections;

namespace Relay.Validation.Exceptions;

/// <summary>
/// Represents an exception raised when validation of an object fails.
/// </summary>
/// <remarks>
/// In addition to the underlying exception information, this exception
/// maintains validation errors grouped by property name.
///
/// The property-level errors are exposed through the overridden
/// <see cref="Exception.Data"/> property.
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors associated with the properties.
    /// </summary>
    /// <value>
    /// A dictionary whose keys are property names and whose values
    /// contain the validation errors associated with each property.
    /// </value>
    public override IDictionary Data => _propertyErrors;

    private readonly Dictionary<string, string[]> _propertyErrors = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// using an optional inner exception and a collection of property errors.
    /// </summary>
    /// <param name="innerException">
    /// The exception that caused or contributed to the validation failure.
    /// </param>
    /// <param name="propertiesErrors">
    /// The validation errors grouped by property name.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when neither an inner exception nor any property errors are provided.
    /// </exception>
    private ValidationException(Exception innerException, Dictionary<string, string[]> propertiesErrors)
        : base("ValidationError", innerException)
    {
        if (innerException is null && propertiesErrors.Count == 0)
            throw new ArgumentException($"At least one of {nameof(innerException)} or {nameof(propertiesErrors)} must have a value.");

        foreach (var propertyErrors in propertiesErrors)
        {
            if (propertyErrors.Value is not null)
                _propertyErrors.TryAdd(propertyErrors.Key, propertyErrors.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// for the specified property and validation errors.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property associated with the validation errors.
    /// </param>
    /// <param name="errors">
    /// The validation errors associated with the property.
    /// </param>
    public ValidationException(string propertyName, params string[] errors)
        : this(null, propertyName, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// with an inner exception and property validation errors.
    /// </summary>
    /// <param name="innerException">
    /// The exception that caused or contributed to the validation failure.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property associated with the validation errors.
    /// </param>
    /// <param name="errors">
    /// The validation errors associated with the property.
    /// </param>
    public ValidationException(Exception innerException, string propertyName, params string[] errors)
        : this(
            innerException,
            new Dictionary<string, string[]>
            {
                [propertyName] = errors
            })
    {
    }

    /// <summary>
    /// Creates a new validation exception based on an existing
    /// <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="exception">
    /// The validation exception to copy.
    /// </param>
    public ValidationException(ValidationException exception)
        : this(exception.InnerException, exception._propertyErrors)
    {
    }

    /// <summary>
    /// Creates a validation exception indicating that the specified property
    /// contains a null or missing required value.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the required property.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationException"/> representing a required-property error.
    /// </returns>
    public static ValidationException NotNull(string propertyName)
        => new(
            new ArgumentNullException(propertyName),
            propertyName,
            "Is Required.");

    /// <summary>
    /// Creates a validation exception indicating that an entity could not
    /// be found using the specified key property.
    /// </summary>
    /// <param name="keyPropertyName">
    /// The name of the property used to identify the entity.
    /// </param>
    /// <param name="errors">
    /// Additional error messages associated with the not-found failure.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationException"/> representing a not-found error.
    /// </returns>
    public static ValidationException NotFound(string keyPropertyName, params string[] errors)
        => new(
            new ArgumentOutOfRangeException(keyPropertyName),
            keyPropertyName,
            [.. errors, "Not Found."]);
}
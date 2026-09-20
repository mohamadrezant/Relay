using Relay.Validation.Exceptions;
using System.Linq.Expressions;

namespace Relay.Validation.Extensions;

/// <summary>
/// Provides extension methods for creating validation results
/// from domain and application objects.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Creates a validation result indicating that an entity could not
    /// be found using the specified property.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the entity being validated.
    /// </typeparam>
    /// <typeparam name="TProperty">
    /// The type of the property used to identify the entity.
    /// </typeparam>
    /// <param name="entity">
    /// The entity associated with the validation operation.
    /// </param>
    /// <param name="property">
    /// An expression identifying the property used as the entity key.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationResult"/> representing a not-found validation failure.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified expression does not represent a property access.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = entity.NotFound(x => x.Id);
    /// </code>
    /// </example>
#pragma warning disable IDE0060 // Remove unused parameter
    public static ValidationResult NotFound<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> property)
    {
        var name = property.GetPropertyName();
        var exception = ValidationException.NotFound(name);

        return new ValidationResult(exception, name);
    }
#pragma warning restore IDE0060 // Remove unused parameter

    /// <summary>
    /// Extracts the property name from a property access expression.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of the object containing the property.
    /// </typeparam>
    /// <typeparam name="TProperty">
    /// The type of the property.
    /// </typeparam>
    /// <param name="expression">
    /// The expression representing a property access.
    /// </param>
    /// <returns>
    /// The name of the accessed property.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the expression does not represent a supported property access.
    /// </exception>
    private static string GetPropertyName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression unary &&
            unary.Operand is MemberExpression unaryMember)
            return unaryMember.Member.Name;

        throw new ArgumentException("Expression must be a property access.");
    }
}
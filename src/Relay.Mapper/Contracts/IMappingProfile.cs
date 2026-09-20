using System.Linq.Expressions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.Mapper;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Defines a mapping profile that describes how to project a source object
/// into a destination object using an expression tree.
/// </summary>
public interface IMappingProfile<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    /// <summary>
    /// Creates the expression used to map an instance of <typeparamref name="TSource"/>
    /// to an instance of <typeparamref name="TDestination"/>.
    /// </summary>
    /// <returns>
    /// An expression representing the mapping between the source and destination types.
    /// </returns>
    Expression<Func<TSource, TDestination>> CreateProfile();
}
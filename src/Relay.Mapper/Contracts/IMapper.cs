#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.Mapper;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Defines a strongly typed mapping contract between a source type and a destination type.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapper<in TSource, out TDestination>
    where TSource : class
    where TDestination : class
{
    /// <summary>
    /// Maps a single source object to the destination type.
    /// </summary>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped destination object.</returns>
    TDestination Map(TSource source);

    /// <summary>
    /// Maps a collection of source objects to destination objects.
    /// </summary>
    /// <param name="sources">The source objects to map.</param>
    /// <returns>A sequence of mapped destination objects.</returns>
    IEnumerable<TDestination> Map(IEnumerable<TSource> sources);

    /// <summary>
    /// Projects a queryable source sequence to the destination type.
    /// </summary>
    /// <param name="query">The source query to project.</param>
    /// <returns>A queryable sequence of destination objects.</returns>
    IQueryable<TDestination> ProjectTo(IQueryable<TSource> query);
}

/// <summary>
/// Defines a runtime mapping contract for resolving source and destination types dynamically.
/// </summary>
/// <remarks>
/// Unlike <see cref="IMapper{TSource, TDestination}"/>, this interface does not require
/// the source and destination types to be known when the mapper is resolved.
/// </remarks>
public interface IMapper
{
    /// <summary>
    /// Maps a runtime source object to the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped destination object.</returns>
    TDestination Map<TDestination>(object source)
        where TDestination : class;

    /// <summary>
    /// Projects a strongly typed query to the specified destination type.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="query">The source query to project.</param>
    /// <returns>A queryable sequence of destination objects.</returns>
    IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> query)
        where TSource : class
        where TDestination : class;

    /// <summary>
    /// Projects a runtime query to the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="query">The source query to project.</param>
    /// <returns>A queryable sequence of destination objects.</returns>
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable query)
        where TDestination : class;
}
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.Mapper;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for projecting queryable source objects to destination objects
/// using Relay mapping profiles.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Projects an <see cref="IQueryable{TSource}"/> to <typeparamref name="TDestination"/>
    /// using a strongly typed mapper.
    /// </summary>
    /// <param name="query">The source query to project.</param>
    /// <param name="mapper">The mapper that defines the source-to-destination mapping.</param>
    /// <returns>An <see cref="IQueryable{TDestination}"/> representing the projected query.</returns>
    public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(this IQueryable<TSource> query, IMapper<TSource, TDestination> mapper)
        where TSource : class
        where TDestination : class
    {
        return mapper.ProjectTo(query);
    }

    /// <summary>
    /// Projects an <see cref="IQueryable{TSource}"/> to <typeparamref name="TDestination"/>
    /// using a runtime mapper.
    /// </summary>
    /// <param name="query">The source query to project.</param>
    /// <param name="mapper">The mapper that resolves the mapping profile.</param>
    /// <returns>An <see cref="IQueryable{TDestination}"/> representing the projected query.</returns>
    public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(this IQueryable<TSource> query, IMapper mapper)
        where TSource : class
        where TDestination : class
    {
        return mapper.ProjectTo<TSource, TDestination>(query);
    }

    /// <summary>
    /// Projects a non-generic <see cref="IQueryable"/> to <typeparamref name="TDestination"/>
    /// using a runtime mapper.
    /// </summary>
    /// <param name="query">The source query to project.</param>
    /// <param name="mapper">The mapper that resolves the mapping profile.</param>
    /// <returns>An <see cref="IQueryable{TDestination}"/> representing the projected query.</returns>
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable query, IMapper mapper)
        where TDestination : class
    {
        return mapper.ProjectTo<TDestination>(query);
    }
}
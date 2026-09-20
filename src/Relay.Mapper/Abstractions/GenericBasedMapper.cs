namespace Relay.Mapper.Abstractions;

/// <summary>
/// Provides a base implementation for mapping objects from a source type
/// to a destination type using a strongly typed <see cref="IMappingProfile{TSource, TDestination}"/>.
/// </summary>
/// <typeparam name="TSource">
/// The source type to be mapped.
/// </typeparam>
/// <typeparam name="TDestination">
/// The destination type produced by the mapping.
/// </typeparam>
public abstract class GenericBasedMapper<TSource, TDestination> : IMapper<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    /// <summary>
    /// Maps a single source object to a destination object.
    /// </summary>
    /// <param name="source">
    /// The source object to map.
    /// </param>
    /// <returns>
    /// A mapped instance of <typeparamref name="TDestination"/>,
    /// or <see langword="null"/> when <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    public TDestination Map(TSource source)
    {
        if (source is null)
            return default;

        var profile = FindMappingProfile();

        var compiledProfile = profile.CreateProfile().Compile();

        return compiledProfile(source);
    }

    /// <summary>
    /// Maps a sequence of source objects to destination objects.
    /// </summary>
    /// <param name="sources">
    /// The source objects to map.
    /// </param>
    /// <returns>
    /// A lazily evaluated sequence of mapped destination objects.
    /// </returns>
    public IEnumerable<TDestination> Map(IEnumerable<TSource> sources)
    {
        foreach (var source in sources)
        {
            yield return Map(source);
        }
    }

    /// <summary>
    /// Projects an <see cref="IQueryable{T}"/> source query into an
    /// <see cref="IQueryable{T}"/> of destination objects using the mapping expression.
    /// </summary>
    /// <param name="query">
    /// The source query to project.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> that applies the mapping expression
    /// to the source query.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Map(TSource)"/>, this method does not compile or execute
    /// the mapping expression in memory. The expression is passed to the underlying
    /// query provider, allowing providers such as Entity Framework Core to translate
    /// the projection into a data-source query.
    /// </remarks>
    public IQueryable<TDestination> ProjectTo(IQueryable<TSource> query)
    {
        var profile = FindMappingProfile();

        var profileExpression = profile.CreateProfile();

        return query.Select(profileExpression);
    }

    /// <summary>
    /// Finds the mapping profile responsible for mapping
    /// <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <returns>
    /// The mapping profile used to create the mapping expression.
    /// </returns>
    protected abstract IMappingProfile<TSource, TDestination> FindMappingProfile();
}
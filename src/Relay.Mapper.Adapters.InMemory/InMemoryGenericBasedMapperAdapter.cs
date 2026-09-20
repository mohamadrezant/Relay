namespace Relay.Mapper.Adapters.InMemory;

/// <summary>
/// Provides an in-memory adapter for resolving and executing a strongly typed mapping profile
/// through the application's dependency injection container.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
internal sealed class InMemoryGenericBasedMapperAdapter<TSource, TDestination>(IServiceProvider serviceProvider)
    : GenericBasedMapper<TSource, TDestination>, IMapper<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Resolves the single registered mapping profile for the configured source and destination types.
    /// </summary>
    /// <returns>The mapping profile used to create the source-to-destination mapping expression.</returns>
    /// <exception cref="NotImplementedException">
    /// Thrown when no mapping profile is registered for the configured source and destination types.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when more than one mapping profile is registered for the configured source and destination types.
    /// </exception>
    protected override IMappingProfile<TSource, TDestination> FindMappingProfile()
    {
        var profileName = $"{typeof(IMappingProfile<,>).Name}<{typeof(TSource).Name}, {typeof(TDestination).Name}>";

        var profiles = _serviceProvider.GetServices<IMappingProfile<TSource, TDestination>>() ?? [];

        if (!profiles.Any())
            throw new NotImplementedException(profileName);

        if (profiles.Count() != 1)
            throw new InvalidOperationException($"More than one profile {profileName}");

        return profiles.FirstOrDefault();
    }
}
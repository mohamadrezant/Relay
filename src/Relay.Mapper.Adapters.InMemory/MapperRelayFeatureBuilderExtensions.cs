using Relay.Mapper;
using Relay.Mapper.Adapters.InMemory;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring the Relay Mapper feature with in-memory adapters.
/// </summary>
public static class MapperRelayFeatureBuilderExtensions
{
    /// <summary>
    /// Configures the Mapper feature to use the built-in in-memory generic-based and reflection-based mapper adapters
    /// and automatically registers discovered mapping profiles with singleton lifetime.
    /// </summary>
    /// <param name="featureBuilder">The Mapper feature builder to configure.</param>
    /// <returns>The configured <see cref="MapperRelayFeatureBuilder"/> instance.</returns>
    public static MapperRelayFeatureBuilder UseInMemory(this MapperRelayFeatureBuilder featureBuilder)
    {
        var genericBasedMapper = (typeof(InMemoryGenericBasedMapperAdapter<,>), ServiceLifetime.Singleton);
        var reflectionBasedMapper = (typeof(InMemoryReflectionBasedMapperAdapter), ServiceLifetime.Singleton);

        featureBuilder = featureBuilder
            .Use(genericBasedMapper, reflectionBasedMapper)
            .AddMappingProfileWithSingletonLifetime();

        return featureBuilder;
    }

    /// <summary>
    /// Scans the configured assemblies for mapping profile implementations and registers them
    /// with singleton lifetime.
    /// </summary>
    /// <param name="featureBuilder">The Mapper feature builder to configure.</param>
    /// <returns>The configured <see cref="MapperRelayFeatureBuilder"/> instance.</returns>
    private static MapperRelayFeatureBuilder AddMappingProfileWithSingletonLifetime(this MapperRelayFeatureBuilder featureBuilder)
    {
        var mappingProfileInterfaces = typeof(IMappingProfile<,>);

        var registrations = featureBuilder.Assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type =>
                type is { IsClass: true, IsAbstract: false } &&
                !type.IsGenericTypeDefinition)
            .SelectMany(type => type
                .GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType &&
                    @interface.GetGenericTypeDefinition() == mappingProfileInterfaces)
                .Select(@interface => new
                {
                    MappingProfile = @interface,
                    Implementation = type
                }));

        foreach (var registration in registrations)
        {
            featureBuilder.Services
                .AddSingleton(registration.MappingProfile, registration.Implementation);
        }

        return featureBuilder;
    }
}
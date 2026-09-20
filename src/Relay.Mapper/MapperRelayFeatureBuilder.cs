using Relay.FeatureBuilder.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Relay.Mapper;

/// <summary>
/// Provides configuration methods for registering Relay Mapper implementations
/// and their corresponding service lifetimes.
/// </summary>
public class MapperRelayFeatureBuilder(RelayFeatureBuilder overrides)
    : RelayFeatureBuilder(overrides)
{
    /// <summary>
    /// Registers both generic-based and reflection-based mapper implementations.
    /// </summary>
    /// <param name="genericBasedMapper">The generic-based mapper type and its service lifetime.</param>
    /// <param name="reflectionBasedMapper">The reflection-based mapper type and its service lifetime.</param>
    /// <returns>The current <see cref="MapperRelayFeatureBuilder"/> instance for fluent configuration.</returns>
    public MapperRelayFeatureBuilder Use((Type Type, ServiceLifetime Lifetime) genericBasedMapper, (Type Type, ServiceLifetime Lifetime) reflectionBasedMapper)
    {
        UseGenericBased(genericBasedMapper);

        UseReflectionBased(reflectionBasedMapper);

        return this;
    }

    /// <summary>
    /// Validates and registers a generic-based mapper implementation.
    /// </summary>
    /// <param name="mapper">The mapper type and its service lifetime.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mapper type is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the mapper type does not implement <see cref="IMapper{TSource, TDestination}"/>.
    /// </exception>
    private void UseGenericBased((Type Type, ServiceLifetime Lifetime) mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper.Type);

        var genericMapperInterface = typeof(IMapper<,>);

        var hasGenericMapperInterface = mapper.Type
            .GetInterfaces()
            .Any(@interface => @interface.IsGenericType &&
                 @interface.GetGenericTypeDefinition() == genericMapperInterface);

        if (!hasGenericMapperInterface)
        {
            throw new ArgumentException($"Type '{mapper.Type.FullName}' must implement '{genericMapperInterface.FullName}'.", nameof(mapper));
        }

        Services.Add(new(typeof(IMapper<,>), mapper.Type, mapper.Lifetime));
    }

    /// <summary>
    /// Validates and registers a reflection-based mapper implementation.
    /// </summary>
    /// <param name="mapper">The mapper type and its service lifetime.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the mapper type is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the mapper type does not implement <see cref="IMapper"/>.
    /// </exception>
    private void UseReflectionBased((Type Type, ServiceLifetime Lifetime) mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper.Type);

        var hasReflectionMapperInterface = typeof(IMapper)
            .IsAssignableFrom(mapper.Type);

        if (!hasReflectionMapperInterface)
        {
            throw new ArgumentException($"Type '{mapper.Type.FullName}' must implement '{typeof(IMapper).FullName}'.", nameof(mapper));
        }

        Services.Add(new(typeof(IMapper), mapper.Type, mapper.Lifetime));
    }
}
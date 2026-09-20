using Relay.Mapper;
using Relay.FeatureBuilder.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides dependency injection extensions for configuring the Relay Mapper feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Creates a mapper feature builder from the current Relay feature builder.
    /// </summary>
    /// <typeparam name="TFeatureBuilder">The type of the current Relay feature builder.</typeparam>
    /// <param name="featureBuilder">The Relay feature builder to extend with Mapper configuration.</param>
    /// <returns>A <see cref="MapperRelayFeatureBuilder"/> for configuring the Mapper feature.</returns>
    public static MapperRelayFeatureBuilder Mapper<TFeatureBuilder>(this TFeatureBuilder featureBuilder)
        where TFeatureBuilder : RelayFeatureBuilder
    {
        return new(featureBuilder);
    }
}
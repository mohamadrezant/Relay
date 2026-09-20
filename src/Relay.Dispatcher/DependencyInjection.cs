using Relay.Dispatcher;
using Relay.FeatureBuilder.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring the Relay Dispatcher feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Creates a Dispatcher feature builder based on the specified Relay feature builder.
    /// </summary>
    /// <typeparam name="TFeatureBuilder">The type of the existing Relay feature builder.</typeparam>
    /// <param name="featureBuilder">The existing Relay feature builder.</param>
    /// <returns>A Dispatcher feature builder for configuring Dispatcher services.</returns>
    public static DispatcherRelayFeatureBuilder Dispatcher<TFeatureBuilder>(this TFeatureBuilder featureBuilder)
        where TFeatureBuilder : RelayFeatureBuilder
    {
        return new(featureBuilder);
    }
}
using Relay.Validation;
using Relay.FeatureBuilder.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides dependency injection extension methods for configuring
/// the Relay Validation feature.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Creates a Validation feature builder from the current Relay feature builder.
    /// </summary>
    /// <typeparam name="TFeatureBuilder">
    /// The type of the current Relay feature builder.
    /// </typeparam>
    /// <param name="featureBuilder">
    /// The current Relay feature builder used as the source of the shared
    /// service collection and configured assemblies.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationRelayFeatureBuilder"/> that can be used
    /// to configure the Validation feature.
    /// </returns>
    /// <remarks>
    /// The returned builder preserves the service collection and assembly
    /// configuration of the current feature builder, allowing Validation
    /// configuration to be composed with other Relay features through
    /// the fluent API.
    /// </remarks>
    /// <example>
    /// <code>
    /// services
    ///     .AddRelay("MyApp")
    ///     .Validation()
    ///     .UseInMemory()
    ///     .Done();
    /// </code>
    /// </example>
    public static ValidationRelayFeatureBuilder Validation<TFeatureBuilder>(this TFeatureBuilder featureBuilder)
        where TFeatureBuilder : RelayFeatureBuilder
    {
        return new(featureBuilder);
    }
}
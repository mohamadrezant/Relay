using Microsoft.Extensions.DependencyInjection;
using Relay.FeatureBuilder.Abstractions;
using Relay.Validation.Contracts;

namespace Relay.Validation;

/// <summary>
/// Provides fluent configuration for the Relay Validation feature.
/// </summary>
/// <param name="featureBuilder">
/// The parent Relay feature builder whose service collection and configured
/// assemblies are shared with the Validation feature.
/// </param>
/// <remarks>
/// This builder provides Validation-specific configuration while preserving
/// the shared dependency injection services and assembly discovery settings
/// established by the parent <see cref="RelayFeatureBuilder"/>.
/// </remarks>
public class ValidationRelayFeatureBuilder(RelayFeatureBuilder featureBuilder)
    : RelayFeatureBuilder(featureBuilder)
{
    /// <summary>
    /// Registers a validation resolver implementation with the specified
    /// dependency injection lifetime.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the <see cref="IValidationResolver"/> implementation
    /// to register.
    /// </typeparam>
    /// <param name="lifetime">
    /// The lifetime with which the validation resolver should be registered.
    /// </param>
    /// <returns>
    /// The current <see cref="ValidationRelayFeatureBuilder"/> instance,
    /// enabling further fluent configuration.
    /// </returns>
    /// <remarks>
    /// The implementation is registered against
    /// <see cref="IValidationResolver"/>.
    ///
    /// This method allows Relay to remain independent of a specific
    /// validation resolution mechanism. For example, the In-Memory adapter
    /// can provide an implementation based on the application's
    /// <see cref="IServiceProvider"/>.
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
    public ValidationRelayFeatureBuilder Use<T>(ServiceLifetime lifetime)
        where T : class, IValidationResolver
    {
        Services.Add(new(typeof(IValidationResolver), typeof(T), lifetime));

        return this;
    }
}
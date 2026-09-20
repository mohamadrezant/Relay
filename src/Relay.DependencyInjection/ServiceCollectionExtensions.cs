using Microsoft.Extensions.DependencyInjection;
using Relay.FeatureBuilder.Abstractions;

namespace Relay.DependencyInjection;

/// <summary>
/// Provides extension methods for registering and configuring Relay
/// services in an application's dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Relay to the specified <see cref="IServiceCollection"/> and
    /// initializes the Relay feature configuration builder.
    /// </summary>
    /// <param name="serviceDescriptors">
    /// The service collection to which Relay services will be registered.
    /// </param>
    /// <param name="assemblyNameFragments">
    /// Assembly name fragments used to identify assemblies for implementation discovery.
    /// Matching is case-insensitive and uses a substring comparison.
    /// </param>
    /// <returns>
    /// A <see cref="RelayFeatureBuilder"/> that can be used to configure
    /// Relay features, such as Dispatcher, Mapper, and Validation.
    /// </returns>
    /// <remarks>
    /// This method initializes the Relay configuration process but does not
    /// register feature-specific services by itself.
    /// Feature registration is performed through the returned builder.
    /// </remarks>
    /// <example>
    /// <code>
    /// services
    ///     .AddRelay("MyApp")
    ///     .Dispatcher()
    ///     .UseInMemory()
    ///     .Mapper()
    ///     .UseInMemory()
    ///     .Validation()
    ///     .UseInMemory()
    ///     .Done();
    /// </code>
    /// </example>
    public static RelayFeatureBuilder AddRelay(this IServiceCollection serviceDescriptors, params string[] assemblyNameFragments)
    {
        return new RelayBuilder(serviceDescriptors, assemblyNameFragments);
    }
}
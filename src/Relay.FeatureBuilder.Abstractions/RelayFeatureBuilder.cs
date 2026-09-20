using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyInjection;

namespace Relay.FeatureBuilder.Abstractions;

/// <summary>
/// Provides the base functionality for configuring Relay features through
/// a shared <see cref="IServiceCollection"/> and a collection of application assemblies.
/// </summary>
/// <remarks>
/// Feature builders use a fluent configuration approach and share the same
/// service collection and assembly list across different Relay features,
/// such as Dispatcher, Mapper, and Validation.
/// </remarks>
public abstract class RelayFeatureBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelayFeatureBuilder"/> class
    /// using the configuration of an existing feature builder.
    /// </summary>
    /// <param name="overrides">
    /// The existing feature builder whose service collection and assemblies
    /// are reused by the new feature builder.
    /// </param>
    /// <remarks>
    /// This constructor enables multiple Relay feature builders to participate
    /// in the same fluent configuration chain without creating a new
    /// <see cref="IServiceCollection"/> or rescanning assemblies.
    /// </remarks>
    protected RelayFeatureBuilder(RelayFeatureBuilder overrides)
    {
        Services = overrides.Services;

        Assemblies = overrides.Assemblies;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayFeatureBuilder"/> class.
    /// </summary>
    /// <param name="services">
    /// The service collection to which Relay services and implementations
    /// will be registered.
    /// </param>
    /// <param name="assemblyNameFragments">
    /// Assembly name fragments used to identify assemblies that should be loaded
    /// for implementation discovery.
    /// </param>
    /// <remarks>
    /// Assemblies are discovered from the application's runtime dependency context.
    /// An assembly is selected when its name contains at least one of the provided
    /// values using a case-insensitive comparison.
    /// </remarks>
    protected RelayFeatureBuilder(IServiceCollection services, string[] assemblyNameFragments)
    {
        Services = services;

        Assemblies = [.. LoadAssemblies(assemblyNameFragments)];
    }

    /// <summary>
    /// Gets the service collection used to register Relay services.
    /// </summary>
    /// <value>
    /// The shared <see cref="IServiceCollection"/> instance used by all
    /// feature builders in the current configuration chain.
    /// </value>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the assemblies selected for implementation discovery.
    /// </summary>
    /// <value>
    /// A read-only collection of assemblies matching the configured
    /// assembly name filters.
    /// </value>
    /// <remarks>
    /// These assemblies are used by Relay adapters to discover implementations
    /// such as command handlers, query handlers, validators, and mapping profiles.
    /// </remarks>
    public IReadOnlyList<Assembly> Assemblies { get; }

    /// <summary>
    /// Loads application assemblies from the runtime dependency context.
    /// </summary>
    /// <param name="assemblyNameFragments">
    /// Assembly name fragments used to filter runtime libraries.
    /// </param>
    /// <returns>
    /// An enumerable sequence of assemblies whose names contain at least one
    /// of the specified filters.
    /// </returns>
    /// <remarks>
    /// Assembly name matching is case-insensitive and uses
    /// <see cref="string.Contains(string, StringComparison)"/>.
    /// The method loads matching assemblies using <see cref="Assembly.Load(AssemblyName)"/>.
    /// </remarks>
    private static IEnumerable<Assembly> LoadAssemblies(string[] assemblyNameFragments)
    {
        foreach (var library in DependencyContext.Default?.RuntimeLibraries)
        {
            if (assemblyNameFragments.Any(x => library.Name.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                var assembly = Assembly.Load(new AssemblyName(library.Name));

                yield return assembly;
            }
        }
    }

    /// <summary>
    /// Completes the Relay configuration and returns the configured service collection.
    /// </summary>
    /// <returns>
    /// The <see cref="IServiceCollection"/> containing the registered Relay services.
    /// </returns>
    /// <remarks>
    /// This method is intended to terminate the fluent configuration chain
    /// and return the service collection for additional dependency injection
    /// registrations or application startup configuration.
    /// </remarks>
    public IServiceCollection Done()
        => Services;
}
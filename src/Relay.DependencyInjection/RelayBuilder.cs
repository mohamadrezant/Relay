using Microsoft.Extensions.DependencyInjection;
using Relay.FeatureBuilder.Abstractions;

namespace Relay.DependencyInjection;

/// <summary>
/// Provides the root feature builder for configuring Relay services.
/// </summary>
/// <param name="services">
/// The service collection used to register Relay services.
/// </param>
/// <param name="assemblyNameFragments">
/// Assembly name fragments used to identify assemblies for implementation discovery.
/// </param>
/// <remarks>
/// <see cref="RelayBuilder"/> serves as the entry point for Relay's fluent
/// configuration API. It passes the service collection and assembly name
/// filters to the base <see cref="RelayFeatureBuilder"/>.
///
/// Feature-specific configuration can be continued through specialized
/// builders, such as Dispatcher, Mapper, and Validation feature builders.
/// </remarks>
public class RelayBuilder(IServiceCollection services, string[] assemblyNameFragments)
    : RelayFeatureBuilder(services, assemblyNameFragments)
{
}
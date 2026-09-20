using Relay.Dispatcher;
using Relay.Dispatcher.Adapters.InMemory;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring the Relay Dispatcher feature with in-memory adapters.
/// </summary>
public static class DispatcherRelayFeatureBuilderExtensions
{
    /// <summary>
    /// Configures the Dispatcher feature to use the in-memory dispatcher adapter
    /// and automatically registers discovered handlers with scoped lifetime.
    /// </summary>
    /// <param name="featureBuilder">The Dispatcher feature builder to configure.</param>
    /// <returns>The configured <see cref="DispatcherRelayFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="featureBuilder"/> is <see langword="null"/> or when no assemblies
    /// have been configured for handler discovery.
    /// </exception>
    public static DispatcherRelayFeatureBuilder UseInMemory(this DispatcherRelayFeatureBuilder featureBuilder)
    {
        ArgumentNullException.ThrowIfNull(featureBuilder, nameof(featureBuilder));

        if (!featureBuilder.Assemblies.Any())
            throw new ArgumentNullException(nameof(featureBuilder), $"{nameof(featureBuilder.Assemblies)} is required.");

        featureBuilder = featureBuilder
            .Use<InMemoryDispatcherAdapter>(ServiceLifetime.Scoped)
            .AddHandlersWithScopedLifetime();

        return featureBuilder;
    }

    /// <summary>
    /// Scans the configured assemblies for supported Dispatcher handler implementations
    /// and registers them with scoped lifetime.
    /// </summary>
    /// <param name="featureBuilder">The Dispatcher feature builder to configure.</param>
    /// <returns>The configured <see cref="DispatcherRelayFeatureBuilder"/> instance.</returns>
    private static DispatcherRelayFeatureBuilder AddHandlersWithScopedLifetime(this DispatcherRelayFeatureBuilder featureBuilder)
    {
        var handlerInterfaces = new Type[] { typeof(ICommandHandler<,>), typeof(IEventListener<>), typeof(INotificationHandler<>), typeof(IQueryHandler<,>) };

        var registrations = featureBuilder.Assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type =>
                type is { IsClass: true, IsAbstract: false } &&
                !type.IsGenericTypeDefinition)
            .SelectMany(type => type
                .GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType &&
                    handlerInterfaces.Contains(@interface.GetGenericTypeDefinition()))
                .Select(@interface => new
                {
                    Service = @interface,
                    Implementation = type
                }));

        foreach (var registration in registrations)
        {
            featureBuilder.Services
                .AddScoped(registration.Service, registration.Implementation);
        }

        return featureBuilder;
    }
}
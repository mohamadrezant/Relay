using Relay.Validation;
using Relay.Validation.Adapters.InMemory;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Relay.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring the Relay Validation feature.
/// </summary>
public static class ValidationRelayFeatureBuilderExtensions
{
    /// <summary>
    /// Configures the Validation feature to use the built-in
    /// dependency injection-based implementation.
    /// </summary>
    /// <param name="featureBuilder">
    /// The Validation feature builder used to configure validation services.
    /// </param>
    /// <returns>
    /// The configured <see cref="ValidationRelayFeatureBuilder"/> instance,
    /// enabling further fluent configuration.
    /// </returns>
    /// <remarks>
    /// This method registers the validation resolver with a scoped lifetime
    /// and discovers and registers validators from the configured assemblies
    /// with scoped lifetimes.
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
    public static ValidationRelayFeatureBuilder UseInMemory(this ValidationRelayFeatureBuilder featureBuilder)
    {
        featureBuilder = featureBuilder
            .Use<InMemoryValidationResolverAdapter>(ServiceLifetime.Scoped)
            .AddValidatorWithScopedLifetime();

        return featureBuilder;
    }

    /// <summary>
    /// Discovers and registers validators from the configured assemblies
    /// using a scoped service lifetime.
    /// </summary>
    /// <param name="featureBuilder">
    /// The Validation feature builder containing the assemblies to scan
    /// and the service collection used for registration.
    /// </param>
    /// <returns>
    /// The same <see cref="ValidationRelayFeatureBuilder"/> instance
    /// after validator registration is completed.
    /// </returns>
    /// <remarks>
    /// This method scans the configured assemblies for concrete, non-abstract,
    /// non-generic-type-definition classes that implement
    /// <see cref="IValidator{TCommand}"/>.
    ///
    /// Each discovered validator is registered using its corresponding
    /// closed generic validator interface and its implementation type.
    /// </remarks>
    private static ValidationRelayFeatureBuilder AddValidatorWithScopedLifetime(this ValidationRelayFeatureBuilder featureBuilder)
    {
        var validatorInterfaces = typeof(IValidator<>);

        var registrations = featureBuilder.Assemblies
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(static type =>
                type is { IsClass: true, IsAbstract: false } &&
                !type.IsGenericTypeDefinition)
            .SelectMany(type => type
                .GetInterfaces()
                .Where(@interface =>
                    @interface.IsGenericType &&
                    @interface.GetGenericTypeDefinition() == validatorInterfaces)
                .Select(@interface => new
                {
                    Validator = @interface,
                    Implementation = type
                }));

        foreach (var registration in registrations)
        {
            featureBuilder.Services
                .AddScoped(registration.Validator, registration.Implementation);
        }

        return featureBuilder;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Relay.FeatureBuilder.Abstractions;

namespace Relay.Dispatcher;

/// <summary>
/// Provides fluent configuration for the Relay Dispatcher feature, including dispatcher,
/// pipeline behavior, and Outbox store registrations.
/// </summary>
public class DispatcherRelayFeatureBuilder(RelayFeatureBuilder overrides)
    : RelayFeatureBuilder(overrides)
{
    /// <summary>
    /// Registers the specified dispatcher implementation with the configured service lifetime.
    /// </summary>
    /// <typeparam name="T">The dispatcher implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime used to register the dispatcher.</param>
    /// <returns>The current Dispatcher feature builder.</returns>
    public DispatcherRelayFeatureBuilder Use<T>(ServiceLifetime lifetime)
        where T : class, IDispatcher
    {
        Services.Add(new(typeof(IDispatcher), typeof(T), lifetime));

        return this;
    }

    /// <summary>
    /// Registers the specified pipeline behavior implementation with the configured service lifetime.
    /// Determines whether the implementation handles requests with or without a response and registers
    /// it against the corresponding open generic pipeline behavior interface.
    /// </summary>
    /// <param name="pipelineType">The pipeline behavior implementation type.</param>
    /// <param name="lifetime">The service lifetime used to register the pipeline behavior.</param>
    /// <returns>The current Dispatcher feature builder.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="pipelineType"/> does not implement
    /// <see cref="IPipelineBehavior{TRequest}"/> or
    /// <see cref="IPipelineBehavior{TRequest, TResponse}"/>.
    /// </exception>
    public DispatcherRelayFeatureBuilder UsePipeline(Type pipelineType, ServiceLifetime lifetime)
    {
        var pipelineInterfaces = pipelineType
            .GetInterfaces()
            .Where(@interface => @interface.IsGenericType)
            .Select(@interface => @interface.GetGenericTypeDefinition());

        if (pipelineInterfaces.Contains(typeof(IPipelineBehavior<>)))
        {
            Services.Add(new(typeof(IPipelineBehavior<>), pipelineType, lifetime));
        }
        else if (pipelineInterfaces.Contains(typeof(IPipelineBehavior<,>)))
        {
            Services.Add(new(typeof(IPipelineBehavior<,>), pipelineType, lifetime));
        }
        else
        {
            throw new ArgumentException($"Type '{pipelineType.FullName}' must implement '{nameof(IPipelineBehavior<>)}<>' or '{nameof(IPipelineBehavior<,>)}<,>'.", nameof(pipelineType));
        }

        return this;
    }

    /// <summary>
    /// Registers the specified Outbox store implementation with the configured service lifetime.
    /// </summary>
    /// <typeparam name="T">The Outbox store implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime used to register the Outbox store.</param>
    /// <returns>The current Dispatcher feature builder.</returns>
    public DispatcherRelayFeatureBuilder UseOutbox<T>(ServiceLifetime lifetime)
        where T : class, IOutboxStore
    {
        Services.Add(new(typeof(IOutboxStore), typeof(T), lifetime));

        return this;
    }
}
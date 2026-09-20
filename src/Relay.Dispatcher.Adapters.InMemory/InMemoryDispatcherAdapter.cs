namespace Relay.Dispatcher.Adapters.InMemory;

/// <summary>
/// Provides an in-memory implementation of the Dispatcher that resolves handlers,
/// pipeline behaviors, and the Outbox store from the application's dependency injection container.
/// </summary>
internal sealed class InMemoryDispatcherAdapter(IServiceProvider serviceProvider)
    : AbstractDispatcher, IDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Resolves the registered query handler for the specified query and result types.
    /// </summary>
    /// <returns>The resolved query handler.</returns>
    /// <exception cref="HandlerNotFoundException{TQuery,TResult}">
    /// Thrown when no query handler is registered for the specified query and result types.
    /// </exception>
    protected override IQueryHandler<TQuery, TResult> FindQueryHandler<TQuery, TResult>()
    {
        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>()
            ?? throw new HandlerNotFoundException<TQuery, TResult>();

        return handler;
    }

    /// <summary>
    /// Resolves the registered command handler for the specified command and response types.
    /// </summary>
    /// <returns>The resolved command handler.</returns>
    /// <exception cref="HandlerNotFoundException{TCommand,TResponse}">
    /// Thrown when no command handler is registered for the specified command and response types.
    /// </exception>
    protected override ICommandHandler<TCommand, TResponse> FindCommandHandler<TCommand, TResponse>()
    {
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResponse>>()
            ?? throw new HandlerNotFoundException<TCommand, TResponse>();

        return handler;
    }

    /// <summary>
    /// Resolves all registered notification handlers for the specified notification type.
    /// </summary>
    /// <returns>The registered notification handlers.</returns>
    protected override IEnumerable<INotificationHandler<TNotification>> FindNotificationHandlers<TNotification>()
    {
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>() ?? [];

        return handlers;
    }

    /// <summary>
    /// Resolves all registered event listeners for the specified event type.
    /// </summary>
    /// <returns>The registered event listeners.</returns>
    protected override IEnumerable<IEventListener<TEvent>> FindEventListeners<TEvent>()
    {
        var listeners = _serviceProvider.GetServices<IEventListener<TEvent>>() ?? [];

        return listeners;
    }

    /// <summary>
    /// Resolves all registered request-only pipeline behaviors for the specified request type.
    /// </summary>
    /// <returns>The registered pipeline behaviors.</returns>
    protected override IEnumerable<IPipelineBehavior<TRequest>> FindPipelineBehaviors<TRequest>()
    {
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest>>() ?? [];

        return behaviors;
    }

    /// <summary>
    /// Resolves all registered request/response pipeline behaviors for the specified request and response types.
    /// </summary>
    /// <returns>The registered pipeline behaviors.</returns>
    protected override IEnumerable<IPipelineBehavior<TRequest, TResponse>> FindPipelineBehaviors<TRequest, TResponse>()
    {
        var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>() ?? [];

        return behaviors;
    }

    /// <summary>
    /// Resolves the registered Outbox store from the dependency injection container.
    /// </summary>
    /// <returns>The resolved Outbox store.</returns>
    /// <exception cref="OutboxStoreNotFoundException">
    /// Thrown when no Outbox store is registered.
    /// </exception>
    protected override IOutboxStore FindRequiredOutboxStore()
    {
        var outboxStore = _serviceProvider.GetService<IOutboxStore>()
            ?? throw new OutboxStoreNotFoundException();

        return outboxStore;
    }
}
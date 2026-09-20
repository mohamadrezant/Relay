namespace Relay.Dispatcher.Abstractions;

/// <summary>
/// Provides the core implementation of the Dispatcher responsible for executing commands and queries,
/// publishing notifications, raising events, invoking pipeline behaviors, and pushing messages to the Outbox.
/// </summary>
public abstract class Dispatcher : IDispatcher
{
    /// <summary>
    /// Sends a query to its corresponding handler through the configured pipeline behaviors.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The result produced by the query handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="HandlerNotFoundException{TQuery,TResult}">Thrown when no handler is found for the query.</exception>
    public async Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        var handler = FindQueryHandler<TQuery, TResult>()
            ?? throw new HandlerNotFoundException<TQuery, TResult>();

        return await InvokePipeline(query, (ctx) => handler.InvokeAsync(query, ctx), handler, cancellationToken);
    }

    /// <summary>
    /// Sends a command to its corresponding handler through the configured pipeline behaviors.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the command response.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The response produced by the command handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
    /// <exception cref="HandlerNotFoundException{TCommand,TResponse}">Thrown when no handler is found for the command.</exception>
    public async Task<TResponse> SendCommandAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
        where TResponse : notnull
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        var handler = FindCommandHandler<TCommand, TResponse>()
            ?? throw new HandlerNotFoundException<TCommand, TResponse>();

        return await InvokePipeline(command, (ctx) => handler.InvokeAsync(command, ctx), handler, cancellationToken);
    }

    /// <summary>
    /// Publishes a notification to its registered handlers according to their configured invocation modes.
    /// Sequential handlers are invoked one by one, parallel handlers are invoked concurrently,
    /// and Outbox handlers cause the notification to be pushed to the configured Outbox store.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is <see langword="null"/>.</exception>
    public async Task PublishNotificationAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var handlers = FindNotificationHandlers<TNotification>();

        var sequentialHandlers = handlers
            .Where(l => l.InvocationMode == InvocationMode.Sequential);

        foreach (var handler in sequentialHandlers)
        {
            await InvokePipeline(notification,
                (ctx) => handler.InvokeAsync(notification, ctx),
                handler,
                cancellationToken);
        }


        var parallelHandlers = handlers
            .Where(l => l.InvocationMode == InvocationMode.Parallel);

        var parallelTasks = parallelHandlers.Select(async handler =>
        {
            try
            {
                await InvokePipeline(notification,
                    ctx => handler.InvokeAsync(notification, ctx),
                    handler,
                    cancellationToken);
            }
            // log and rollback will be apply in behaviors
            catch
            { }
        });

        await Task.WhenAll(parallelTasks);


        var hasAnyOutboxHandler = handlers
            .Any(l => l.InvocationMode == InvocationMode.OutBox);

        if (hasAnyOutboxHandler)
        {
            await PushToOutBoxAsync(notification, cancellationToken);
        }
    }

    /// <summary>
    /// Raises an event to its registered listeners according to their configured invocation modes.
    /// Sequential listeners are invoked one by one, parallel listeners are invoked concurrently,
    /// and Outbox listeners cause the event to be pushed to the configured Outbox store.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="raisedEvent">The event to raise.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="raisedEvent"/> is <see langword="null"/>.</exception>
    public async Task RaiseEventAsync<TEvent>(TEvent raisedEvent, CancellationToken cancellationToken)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(raisedEvent, nameof(raisedEvent));

        var listeners = FindEventListeners<TEvent>();

        var sequentialListeners = listeners
            .Where(l => l.InvocationMode == InvocationMode.Sequential);

        foreach (var listener in sequentialListeners)
        {
            try
            {
                await InvokePipeline(raisedEvent,
                    (ctx) => listener.InvokeAsync(raisedEvent, ctx),
                    listener,
                    cancellationToken);
            }
            catch
            {
                throw;
            }
        }


        var parallelListeners = listeners
            .Where(l => l.InvocationMode == InvocationMode.Parallel);

        var parallelTasks = parallelListeners.Select(async listener =>
        {
            try
            {
                await InvokePipeline(raisedEvent,
                    ctx => listener.InvokeAsync(raisedEvent, ctx),
                    listener,
                    cancellationToken);
            }
            // log and rollback will be apply in behaviors
            catch
            { }
        });

        await Task.WhenAll(parallelTasks);


        var hasAnyOutboxListener = listeners
            .Any(l => l.InvocationMode == InvocationMode.OutBox);

        if (hasAnyOutboxListener)
        {
            await PushToOutBoxAsync(raisedEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Builds and executes the request/response pipeline by composing registered behaviors
    /// around the specified handler delegate.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request being processed.</param>
    /// <param name="handlerDelegate">The delegate that invokes the underlying handler.</param>
    /// <param name="handler">The handler associated with the request.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The response produced by the composed pipeline.</returns>
    private async Task<TResponse> InvokePipeline<TRequest, TResponse>(TRequest request, Func<CancellationToken, Task<TResponse>> handlerDelegate, IHandler handler, CancellationToken cancellationToken)
    {
        var behaviors = FindPipelineBehaviors<TRequest, TResponse>()
            .OrderByDescending(b => b.Order);

        var next = handlerDelegate;

        foreach (var behavior in behaviors)
        {
            var nextCopy = next;
            next = (ct) => behavior.Handle(request, nextCopy, handler, ct);
        }

        return await next(cancellationToken);
    }

    /// <summary>
    /// Builds and executes the request-only pipeline by composing registered behaviors
    /// around the specified handler delegate.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <param name="request">The request being processed.</param>
    /// <param name="handlerDelegate">The delegate that invokes the underlying handler.</param>
    /// <param name="handler">The handler associated with the request.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    private async Task InvokePipeline<TRequest>(TRequest request, Func<CancellationToken, Task> handlerDelegate, IHandler handler, CancellationToken cancellationToken)
        where TRequest : notnull
    {
        var behaviors = FindPipelineBehaviors<TRequest>()
            .OrderByDescending(b => b.Order);

        var next = handlerDelegate;

        foreach (var behavior in behaviors)
        {
            var nextCopy = next;
            next = (ct) => behavior.Handle(request, nextCopy, handler, ct);
        }

        await next(cancellationToken);
    }

    /// <summary>
    /// Creates an Outbox context for the specified payload and pushes it to the required Outbox store.
    /// </summary>
    /// <typeparam name="T">The type of the Outbox payload.</typeparam>
    /// <param name="payload">The payload to store in the Outbox.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    private async Task PushToOutBoxAsync<T>(T payload, CancellationToken cancellationToken)
        where T : notnull
    {
        var context = OutboxContext.Create(payload);

        var outboxStore = FindRequiredOutboxStore();

        await outboxStore?.PushAsync(context, cancellationToken);
    }


    /// <summary>
    /// Resolves the handler responsible for processing the specified query.
    /// </summary>
    protected abstract IQueryHandler<TQuery, TResult> FindQueryHandler<TQuery, TResult>()
        where TQuery : IQuery<TResult>
        where TResult : notnull;

    /// <summary>
    /// Resolves the handler responsible for processing the specified command.
    /// </summary>
    protected abstract ICommandHandler<TCommand, TResponse> FindCommandHandler<TCommand, TResponse>()
        where TCommand : ICommand<TResponse>
        where TResponse : notnull;

    /// <summary>
    /// Resolves all handlers registered for the specified notification.
    /// </summary>
    protected abstract IEnumerable<INotificationHandler<TNotification>> FindNotificationHandlers<TNotification>()
        where TNotification : INotification;

    /// <summary>
    /// Resolves all listeners registered for the specified event.
    /// </summary>
    protected abstract IEnumerable<IEventListener<TEvent>> FindEventListeners<TEvent>()
        where TEvent : class;

    /// <summary>
    /// Resolves all request-only pipeline behaviors registered for the specified request type.
    /// </summary>
    protected abstract IEnumerable<IPipelineBehavior<TRequest>> FindPipelineBehaviors<TRequest>()
        where TRequest : notnull;

    /// <summary>
    /// Resolves all request/response pipeline behaviors registered for the specified request and response types.
    /// </summary>
    protected abstract IEnumerable<IPipelineBehavior<TRequest, TResponse>> FindPipelineBehaviors<TRequest, TResponse>()
        where TRequest : notnull
        where TResponse : notnull;

    /// <summary>
    /// Resolves the required Outbox store used to persist Outbox contexts.
    /// </summary>
    protected abstract IOutboxStore FindRequiredOutboxStore();
}
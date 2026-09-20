namespace Relay.Dispatcher.Contracts.Pipelines;

/// <summary>
/// Defines a pipeline behavior that can intercept and process a request
/// before and/or after the next component in the pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of request processed by the pipeline.</typeparam>
public interface IPipelineBehavior<in TRequest>
    where TRequest : notnull
{
    /// <summary>
    /// Gets the execution order of the pipeline behavior.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Handles the request and optionally invokes the next component in the pipeline.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The delegate that invokes the next component in the pipeline.</param>
    /// <param name="handler">The handler associated with the request.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous pipeline operation.</returns>
    Task Handle(TRequest request, Func<CancellationToken, Task> next, IHandler handler, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a pipeline behavior that can intercept and process a request
/// before and/or after the next component in the pipeline and produce a response.
/// </summary>
/// <typeparam name="TRequest">The type of request processed by the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of response produced by the pipeline.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    /// <summary>
    /// Gets the execution order of the pipeline behavior.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Handles the request and optionally invokes the next component in the pipeline.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The delegate that invokes the next component in the pipeline.</param>
    /// <param name="handler">The handler associated with the request.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous pipeline operation and containing the response.</returns>
    Task<TResponse> Handle(TRequest request, Func<CancellationToken, Task<TResponse>> next, IHandler handler, CancellationToken cancellationToken);
}
# Relay

A lightweight and extensible application infrastructure library for .NET.

**Relay** provides a set of reusable abstractions and infrastructure adapters for building applications around:

* CQRS
* Command and Query dispatching
* Notifications
* Application / Domain Events
* Pipeline Behaviors
* Outbox
* Object Mapping
* Expression-based Projection
* Validation
* Dependency Injection
* Assembly-based registration

Relay is designed around a simple idea:

> **Define the abstraction once, then plug in the implementation you need.**

---

# Table of Contents

* [Overview](#overview)
* [Features](#features)
* [Architecture](#architecture)
* [Project Structure](#project-structure)
* [Getting Started](#getting-started)
* [Relay Builder](#relay-builder)
* [Assembly Discovery](#assembly-discovery)
* [Dispatcher](#dispatcher)

  * [Commands](#commands)
  * [Queries](#queries)
  * [Notifications](#notifications)
  * [Events](#events)
  * [Invocation Modes](#invocation-modes)
  * [Pipeline Behaviors](#pipeline-behaviors)
  * [Outbox](#outbox)
* [Mapper](#mapper)

  * [Mapping Profiles](#mapping-profiles)
  * [Object Mapping](#object-mapping)
  * [Collection Mapping](#collection-mapping)
  * [Queryable Projection](#queryable-projection)
* [Validation](#validation)

  * [Validators](#validators)
  * [ValidationResult](#validationresult)
  * [ValidationException](#validationexception)
* [Pagination](#pagination)
* [Dependency Injection](#dependency-injection)
* [In-Memory Adapters](#in-memory-adapters)
* [Extending Relay](#extending-relay)
* [Package Structure](#package-structure)
* [Design Principles](#design-principles)
* [Current Status](#current-status)
* [Roadmap](#roadmap)
* [License](#license)

---

# Overview

Relay is an application infrastructure library for .NET applications that need reusable abstractions around common application-layer concerns.

Instead of coupling application code directly to a specific implementation, Relay separates:

```text
Application
     │
     ▼
Relay Abstraction
     │
     ▼
Adapter
     │
     ▼
Infrastructure
```

For example, the application depends on:

```csharp
IDispatcher
```

while the current implementation can use:

```text
InMemoryDispatcherAdapter
```

The same approach is used for mapping and validation.

---

# Features

## Dispatcher

Relay Dispatcher supports:

* Commands
* Queries
* Notifications
* Events
* Sequential invocation
* Parallel invocation
* Outbox invocation
* Pipeline behaviors
* Handler discovery
* Dependency Injection-based handler resolution

## Mapper

Relay Mapper supports:

* Generic mapping
* Reflection-based mapping
* Mapping profiles
* Collection mapping
* `IQueryable` projection
* Expression-based mapping

## Validation

Relay Validation supports:

* Generic validators
* Validation resolver
* Validation results
* Property-level errors
* Validation exceptions
* Common `NotNull` and `NotFound` helpers

## Infrastructure

Relay provides:

* Feature builders
* Assembly discovery
* Microsoft DI integration
* In-memory adapters

---

# Architecture

Relay is divided into independent features.

```text
                              Relay
                                │
          ┌─────────────────────┼─────────────────────┐
          │                     │                     │
          ▼                     ▼                     ▼
     Dispatcher              Mapper              Validation
          │                     │                     │
          ▼                     ▼                     ▼
       Adapter               Adapter               Adapter
          │                     │                     │
          └─────────────────────┼─────────────────────┘
                                │
                                ▼
                         Application / DI
```

Each feature exposes its own abstractions and builder.

This allows the features to evolve independently.

---

# Project Structure

The current solution contains:

```text
Relay
│
├── src/
│
├── Relay.DependencyInjection
├── Relay.FeatureBuilder.Abstractions
│
├── Dispatcher/
│   ├── Relay.Dispatcher
│   └── Relay.Dispatcher.Adapters.InMemory
│
├── Mapper/
│   ├── Relay.Mapper
│   └── Relay.Mapper.Adapters.InMemory
│
└── Validation/
    ├── Relay.Validation
    └── Relay.Validation.Adapters.InMemory
```

More explicitly:

```text
src/
│
├── Relay.DependencyInjection/
│
├── Relay.FeatureBuilder.Abstractions/
│
├── Dispatcher/
│   ├── Relay.Dispatcher/
│   └── Relay.Dispatcher.Adapters.InMemory/
│
├── Mapper/
│   ├── Relay.Mapper/
│   └── Relay.Mapper.Adapters.InMemory/
│
└── Validation/
    ├── Relay.Validation/
    └── Relay.Validation.Adapters.InMemory/
```

All current projects target:

```text
.NET 10
```

---

# Getting Started

Relay integrates with `IServiceCollection`.

The root entry point is:

```csharp
services.AddRelay(...)
```

For example:

```csharp
builder.Services
    .AddRelay("MyApplication");
```

The argument specifies assembly name fragments used by Relay for assembly discovery.

Features can then be configured from the returned builder.

---

# Relay Builder

The central abstraction is:

```csharp
RelayFeatureBuilder
```

It contains:

```csharp
public IServiceCollection Services { get; }

public IReadOnlyList<Assembly> Assemblies { get; }
```

The builder also provides:

```csharp
public IServiceCollection Done()
    => Services;
```

This allows returning to the underlying `IServiceCollection` after configuring Relay.

For example:

```csharp
var services = new ServiceCollection();

var serviceCollection = services
    .AddRelay("MyApplication")
    .Done();
```

Feature builders inherit from `RelayFeatureBuilder`.

Current feature builders include:

```text
DispatcherRelayFeatureBuilder
MapperRelayFeatureBuilder
ValidationRelayFeatureBuilder
```

---

# Feature Builder Flow

The Relay builder follows this model:

```text
IServiceCollection
       │
       ▼
   AddRelay()
       │
       ▼
 RelayBuilder
       │
       ├───────────────┐
       │               │
       ▼               ▼
 Dispatcher()       Mapper()
       │
       ▼
 Validation()
```

Each feature returns its own builder so that feature-specific configuration can be added.

---

# Assembly Discovery

Relay uses:

```csharp
DependencyContext.Default.RuntimeLibraries
```

to discover runtime libraries.

The assemblies are selected using the values passed to:

```csharp
AddRelay(params string[] assembliesPrefix)
```

For example:

```csharp
services.AddRelay(
    "MyCompany.Application",
    "MyCompany.Domain");
```

Relay searches the runtime libraries and loads matching assemblies.

The currently used matching strategy is case-insensitive `Contains`.

The discovered assemblies are stored in:

```csharp
IReadOnlyList<Assembly>
```

and shared between the feature builders.

This allows Dispatcher, Mapper and Validation to scan the same application assemblies.

---

# Dispatcher

The Dispatcher is responsible for dispatching application messages to their handlers.

The main abstraction is:

```csharp
public interface IDispatcher
{
    Task<TResult> SendQueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
        where TResult : notnull;

    Task<TResponse> SendCommandAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
        where TResponse : notnull;

    Task PublishNotificationAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification;

    Task RaiseEventAsync<TEvent>(
        TEvent raisedEvent,
        CancellationToken cancellationToken)
        where TEvent : class;
}
```

The Dispatcher supports four types of messages:

```text
Command
Query
Notification
Event
```

---

# Commands

A command represents an operation that produces a response and normally changes application state.

Define a command:

```csharp
public sealed record CreateOrderCommand(
    Guid CustomerId,
    decimal Amount)
    : ICommand<Guid>;
```

Create its handler:

```csharp
public sealed class CreateOrderCommandHandler
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public Task<Guid> InvokeAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = Guid.CreateVersion7();

        // Application logic

        return Task.FromResult(orderId);
    }
}
```

Dispatch it:

```csharp
var orderId =
    await dispatcher.SendCommandAsync<
        CreateOrderCommand,
        Guid>(
            new CreateOrderCommand(
                customerId,
                100),
            cancellationToken);
```

A command has a single handler.

---

# Queries

Queries represent read operations.

Define a query:

```csharp
public sealed record GetOrderQuery(
    Guid OrderId)
    : IQuery<OrderDto>;
```

Create its handler:

```csharp
public sealed class GetOrderQueryHandler
    : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<OrderDto> InvokeAsync(
        GetOrderQuery query,
        CancellationToken cancellationToken)
    {
        // Query data

        return ...;
    }
}
```

Dispatch it:

```csharp
var order =
    await dispatcher.SendQueryAsync<
        GetOrderQuery,
        OrderDto>(
            new GetOrderQuery(orderId),
            cancellationToken);
```

Like commands, a query resolves a single handler.

---

# Notifications

Notifications represent messages that can have multiple handlers.

Define a notification:

```csharp
public sealed record OrderCreatedNotification(
    Guid OrderId)
    : INotification;
```

Create handlers:

```csharp
public sealed class SendOrderNotificationHandler
    : INotificationHandler<OrderCreatedNotification>
{
    public Task InvokeAsync(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        // Handle notification

        return Task.CompletedTask;
    }
}
```

Another handler can independently subscribe:

```csharp
public sealed class UpdateStatisticsHandler
    : INotificationHandler<OrderCreatedNotification>
{
    public Task InvokeAsync(
        OrderCreatedNotification notification,
        CancellationToken cancellationToken)
    {
        // Update statistics

        return Task.CompletedTask;
    }
}
```

Publish the notification:

```csharp
await dispatcher.PublishNotificationAsync(
    new OrderCreatedNotification(orderId),
    cancellationToken);
```

All registered notification handlers are discovered and invoked according to their `InvocationMode`.

---

# Events

Relay provides a separate abstraction for events:

```csharp
IEventListener<TEvent>
```

Example:

```csharp
public sealed record OrderCompletedEvent(
    Guid OrderId);
```

Listener:

```csharp
public sealed class OrderCompletedListener
    : IEventListener<OrderCompletedEvent>
{
    public Task InvokeAsync(
        OrderCompletedEvent raisedEvent,
        CancellationToken cancellationToken)
    {
        // Handle event

        return Task.CompletedTask;
    }
}
```

Raise the event:

```csharp
await dispatcher.RaiseEventAsync(
    new OrderCompletedEvent(orderId),
    cancellationToken);
```

Events support:

* Sequential invocation
* Parallel invocation
* Outbox invocation

---

# Invocation Modes

Relay defines:

```csharp
public enum InvocationMode
{
    Sequential = 0,
    Parallel = 1,
    OutBox = 2
}
```

Handlers that support invocation modes implement:

```csharp
IParallelHandler
```

which exposes:

```csharp
InvocationMode InvocationMode { get; }
```

---

## Sequential

Sequential handlers execute one after another.

```text
Handler A
    │
    ▼
Handler B
    │
    ▼
Handler C
```

Use this when the operations should execute sequentially.

Example:

```csharp
public sealed class MyNotificationHandler
    : INotificationHandler<MyNotification>
{
    public InvocationMode InvocationMode
        => InvocationMode.Sequential;

    public Task InvokeAsync(
        MyNotification notification,
        CancellationToken cancellationToken)
    {
        // ...

        return Task.CompletedTask;
    }
}
```

---

# Parallel

Parallel handlers are invoked concurrently.

```text
             ┌── Handler A
             │
Notification ├── Handler B
             │
             └── Handler C
```

Example:

```csharp
public sealed class MyNotificationHandler
    : INotificationHandler<MyNotification>
{
    public InvocationMode InvocationMode
        => InvocationMode.Parallel;

    public Task InvokeAsync(
        MyNotification notification,
        CancellationToken cancellationToken)
    {
        // ...

        return Task.CompletedTask;
    }
}
```

Parallel invocation should only be used when the handlers can safely execute concurrently.

In particular, shared non-thread-safe resources such as the same `DbContext` instance should not be accessed concurrently.

---

# OutBox

The third invocation mode is:

```csharp
InvocationMode.OutBox
```

An OutBox handler causes the payload to be persisted through an `IOutboxStore`.

The Dispatcher creates:

```csharp
OutboxContext
```

and sends it to:

```csharp
IOutboxStore.PushAsync(...)
```

This allows the actual processing to be performed asynchronously by an external worker or infrastructure implementation.

---

# Outbox Store

The abstraction is:

```csharp
public interface IOutboxStore
{
    Task PushAsync(
        OutboxContext context,
        CancellationToken cancellationToken);

    Task<OutboxContext> PopAsync(
        CancellationToken cancellationToken);
}
```

The storage technology is deliberately not specified.

An implementation can store the message in:

* SQL Server
* PostgreSQL
* another relational database
* NoSQL
* another durable storage mechanism

Relay only defines the contract.

---

# OutboxContext

The current Outbox context contains:

```csharp
public record OutboxContext
{
    public Guid Id { get; }

    public DateTime CreatedAtUtc { get; private set; }

    public object Event { get; private set; }

    public INotification Notification { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }
}
```

It can be created using:

```csharp
var context = OutboxContext.Create(payload);
```

The ID is generated using:

```csharp
Guid.CreateVersion7()
```

The payload is stored as either:

```text
Notification
```

or:

```text
Event
```

depending on the payload type.

---

# Pipeline Behaviors

Relay supports pipeline behaviors for cross-cutting concerns.

There are two pipeline contracts.

For operations without a response:

```csharp
public interface IPipelineBehavior<in TRequest>
    where TRequest : notnull
{
    int Order { get; }

    Task Handle(
        TRequest request,
        Func<CancellationToken, Task> next,
        IHandler handler,
        CancellationToken cancellationToken);
}
```

For operations with a response:

```csharp
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    int Order { get; }

    Task<TResponse> Handle(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> next,
        IHandler handler,
        CancellationToken cancellationToken);
}
```

---

# Pipeline Execution

Pipeline behaviors wrap the actual handler.

Conceptually:

```text
Request
   │
   ▼
Behavior
   │
   ▼
Behavior
   │
   ▼
Behavior
   │
   ▼
Handler
```

A behavior can execute logic before and after the next component:

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    public int Order => 100;

    public async Task<TResponse> Handle(
        TRequest request,
        Func<CancellationToken, Task<TResponse>> next,
        IHandler handler,
        CancellationToken cancellationToken)
    {
        // Before

        var response = await next(cancellationToken);

        // After

        return response;
    }
}
```

Typical use cases include:

* Logging
* Validation
* Authorization
* Transactions
* Metrics
* Error handling
* Performance measurement

---

# Pipeline Ordering

Each pipeline behavior defines:

```csharp
int Order
```

Relay orders the behaviors before constructing the pipeline.

The current Dispatcher uses:

```csharp
.OrderByDescending(b => b.Order)
```

when building the pipeline.

Therefore, the effective nesting should be considered when assigning `Order`.

For example:

```text
Order 300
Order 200
Order 100
Handler
```

becomes a nested execution pipeline.

Applications should define a consistent ordering convention for their behaviors.

---

# Mapper

Relay Mapper provides two mapping abstractions:

```csharp
IMapper<TSource, TDestination>
```

and:

```csharp
IMapper
```

The generic mapper is useful when the source and destination types are known at compile time.

The reflection-based mapper is useful when the types are determined at runtime.

---

# Generic Mapper

The generic mapper contract is:

```csharp
public interface IMapper<TSource, TDestination>
    where TSource : class
    where TDestination : class
{
    TDestination Map(TSource source);

    IEnumerable<TDestination> Map(
        IEnumerable<TSource> sources);

    IQueryable<TDestination> ProjectTo(
        IQueryable<TSource> query);
}
```

---

# Mapping Profiles

Mapping rules are represented by:

```csharp
IMappingProfile<TSource, TDestination>
```

Example:

```csharp
public sealed class OrderMappingProfile
    : IMappingProfile<Order, OrderDto>
{
    public Expression<Func<Order, OrderDto>>
        CreateProfile()
    {
        return order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Amount = order.Amount
        };
    }
}
```

The important part is that the mapping is an expression:

```csharp
Expression<Func<TSource, TDestination>>
```

rather than only a delegate.

This allows the same mapping definition to be used for LINQ projection.

---

# Object Mapping

With a generic mapper:

```csharp
var dto = mapper.Map(order);
```

The mapper:

1. Resolves the mapping profile.
2. Creates the mapping expression.
3. Compiles the expression.
4. Executes it against the source.

---

# Collection Mapping

Collections can also be mapped:

```csharp
IEnumerable<OrderDto> dtos =
    mapper.Map(orders);
```

The generic mapper maps each source item using the same mapping profile.

---

# Reflection-Based Mapper

Relay also contains:

```csharp
ReflectionBasedMapper
```

which implements:

```csharp
IMapper
```

It can determine source and destination types at runtime.

For example:

```csharp
IMapper mapper = ...;

var dto =
    mapper.Map<OrderDto>(order);
```

The reflection-based mapper resolves the corresponding:

```text
IMappingProfile<Order, OrderDto>
```

and executes its expression.

---

# Collection Mapping with Reflection

The reflection-based mapper supports common collection targets including:

```text
IEnumerable<T>
ICollection<T>
IReadOnlyCollection<T>
IReadOnlyList<T>
List<T>
HashSet<T>
T[]
```

For example:

```csharp
var orders =
    mapper.Map<List<OrderDto>>(sourceOrders);
```

The mapper determines:

```text
Source element type
        +
Destination element type
        │
        ▼
Mapping Profile
        │
        ▼
Mapped Collection
```

---

# IQueryable Projection

Relay's expression-based profiles also support projection.

Example:

```csharp
var query =
    dbContext.Orders
        .Where(x => x.CustomerId == customerId)
        .ProjectTo<Order, OrderDto>(mapper);
```

The extension method:

```csharp
ProjectTo(...)
```

uses the mapping expression inside `Queryable.Select`.

Conceptually:

```text
IQueryable<Order>
       │
       ▼
Mapping Expression
       │
       ▼
Queryable<OrderDto>
```

This allows LINQ providers such as Entity Framework Core to translate the projection.

---

# Validation

Relay provides a lightweight validation abstraction.

The validator contract is:

```csharp
public interface IValidator<TCommand>
    where TCommand : notnull
{
    Task<ValidationResult> ValidateAsync(
        TCommand command,
        CancellationToken cancellationToken);
}
```

A validator can therefore be implemented without coupling Relay to a specific validation framework.

---

# Validation Resolver

Relay resolves validators through:

```csharp
IValidationResolver
```

The contract is:

```csharp
public interface IValidationResolver
{
    IValidator<TCommand> Resolve<TCommand>(
        TCommand command)
        where TCommand : notnull;
}
```

The current in-memory adapter resolves validators through `IServiceProvider`.

---

# Validators

Example:

```csharp
public sealed class CreateOrderValidator
    : IValidator<CreateOrderCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CustomerId == Guid.Empty)
            return Task.FromResult(
                ValidationResult.NotNull(
                    nameof(command.CustomerId)));

        return Task.FromResult(
            ValidationResult.Success());
    }
}
```

---

# ValidationResult

The result is represented by:

```csharp
ValidationResult
```

A successful result:

```csharp
var result = ValidationResult.Success();
```

A required property:

```csharp
var result =
    ValidationResult.NotNull(
        nameof(command.CustomerId));
```

A not-found result:

```csharp
var result =
    ValidationResult.NotFound(
        nameof(command.CustomerId));
```

The result exposes:

```csharp
bool IsFailure
bool IsValid
string PropertyName
string[] Errors
ValidationException Exception
```

---

# ThrowOnFailure

Validation results can be converted into exceptions:

```csharp
result.ThrowOnFailure();
```

If the result is valid, nothing happens.

If validation failed, the associated `ValidationException` is thrown.

---

# ValidationException

Relay defines:

```csharp
ValidationException
```

which stores property-level errors in its `Data` collection.

For example, a validation error can contain:

```text
CustomerId
    └── Is Required.
```

The exception also supports common factory methods:

```csharp
ValidationException.NotNull(...)
```

and:

```csharp
ValidationException.NotFound(...)
```

---

# Validation Extensions

Relay provides an extension for property-based `NotFound` validation.

Example:

```csharp
var result =
    entity.NotFound(x => x.Id);
```

The property name is extracted from the expression.

For example:

```csharp
x => x.Id
```

produces:

```text
Id
```

---

# Pagination

Relay Dispatcher contracts include reusable pagination types.

Offset pagination:

```csharp
public record PagedQuery<TResult>
    : IQuery<PagedQueryResult<TResult>>
    where TResult : class
{
    public int PageSize { get; set; }

    public int CurrentPage { get; set; }
}
```

`CurrentPage` is documented as starting from `1`.

The result is:

```csharp
public record PagedQueryResult<TResult>
    where TResult : class
{
    public IEnumerable<TResult> Results { get; set; }

    public int TotalCount { get; set; }
}
```

---

# Keyset Pagination

Relay also provides:

```csharp
KeysetPagedQuery<TKey, TResult>
```

Example:

```csharp
public record KeysetPagedQuery<TKey, TResult>
    : IQuery<PagedQueryResult<TResult>>
    where TKey : IComparable<TKey>
    where TResult : class
{
    public int PageSize { get; set; }

    public TKey LastSeenKey { get; set; }
}
```

This is intended for keyset/cursor-style pagination.

Example conceptual query:

```sql
WHERE Id > @LastSeenId
ORDER BY Id
```

instead of relying on large offsets.

---

# Dependency Injection

Relay uses:

```csharp
Microsoft.Extensions.DependencyInjection
```

and exposes feature-specific registration methods.

The root registration is:

```csharp
services.AddRelay(...)
```

The current feature methods are:

```csharp
.Dispatcher()
.Mapper()
.Validation()
```

---

# Dispatcher Registration

The Dispatcher builder provides:

```csharp
Use<T>(ServiceLifetime lifetime)
```

for registering an `IDispatcher`.

It also provides:

```csharp
UsePipeline(...)
```

and:

```csharp
UseOutbox<T>(...)
```

Example:

```csharp
services
    .AddRelay("MyApplication")
    .Dispatcher()
    .UseOutbox<MyOutboxStore>(
        ServiceLifetime.Scoped);
```

---

# Mapper Registration

The Mapper builder supports two implementations:

```text
Generic-based Mapper
Reflection-based Mapper
```

They are configured using:

```csharp
Use(
    (Type Type, ServiceLifetime Lifetime) genericBasedMapper,
    (Type Type, ServiceLifetime Lifetime) reflectionBasedMapper)
```

The builder verifies that the supplied types implement the expected mapper abstractions before registering them.

---

# Validation Registration

The Validation builder provides:

```csharp
Use<T>(ServiceLifetime lifetime)
```

where `T` implements:

```csharp
IValidationResolver
```

Example:

```csharp
services
    .AddRelay("MyApplication")
    .Validation()
    .Use<MyValidationResolver>(
        ServiceLifetime.Scoped);
```

---

# In-Memory Adapters

Relay currently provides in-memory adapters for:

```text
Dispatcher
Mapper
Validation
```

These adapters use:

```csharp
IServiceProvider
```

for runtime resolution.

---

# Dispatcher In-Memory Adapter

The in-memory Dispatcher adapter:

```text
InMemoryDispatcherAdapter
```

resolves:

```text
ICommandHandler<TCommand,TResponse>
IQueryHandler<TQuery,TResult>
INotificationHandler<TNotification>
IEventListener<TEvent>
IPipelineBehavior<TRequest>
IPipelineBehavior<TRequest,TResponse>
IOutboxStore
```

from the Microsoft DI container.

---

# Handler Assembly Scanning

The in-memory Dispatcher adapter scans the configured assemblies for:

```text
ICommandHandler<,>
IQueryHandler<,>
INotificationHandler<>
IEventListener<>
```

Concrete, non-abstract classes are discovered and registered as:

```csharp
ServiceLifetime.Scoped
```

This means an application normally does not need to register every handler manually.

---

# Mapper In-Memory Adapter

The in-memory Mapper adapter provides:

```text
InMemoryGenericBasedMapperAdapter<TSource, TDestination>
```

and:

```text
InMemoryReflectionBasedMapperAdapter
```

Mapping profiles are discovered from the configured assemblies and registered as:

```csharp
ServiceLifetime.Singleton
```

---

# Validation In-Memory Adapter

The in-memory Validation adapter provides:

```text
InMemoryValidationResolverAdapter
```

Validators are discovered from the configured assemblies and registered as:

```csharp
ServiceLifetime.Scoped
```

---

# Typical Configuration

A typical application can configure the available in-memory adapters through their feature extensions.

Conceptually:

```csharp
services
    .AddRelay(
        "MyApplication",
        "MyDomain")
    .Dispatcher()
    .UseInMemory();
```

Mapper:

```csharp
services
    .AddRelay(
        "MyApplication",
        "MyDomain")
    .Mapper()
    .UseInMemory();
```

Validation:

```csharp
services
    .AddRelay(
        "MyApplication",
        "MyDomain")
    .Validation()
    .UseInMemory();
```

After configuring a feature, `Done()` can be used to return to `IServiceCollection`:

```csharp
services
    .AddRelay("MyApplication")
    .Dispatcher()
    .UseInMemory()
    .Done()
    .AddScoped<SomeService>();
```

---

# Extending Relay

Relay is designed so that adapters can be implemented without modifying the core feature.

For example, a custom Dispatcher can inherit:

```csharp
Dispatcher
```

and implement the handler resolution methods.

A custom Outbox only needs:

```csharp
IOutboxStore
```

A custom validation implementation can provide:

```csharp
IValidationResolver
```

A custom mapper can implement:

```csharp
IMapper<TSource, TDestination>
```

or:

```csharp
IMapper
```

This keeps infrastructure-specific concerns outside the Relay contracts.

---

# Custom Dispatcher

A custom Dispatcher can inherit:

```csharp
public abstract class Dispatcher
```

and implement:

```csharp
FindQueryHandler(...)
FindCommandHandler(...)
FindNotificationHandlers(...)
FindEventListeners(...)
FindPipelineBehaviors(...)
FindRequiredOutboxStore(...)
```

The base Dispatcher is responsible for the common dispatching and pipeline behavior.

The adapter is responsible for resolving dependencies.

---

# Custom Outbox

A custom Outbox implementation:

```csharp
public sealed class MyOutboxStore : IOutboxStore
{
    public Task PushAsync(
        OutboxContext context,
        CancellationToken cancellationToken)
    {
        // Persist the message

        return Task.CompletedTask;
    }

    public Task<OutboxContext> PopAsync(
        CancellationToken cancellationToken)
    {
        // Retrieve the next message

        throw new NotImplementedException();
    }
}
```

Register it through:

```csharp
.UseOutbox<MyOutboxStore>(
    ServiceLifetime.Scoped)
```

---

# Custom Mapper

A custom generic mapper can implement:

```csharp
IMapper<TSource, TDestination>
```

while a runtime mapper can implement:

```csharp
IMapper
```

The mapping infrastructure does not require a particular third-party mapping library.

---

# Custom Validation

A custom validation resolver can implement:

```csharp
IValidationResolver
```

and resolve validators however the application requires.

This makes it possible to integrate another validation system while keeping application code dependent on Relay's abstraction.

---

# Error Handling

Relay defines dedicated exceptions for infrastructure-level failures.

## HandlerNotFoundException

Thrown when a command or query handler cannot be resolved.

```text
HandlerNotFoundException<TRequest,TResponse>
```

## OutboxStoreNotFoundException

Thrown when an OutBox invocation is requested but no `IOutboxStore` is available.

Applications can handle these exceptions at their own boundary.

---

# CQRS Model

Relay fits naturally into a CQRS-based architecture.

```text
                 Application
                     │
             ┌───────┴───────┐
             │               │
             ▼               ▼
          Commands         Queries
             │               │
             ▼               ▼
          Handler          Handler
             │               │
             ▼               ▼
        Write Model       Read Model
```

Notifications and events can be used alongside commands:

```text
Command
   │
   ▼
Command Handler
   │
   ├──────────► Notification
   │
   └──────────► Event
```

Pipeline behaviors can then wrap the execution:

```text
Request
   │
   ▼
Pipeline
   │
   ▼
Handler
```

---

# Design Principles

Relay is built around several principles.

## 1. Abstraction First

The core packages define contracts rather than infrastructure implementations.

## 2. Adapter-Based Architecture

Infrastructure implementations live in adapter packages.

## 3. Feature Isolation

Dispatcher, Mapper and Validation are independent features.

## 4. Dependency Injection

Microsoft DI is used as the default integration mechanism.

## 5. Assembly Discovery

Common application registrations can be discovered automatically.

## 6. Expression-Based Mapping

Mapping profiles are expressions so they can be used for LINQ projection.

## 7. Async First

Application operations use asynchronous APIs and accept `CancellationToken`.

## 8. Extensibility

The main infrastructure components can be replaced with custom implementations.

---

# Package Structure

The current source structure naturally maps to these packages:

```text
Relay.FeatureBuilder.Abstractions
Relay.DependencyInjection

Relay.Dispatcher
Relay.Dispatcher.Adapters.InMemory

Relay.Mapper
Relay.Mapper.Adapters.InMemory

Relay.Validation
Relay.Validation.Adapters.InMemory
```

The dependency direction is:

```text
                 FeatureBuilder.Abstractions
                           ▲
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          │                │                │
      Dispatcher         Mapper         Validation
          ▲                ▲                ▲
          │                │                │
       Adapter          Adapter          Adapter
```

The adapters depend on their corresponding feature package.

---

# Current Status

The current implementation contains:

### Dispatcher

* `IDispatcher`
* Commands
* Queries
* Notifications
* Events
* Pipeline behaviors
* Sequential invocation
* Parallel invocation
* Outbox invocation
* Handler discovery
* In-memory handler resolution
* Outbox abstraction
* Pagination contracts
* Keyset pagination contracts

### Mapper

* `IMapper<TSource, TDestination>`
* `IMapper`
* `IMappingProfile<TSource, TDestination>`
* Generic-based mapper
* Reflection-based mapper
* Collection mapping
* Expression-based mapping
* `IQueryable` projection
* Mapping profile discovery
* In-memory mapper adapter

### Validation

* `IValidator<TCommand>`
* `IValidationResolver`
* `ValidationResult`
* `ValidationException`
* `NotNull`
* `NotFound`
* Expression-based property name extraction
* Validator discovery
* In-memory validation resolver

### Infrastructure

* Feature builders
* Shared assembly discovery
* Microsoft DI integration
* `Done()` builder completion

---

# Roadmap

Potential future Relay packages and integrations include:

```text
Relay.Outbox
Relay.Outbox.EntityFrameworkCore
Relay.Outbox.SqlServer
Relay.Outbox.PostgreSql

Relay.Validation.FluentValidation

Relay.Mapper.Mapster
Relay.Mapper.AutoMapper
```

Other infrastructure integrations can be implemented as separate adapters without changing the core contracts.

---

# Important Implementation Notes

Relay is intentionally lightweight, but several infrastructure decisions belong to the consuming application.

## Parallel handlers

Parallel execution should only be used when handlers are safe to run concurrently.

For example, the same scoped `DbContext` should not be used concurrently by multiple parallel handlers.

## Outbox

Relay defines the Outbox contract but does not prescribe:

* Database schema
* Serialization format
* Retry strategy
* Locking strategy
* Worker implementation
* Dead-letter handling

Those concerns belong to the Outbox implementation.

## Pipeline

Pipeline behavior ordering is controlled by `Order`.

Applications should establish a consistent convention for assigning order values.

## Assembly scanning

The current assembly discovery mechanism uses runtime library names and case-insensitive substring matching.

Applications should therefore choose sufficiently specific assembly names when configuring `AddRelay`.

---

# Example Application Structure

Relay can be used alongside a Clean Architecture / CQRS application such as:

```text
MyApplication
│
├── Domain
│   ├── Entities
│   ├── ValueObjects
│   └── Events
│
├── Application
│   ├── Commands
│   │   ├── CreateOrder
│   │   └── UpdateOrder
│   │
│   ├── Queries
│   │   ├── GetOrder
│   │   └── GetOrders
│   │
│   ├── Notifications
│   ├── Validators
│   └── Mapping
│
├── Infrastructure
│   ├── Persistence
│   └── Outbox
│
└── Presentation
    └── API
```

Relay sits primarily in the Application/Infrastructure boundary:

```text
Application
     │
     ├── Relay.Dispatcher
     ├── Relay.Mapper
     └── Relay.Validation
             │
             ▼
      Infrastructure
```

---

# License

Relay is intended to be distributed under the MIT License.

The repository license file is the authoritative source for the license terms.

---

# Summary

Relay is a modular .NET application infrastructure library focused on reusable abstractions for application-layer development.

Its current architecture is:

```text
                         Relay
                           │
       ┌───────────────────┼───────────────────┐
       │                   │                   │
       ▼                   ▼                   ▼
 Dispatcher              Mapper            Validation
       │                   │                   │
       ▼                   ▼                   ▼
   Commands            Mapping              Validators
   Queries             Profiles
   Notifications       Projection
   Events
   Pipelines
   Outbox
```

The infrastructure is adapter-oriented:

```text
Application
     │
     ▼
Relay Contracts
     │
     ▼
Relay Feature
     │
     ▼
Adapter
     │
     ▼
Infrastructure
```

This makes Relay suitable as a reusable foundation for .NET applications that need CQRS, dispatching, mapping, validation and infrastructure abstractions without tightly coupling application code to a particular implementation.

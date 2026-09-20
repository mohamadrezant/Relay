using System.Linq.Expressions;

namespace Relay.Mapper.Adapters.InMemory;

/// <summary>
/// Provides an in-memory adapter for resolving mapping profiles dynamically
/// through the application's dependency injection container.
/// </summary>
internal sealed class InMemoryReflectionBasedMapperAdapter(IServiceProvider serviceProvider)
    : ReflectionBasedMapper, IMapper
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Resolves the mapping profile for the specified source and destination types,
    /// creates its mapping expression, and returns the resulting profile information.
    /// </summary>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>A <see cref="MappingProfileInfo"/> containing the source type, destination type, and mapping expression.</returns>
    /// <exception cref="NotImplementedException">
    /// Thrown when no mapping profile is registered for the specified source and destination types.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when multiple profiles are registered, the profile does not expose the expected
    /// <c>CreateProfile</c> method, or the method does not return an expression.
    /// </exception>
    protected override MappingProfileInfo FindMappingProfile(Type sourceType, Type destinationType)
    {
        var profileName = $"{typeof(IMappingProfile<,>).Name}<{sourceType.Name}, {destinationType.Name}>";

        var profileTypes = typeof(IMappingProfile<,>).MakeGenericType(sourceType, destinationType);
        var profiles = _serviceProvider.GetServices(profileTypes) ?? [];

        if (!profiles.Any())
            throw new NotImplementedException(profileName);

        if (profiles.Count() != 1)
            throw new InvalidOperationException($"More than one profile {profileName}");

        var profile = profiles.First();

        var createProfileMethodName = nameof(IMappingProfile<,>.CreateProfile);

        var methodInfo = profile.GetType().GetMethod(createProfileMethodName)
            ?? throw new InvalidOperationException($"{createProfileMethodName} method not found in {profile.GetType().Name}");

        if (methodInfo.Invoke(profile, null) is not Expression expression)
            throw new InvalidOperationException($"{createProfileMethodName} method did not return an Expression for {profileName}");

        return new MappingProfileInfo(sourceType, destinationType, expression);
    }
}
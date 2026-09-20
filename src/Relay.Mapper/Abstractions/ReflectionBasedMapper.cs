using System.Collections;
using System.Linq.Expressions;
using Relay.Mapper.Contracts;

namespace Relay.Mapper.Abstractions;

/// <summary>
/// Provides a reflection-based implementation of <see cref="IMapper"/> that resolves
/// mapping profiles at runtime based on source and destination types.
/// </summary>
/// <remarks>
/// This mapper supports mapping single objects and collections, as well as projecting
/// <see cref="IQueryable"/> instances using mapping expressions.
/// </remarks>
public abstract class ReflectionBasedMapper : IMapper
{
    /// <summary>
    /// Maps a source object to the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped destination object, or <see langword="null"/> when the source is null.</returns>
    public TDestination Map<TDestination>(object source)
        where TDestination : class
    {
        if (source is null)
            return default;

        if (source is IEnumerable enumerable && source is not string)
        {
            if (IsCollectionType(typeof(TDestination)))
                return MapCollection<TDestination>(enumerable);

            var firstItem = GetFirstItem(enumerable);

            if (firstItem is null)
                return default;

            return MapSingle<TDestination>(firstItem);
        }

        return MapSingle<TDestination>(source);
    }

    /// <summary>
    /// Maps a single source object to the specified destination type using its mapping profile.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped destination object.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mapping profile, mapping expression, or compiled mapping delegate cannot be resolved.
    /// </exception>
    private TDestination MapSingle<TDestination>(object source)
        where TDestination : class
    {
        if (source is null)
            return default;

        var sourceType = source.GetType();
        var profile = FindMappingProfile<TDestination>(sourceType);

        if (profile?.Expression is null)
        {
            throw new InvalidOperationException($"Mapping profile or expression is null for {sourceType.Name} -> {typeof(TDestination).Name}");
        }

        var expressionType = profile.Expression.GetType();

        var compileMethod = expressionType.GetMethod(nameof(Expression<>.Compile), Type.EmptyTypes)
            ?? throw new InvalidOperationException("Compile method not found");

        var compiledDelegate = compileMethod.Invoke(profile.Expression, null)
            ?? throw new InvalidOperationException("Compile returned null");

        var result = compiledDelegate
            .GetType()
            .GetMethod(nameof(Func<>.Invoke))
            ?.Invoke(compiledDelegate, [source]);

        return result as TDestination;
    }

    /// <summary>
    /// Maps all elements of a source collection to the specified destination collection type.
    /// </summary>
    /// <typeparam name="TDestination">The destination collection type.</typeparam>
    /// <param name="sourceCollection">The source collection to map.</param>
    /// <returns>The mapped destination collection, or <see langword="null"/> when the source collection is null.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the destination element type or mapping profile cannot be determined,
    /// or when the mapping expression cannot be compiled.
    /// </exception>
    private TDestination MapCollection<TDestination>(IEnumerable sourceCollection)
        where TDestination : class
    {
        if (sourceCollection is null)
            return default;

        var destinationType = typeof(TDestination);

        var destinationElementType = GetEnumerableElementType(destinationType)
            ?? throw new InvalidOperationException($"Cannot determine element type of {destinationType.Name}");

        var sourceElementType = GetCollectionElementType(sourceCollection);

        var profile = FindMappingProfile(sourceElementType, destinationElementType);

        if (profile?.Expression is null)
        {
            throw new InvalidOperationException($"Mapping profile not found for {sourceElementType.Name} -> {destinationElementType.Name}");
        }

        var expressionType = profile.Expression.GetType();

        var compileMethod = expressionType.GetMethod(nameof(Expression<>.Compile), Type.EmptyTypes)
            ?? throw new InvalidOperationException("Compile method not found");

        var compiledDelegate = compileMethod.Invoke(profile.Expression, null)
            ?? throw new InvalidOperationException("Compile returned null");

        var resultList = new List<object>();

        foreach (var item in sourceCollection)
        {
            var mappedItem = compiledDelegate
                .GetType()
                .GetMethod(nameof(Func<>.Invoke))
                ?.Invoke(compiledDelegate, [item]);

            resultList.Add(mappedItem);
        }

        return ConvertToTargetCollection<TDestination>(resultList, destinationElementType);
    }

    /// <summary>
    /// Determines whether the specified type represents a supported collection type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true"/> when the type represents an enumerable or array;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(IEnumerable))
            return true;

        if (type.IsArray)
            return true;

        return GetEnumerableElementType(type) is not null;
    }

    /// <summary>
    /// Determines the element type of a runtime collection.
    /// </summary>
    /// <param name="collection">The collection to inspect.</param>
    /// <returns>
    /// The collection element type, or <see cref="object"/> when the element type cannot be determined.
    /// </returns>
    private static Type GetCollectionElementType(IEnumerable collection)
    {
        var collectionType = collection.GetType();

        var elementType = GetEnumerableElementType(collectionType);

        if (elementType is not null)
            return elementType;

        foreach (var item in collection)
        {
            if (item is not null)
                return item.GetType();
        }

        return typeof(object);
    }

    /// <summary>
    /// Converts mapped objects into the requested destination collection type.
    /// </summary>
    /// <typeparam name="TDestination">The destination collection type.</typeparam>
    /// <param name="items">The mapped objects.</param>
    /// <param name="elementType">The destination collection element type.</param>
    /// <returns>An instance of the requested destination collection type.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the requested collection type is not supported.
    /// </exception>
    private static TDestination ConvertToTargetCollection<TDestination>(List<object> items, Type elementType)
        where TDestination : class
    {
        var targetType = typeof(TDestination);

        if (targetType == typeof(IEnumerable))
            return items as TDestination;

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                array.SetValue(items[i], i);
            }

            return array as TDestination;
        }

        if (!targetType.IsGenericType)
            return items as TDestination;

        var genericDefinition = targetType.GetGenericTypeDefinition();

        if (genericDefinition == typeof(IEnumerable<>) ||
            genericDefinition == typeof(ICollection<>) ||
            genericDefinition == typeof(IReadOnlyCollection<>) ||
            genericDefinition == typeof(IReadOnlyList<>) ||
            genericDefinition == typeof(List<>))
        {
            return CreateList<TDestination>(elementType, items);
        }

        if (genericDefinition == typeof(HashSet<>))
        {
            return CreateHashSet<TDestination>(elementType, items);
        }

        throw new InvalidOperationException($"Collection type '{targetType.Name}' is not supported.");
    }

    /// <summary>
    /// Creates a generic list containing the specified mapped objects.
    /// </summary>
    /// <typeparam name="TDestination">The destination collection type.</typeparam>
    /// <param name="elementType">The element type of the destination collection.</param>
    /// <param name="items">The mapped objects to add to the list.</param>
    /// <returns>The created list cast to the destination collection type.</returns>
    private static TDestination CreateList<TDestination>(Type elementType, List<object> items)
        where TDestination : class
    {
        var listType = typeof(List<>).MakeGenericType(elementType);

        var list = Activator.CreateInstance(listType)
            ?? throw new InvalidOperationException($"Unable to create collection of type {listType.Name}");

        var addMethod = listType.GetMethod(nameof(IList.Add))
            ?? throw new InvalidOperationException($"Add method not found on {listType.Name}");

        foreach (var item in items)
        {
            addMethod.Invoke(list, [item]);
        }

        return list as TDestination;
    }

    /// <summary>
    /// Creates a generic hash set containing the specified mapped objects.
    /// </summary>
    /// <typeparam name="TDestination">The destination collection type.</typeparam>
    /// <param name="elementType">The element type of the destination collection.</param>
    /// <param name="items">The mapped objects to add to the hash set.</param>
    /// <returns>The created hash set cast to the destination collection type.</returns>
    private static TDestination CreateHashSet<TDestination>(Type elementType, List<object> items)
        where TDestination : class
    {
        var hashSetType = typeof(HashSet<>).MakeGenericType(elementType);

        var hashSet = Activator.CreateInstance(hashSetType)
            ?? throw new InvalidOperationException($"Unable to create collection of type {hashSetType.Name}");

        var addMethod = hashSetType.GetMethod(nameof(ISet<>.Add))
            ?? throw new InvalidOperationException($"Add method not found on {hashSetType.Name}");

        foreach (var item in items)
        {
            addMethod.Invoke(hashSet, [item]);
        }

        return hashSet as TDestination;
    }

    /// <summary>
    /// Retrieves the first item from an enumerable sequence.
    /// </summary>
    /// <param name="enumerable">The sequence to inspect.</param>
    /// <returns>The first item in the sequence, or <see langword="null"/> when the sequence is empty.</returns>
    private static object GetFirstItem(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            return item;
        }

        return default;
    }

    /// <summary>
    /// Determines the element type represented by the specified enumerable type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// The enumerable element type, or <see langword="null"/> when the type does not represent a generic enumerable.
    /// </returns>
    private static Type GetEnumerableElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType &&
                iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        return default;
    }

    /// <summary>
    /// Projects a strongly typed query to the specified destination type.
    /// </summary>
    /// <typeparam name="TSource">The source element type.</typeparam>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="query">The source query to project.</param>
    /// <returns>A queryable sequence containing the projected destination objects.</returns>
    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> query)
        where TSource : class
        where TDestination : class
    {
        return ProjectTo<TDestination>(query);
    }

    /// <summary>
    /// Projects a runtime query to the specified destination type using its mapping expression.
    /// </summary>
    /// <typeparam name="TDestination">The destination element type.</typeparam>
    /// <param name="query">The source query to project.</param>
    /// <returns>A queryable sequence containing the projected destination objects.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a mapping profile cannot be found for the source and destination types.
    /// </exception>
    /// <remarks>
    /// The mapping expression is added to the query expression tree and is not compiled
    /// in memory, allowing query providers such as Entity Framework Core to translate
    /// the projection into the underlying data source query.
    /// </remarks>
    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable query)
        where TDestination : class
    {
        var profile = FindMappingProfile<TDestination>(query.ElementType);

        if (profile?.Expression is null)
        {
            throw new InvalidOperationException($"Mapping profile not found for {query.ElementType.Name} -> {typeof(TDestination).Name}");
        }

        return query.Provider.CreateQuery<TDestination>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                [query.ElementType, typeof(TDestination)],
                query.Expression,
                Expression.Quote(profile.Expression)
            )
        );
    }

    /// <summary>
    /// Finds the mapping profile for the specified source type and destination type.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="sourceType">The source type.</param>
    /// <returns>The mapping profile information for the requested type pair.</returns>
    private MappingProfileInfo FindMappingProfile<TDestination>(Type sourceType)
        where TDestination : class
    {
        return FindMappingProfile(sourceType, typeof(TDestination));
    }

    /// <summary>
    /// Resolves mapping profile information for a source and destination type pair.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>The mapping profile information used to perform the mapping.</returns>
    protected abstract MappingProfileInfo FindMappingProfile(Type sourceType, Type destinationType);
}
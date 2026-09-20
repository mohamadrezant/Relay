using System.Linq.Expressions;

namespace Relay.Mapper.Contracts;

/// <summary>
/// Contains runtime metadata for a mapping profile, including its source type,
/// destination type, and mapping expression.
/// </summary>
public record MappingProfileInfo(Type Source, Type Destination, Expression Expression);
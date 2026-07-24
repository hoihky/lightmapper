namespace LightMapper;

/// <summary>
/// Maps instances of <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// Implementations are emitted by the source generator for declared pairs.
/// </summary>
public interface ILightMapper<TSource, TDestination>
    where TDestination : notnull
{
    /// <summary>Creates a new destination instance and maps <paramref name="source"/> into it.</summary>
    TDestination Map(TSource source);

    /// <summary>Maps <paramref name="source"/> into an existing <paramref name="destination"/> instance.</summary>
    void MapTo(TSource source, TDestination destination);
}

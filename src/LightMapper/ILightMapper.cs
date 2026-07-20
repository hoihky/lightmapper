namespace LightMapper;

/// <summary>
/// Maps instances of <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
/// Implementations are emitted by the source generator for declared pairs.
/// </summary>
public interface ILightMapper<in TSource, out TDestination>
{
    TDestination Map(TSource source);
}

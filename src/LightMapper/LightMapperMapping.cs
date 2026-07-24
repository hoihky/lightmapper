namespace LightMapper;

/// <summary>
/// Marker interface implemented by generated partial types to customize mapping after automatic member assignment.
/// Implement <see cref="AfterMap"/> in your hand-written partial class.
/// </summary>
/// <typeparam name="TDestination">Destination type for the mapping pair.</typeparam>
public interface ILightMapperAfterMap<in TDestination>
    where TDestination : notnull
{
    /// <summary>Called after generated members are mapped; use for computed or conditional fields.</summary>
    void AfterMap(TDestination destination);
}

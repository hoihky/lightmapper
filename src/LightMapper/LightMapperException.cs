namespace LightMapper;

/// <summary>Thrown when no compile-time mapping exists for the requested type pair.</summary>
public sealed class LightMapperException : InvalidOperationException
{
    public LightMapperException(Type sourceType, Type destinationType)
        : base($"No LightMapper mapping was generated for {sourceType.Name} -> {destinationType.Name}. " +
               "Add [LightMap(typeof(...))] on a partial type and rebuild.")
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    public Type SourceType { get; }
    public Type DestinationType { get; }
}

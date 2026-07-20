namespace LightMapper;

/// <summary>
/// Declares a compile-time mapping from the annotated type to <see cref="DestinationType"/>.
/// When <see cref="Bidirectional"/> is true, the reverse mapping is also generated.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = true)]
public sealed class LightMapAttribute : Attribute
{
    public LightMapAttribute(Type destinationType)
    {
        DestinationType = destinationType;
    }

    /// <summary>Maps from the annotated type to this type.</summary>
    public Type DestinationType { get; }

    /// <summary>When true, also generates mapping from <see cref="DestinationType"/> back to the annotated type.</summary>
    public bool Bidirectional { get; set; }
}

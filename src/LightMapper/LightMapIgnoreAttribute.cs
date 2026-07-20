namespace LightMapper;

/// <summary>Excludes a property from generated mapping.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class LightMapIgnoreAttribute : Attribute
{
}

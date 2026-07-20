namespace LightMapper;

/// <summary>
/// Maps a destination member from a differently named source member.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class LightMapFromAttribute : Attribute
{
    public LightMapFromAttribute(string sourceMemberName)
    {
        SourceMemberName = sourceMemberName;
    }

    public string SourceMemberName { get; }
}

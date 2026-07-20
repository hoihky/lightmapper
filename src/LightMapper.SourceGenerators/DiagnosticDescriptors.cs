using Microsoft.CodeAnalysis;

namespace LightMapper.SourceGenerators;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidLightMapTarget = new(
        id: "LM001",
        title: "Invalid LightMap target",
        messageFormat: "LightMap destination type could not be resolved for '{0}'",
        category: "LightMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatibleMember = new(
        id: "LM002",
        title: "Incompatible mapping member",
        messageFormat: "Cannot map member '{0}' from '{1}' to '{2}'. Declare a [LightMap] between the member types or mark the destination member with [LightMapIgnore]",
        category: "LightMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingSourceMember = new(
        id: "LM003",
        title: "Missing source member",
        messageFormat: "Destination member '{0}' specifies [LightMapFrom(\"{1}\")] but the source type has no accessible member with that name",
        category: "LightMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

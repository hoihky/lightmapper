using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LightMapper.SourceGenerators;

/// <summary>Emits member assignments from source to an existing destination variable.</summary>
internal sealed class MappingMemberEmitter
{
    private static readonly SymbolDisplayFormat FullyQualified = SymbolDisplayFormat.FullyQualifiedFormat;

    private readonly Compilation _compilation;
    private readonly ImmutableHashSet<string> _pairKeys;
    private readonly SourceProductionContext _context;

    public MappingMemberEmitter(
        Compilation compilation,
        ImmutableHashSet<string> pairKeys,
        SourceProductionContext context)
    {
        _compilation = compilation;
        _pairKeys = pairKeys;
        _context = context;
    }

    public void EmitMembers(
        StringBuilder sb,
        MappingDeclaration pair,
        string sourceExpression,
        string targetVariable)
    {
        foreach (var destProp in MappingSymbolExtensions.GetPublicInstanceWritableProperties(pair.Destination))
        {
            if (MappingSymbolExtensions.HasLightMapIgnore(destProp))
                continue;

            var sourceMemberName = MappingSymbolExtensions.GetLightMapFrom(destProp) ?? destProp.Name;
            var sourceProp = MappingSymbolExtensions.FindPublicReadableProperty(pair.Source, sourceMemberName);
            if (sourceProp is null)
            {
                if (MappingSymbolExtensions.GetLightMapFrom(destProp) is not null)
                {
                    _context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.MissingSourceMember,
                        MappingSymbolExtensions.PrimaryLocation(destProp),
                        destProp.Name,
                        sourceMemberName));
                }

                continue;
            }

            if (MappingSymbolExtensions.HasLightMapIgnore(sourceProp))
                continue;

            var assignment = TryBuildAssignment(sourceProp, destProp, sourceExpression);
            if (assignment is not null)
            {
                sb.AppendLine($"            {targetVariable}.{MappingSymbolExtensions.EscapeIdentifier(destProp.Name)} = {assignment};");
                continue;
            }

            if (CollectionMappingEmitter.TryEmit(sb, sourceProp, destProp, sourceExpression, targetVariable, _pairKeys))
                continue;

            _context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.IncompatibleMember,
                MappingSymbolExtensions.PrimaryLocation(destProp),
                destProp.Name,
                sourceProp.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                destProp.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
        }
    }

    private string? TryBuildAssignment(IPropertySymbol sourceProp, IPropertySymbol destProp, string sourceExpression)
    {
        var srcType = sourceProp.Type;
        var dstType = destProp.Type;
        var conversion = _compilation.ClassifyConversion(srcType, dstType);
        var memberAccess = $"{sourceExpression}.{MappingSymbolExtensions.EscapeIdentifier(sourceProp.Name)}";

        if (conversion.Exists && conversion.IsImplicit)
            return memberAccess;

        if (MappingSymbolExtensions.TryGetNamedTypes(srcType, dstType, out var srcNamed, out var dstNamed) &&
            _pairKeys.Contains(MappingSymbolExtensions.PairKey(srcNamed, dstNamed)))
        {
            var core = MappingCodeEmitter.MethodName(srcNamed, dstNamed);
            var call = $"{core}({memberAccess})";

            if (srcType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
                return $"{memberAccess}.HasValue ? {core}({memberAccess}.GetValueOrDefault()) : default!";

            if (!srcNamed.IsValueType && srcType.NullableAnnotation == NullableAnnotation.Annotated)
                return $"{memberAccess} is null ? default! : {call}";

            return call;
        }

        var widen = _compilation.ClassifyConversion(
            TypeSymbolExtensions.StripNullable(srcType),
            TypeSymbolExtensions.StripNullable(dstType));
        if (widen.Exists && widen.IsImplicit)
            return memberAccess;

        return null;
    }
}

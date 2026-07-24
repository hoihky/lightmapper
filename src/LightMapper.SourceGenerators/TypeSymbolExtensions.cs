using Microsoft.CodeAnalysis;

namespace LightMapper.SourceGenerators;

internal static class TypeSymbolExtensions
{
    public static ITypeSymbol StripNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named)
            return named.TypeArguments[0];
        return type;
    }
}

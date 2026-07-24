using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LightMapper.SourceGenerators;

internal static class MappingSymbolExtensions
{
    public static IEnumerable<IPropertySymbol> GetPublicInstanceWritableProperties(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol prop)
                continue;
            if (prop.IsStatic || prop.IsIndexer)
                continue;
            if (prop.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (prop.SetMethod is null)
                continue;
            yield return prop;
        }
    }

    public static IPropertySymbol? FindPublicReadableProperty(INamedTypeSymbol type, string name)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol prop)
                continue;
            if (prop.Name != name)
                continue;
            if (prop.IsStatic || prop.IsIndexer)
                continue;
            if (prop.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (prop.GetMethod is null)
                continue;
            return prop;
        }

        return null;
    }

    public static bool HasLightMapIgnore(IPropertySymbol prop)
    {
        foreach (var attr in prop.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == "LightMapper.LightMapIgnoreAttribute")
                return true;
        }

        return false;
    }

    public static string? GetLightMapFrom(IPropertySymbol prop)
    {
        foreach (var attr in prop.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != "LightMapper.LightMapFromAttribute")
                continue;
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string s)
                return s;
        }

        return null;
    }

    public static bool TryGetNamedTypes(
        ITypeSymbol srcType,
        ITypeSymbol dstType,
        out INamedTypeSymbol srcNamed,
        out INamedTypeSymbol dstNamed)
    {
        srcNamed = null!;
        dstNamed = null!;
        var s = TypeSymbolExtensions.StripNullable(srcType);
        var d = TypeSymbolExtensions.StripNullable(dstType);
        if (s is not INamedTypeSymbol sn)
            return false;
        if (d is not INamedTypeSymbol dn)
            return false;
        srcNamed = sn;
        dstNamed = dn;
        return true;
    }

    public static Location PrimaryLocation(ISymbol symbol) =>
        symbol.Locations.IsEmpty ? Location.None : symbol.Locations[0];

    public static string PairKey(INamedTypeSymbol source, INamedTypeSymbol destination) =>
        source.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " -> " +
        destination.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string EscapeIdentifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
        SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? "@" + name
            : name;

    public static string SanitizeIdentifier(string name)
    {
        var sb = new System.Text.StringBuilder(name.Length);
        foreach (var ch in name)
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else
                sb.Append('_');
        }

        return sb.Length == 0 ? "list" : sb.ToString();
    }
}

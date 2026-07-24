using System;
using Microsoft.CodeAnalysis;

namespace LightMapper.SourceGenerators;

internal enum CollectionShape
{
    Array,
    List,
    ReadOnlyList,
    HashSet,
    Enumerable,
    Collection,
}

internal static class CollectionShapeResolver
{
    public static bool TryGetSequenceShape(ITypeSymbol type, out ITypeSymbol element, out CollectionShape shape)
    {
        element = null!;
        shape = default;
        var t = TypeSymbolExtensions.StripNullable(type);

        if (t is IArrayTypeSymbol arr)
        {
            element = arr.ElementType;
            shape = CollectionShape.Array;
            return true;
        }

        if (t is not INamedTypeSymbol named || !named.IsGenericType || named.TypeArguments.Length != 1)
            return false;

        element = named.TypeArguments[0];

        if (IsConstructedFrom(named, "List`1"))
        {
            shape = CollectionShape.List;
            return true;
        }

        if (IsConstructedFrom(named, "HashSet`1"))
        {
            shape = CollectionShape.HashSet;
            return true;
        }

        if (IsConstructedFrom(named, "IReadOnlyList`1"))
        {
            shape = CollectionShape.ReadOnlyList;
            return true;
        }

        if (IsConstructedFrom(named, "ICollection`1"))
        {
            shape = CollectionShape.Collection;
            return true;
        }

        if (IsConstructedFrom(named, "IEnumerable`1"))
        {
            shape = CollectionShape.Enumerable;
            return true;
        }

        return false;
    }

    public static bool TryGetSourceCountExpression(
        string sourceAccess,
        CollectionShape shape,
        out string? countExpression)
    {
        countExpression = shape switch
        {
            CollectionShape.Array => $"{sourceAccess}.Length",
            CollectionShape.List or CollectionShape.ReadOnlyList or CollectionShape.HashSet
                or CollectionShape.Collection => $"{sourceAccess}.Count",
            CollectionShape.Enumerable => null,
            _ => null,
        };

        return countExpression is not null;
    }

    private static bool IsConstructedFrom(INamedTypeSymbol named, string metadataName) =>
        named.IsGenericType &&
        IsSystemCollectionsGeneric(named) &&
        string.Equals(named.ConstructedFrom.MetadataName, metadataName, StringComparison.Ordinal);

    private static bool IsSystemCollectionsGeneric(INamedTypeSymbol named) =>
        string.Equals(
            named.ConstructedFrom.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            "global::System.Collections.Generic",
            StringComparison.Ordinal);
}

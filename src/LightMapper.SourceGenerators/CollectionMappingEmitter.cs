using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace LightMapper.SourceGenerators;

internal static class CollectionMappingEmitter
{
    private static readonly SymbolDisplayFormat FullyQualified = SymbolDisplayFormat.FullyQualifiedFormat;

    public static bool TryEmit(
        StringBuilder sb,
        IPropertySymbol sourceProp,
        IPropertySymbol destProp,
        string sourceExpression,
        string targetVariable,
        ImmutableHashSet<string> pairKeys)
    {
        if (!CollectionShapeResolver.TryGetSequenceShape(sourceProp.Type, out var srcElem, out var srcShape))
            return false;
        if (!CollectionShapeResolver.TryGetSequenceShape(destProp.Type, out var dstElem, out var dstShape))
            return false;

        if (!TryResolveElementMapping(srcElem, dstElem, pairKeys, out var useMapper, out var mapExpr))
            return false;

        var destPropName = MappingSymbolExtensions.EscapeIdentifier(destProp.Name);
        var sourceAccess = $"{sourceExpression}.{MappingSymbolExtensions.EscapeIdentifier(sourceProp.Name)}";
        var dstElemFq = dstElem.ToDisplayString(FullyQualified);
        var tmp = "__lm_" + MappingSymbolExtensions.SanitizeIdentifier(destProp.Name);

        string MapItemExpr(string item) =>
            useMapper ? $"{mapExpr!}({item})" : item;

        if (dstShape == CollectionShape.Array)
            return EmitToArray(sb, sourceAccess, srcShape, destPropName, targetVariable, dstElemFq, tmp, MapItemExpr);

        if (dstShape == CollectionShape.HashSet)
            return EmitToHashSet(sb, sourceAccess, srcShape, destPropName, targetVariable, dstElemFq, tmp, MapItemExpr);

        return EmitToList(sb, sourceAccess, srcShape, destPropName, targetVariable, dstElemFq, tmp, MapItemExpr);
    }

    private static bool EmitToArray(
        StringBuilder sb,
        string sourceAccess,
        CollectionShape srcShape,
        string destPropName,
        string targetVariable,
        string dstElemFq,
        string tmp,
        Func<string, string> mapItem)
    {
        if (CollectionShapeResolver.TryGetSourceCountExpression(sourceAccess, srcShape, out var countExpr))
        {
            sb.AppendLine($"            if ({sourceAccess} is null)");
            sb.AppendLine($"                {targetVariable}.{destPropName} = global::System.Array.Empty<{dstElemFq}>();");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            if (srcShape == CollectionShape.Array)
            {
                sb.AppendLine($"                var {tmp} = new {dstElemFq}[{countExpr}];");
                sb.AppendLine($"                for (var __lm_i = 0; __lm_i < {tmp}.Length; __lm_i++)");
                sb.AppendLine($"                    {tmp}[__lm_i] = {mapItem($"{sourceAccess}[__lm_i]")};");
            }
            else
            {
                sb.AppendLine($"                var {tmp} = new {dstElemFq}[{countExpr}];");
                sb.AppendLine("                var __lm_i = 0;");
                sb.AppendLine($"                foreach (var __lm_item in {sourceAccess})");
                sb.AppendLine($"                    {tmp}[__lm_i++] = {mapItem("__lm_item")};");
            }

            sb.AppendLine($"                {targetVariable}.{destPropName} = {tmp};");
            sb.AppendLine("            }");
            return true;
        }

        sb.AppendLine($"            if ({sourceAccess} is null)");
        sb.AppendLine($"                {targetVariable}.{destPropName} = global::System.Array.Empty<{dstElemFq}>();");
        sb.AppendLine("            else");
        sb.AppendLine("            {");
        sb.AppendLine($"                var {tmp} = new global::System.Collections.Generic.List<{dstElemFq}>();");
        sb.AppendLine($"                foreach (var __lm_item in {sourceAccess})");
        sb.AppendLine($"                    {tmp}.Add({mapItem("__lm_item")});");
        sb.AppendLine($"                {targetVariable}.{destPropName} = {tmp}.ToArray();");
        sb.AppendLine("            }");
        return true;
    }

    private static bool EmitToHashSet(
        StringBuilder sb,
        string sourceAccess,
        CollectionShape srcShape,
        string destPropName,
        string targetVariable,
        string dstElemFq,
        string tmp,
        Func<string, string> mapItem)
    {
        _ = srcShape;
        sb.AppendLine($"            if ({sourceAccess} is null)");
        sb.AppendLine($"                {targetVariable}.{destPropName} = new global::System.Collections.Generic.HashSet<{dstElemFq}>();");
        sb.AppendLine("            else");
        sb.AppendLine("            {");
        sb.AppendLine($"                var {tmp} = new global::System.Collections.Generic.HashSet<{dstElemFq}>();");
        sb.AppendLine($"                foreach (var __lm_item in {sourceAccess})");
        sb.AppendLine($"                    {tmp}.Add({mapItem("__lm_item")});");
        sb.AppendLine($"                {targetVariable}.{destPropName} = {tmp};");
        sb.AppendLine("            }");
        return true;
    }

    private static bool EmitToList(
        StringBuilder sb,
        string sourceAccess,
        CollectionShape srcShape,
        string destPropName,
        string targetVariable,
        string dstElemFq,
        string tmp,
        Func<string, string> mapItem)
    {
        sb.AppendLine($"            if ({sourceAccess} is null)");
        sb.AppendLine($"                {targetVariable}.{destPropName} = new global::System.Collections.Generic.List<{dstElemFq}>();");
        sb.AppendLine("            else");
        sb.AppendLine("            {");

        if (CollectionShapeResolver.TryGetSourceCountExpression(sourceAccess, srcShape, out var countExpr))
            sb.AppendLine($"                var {tmp} = new global::System.Collections.Generic.List<{dstElemFq}>({countExpr});");
        else
            sb.AppendLine($"                var {tmp} = new global::System.Collections.Generic.List<{dstElemFq}>();");

        sb.AppendLine($"                foreach (var __lm_item in {sourceAccess})");
        sb.AppendLine($"                    {tmp}.Add({mapItem("__lm_item")});");
        sb.AppendLine($"                {targetVariable}.{destPropName} = {tmp};");
        sb.AppendLine("            }");
        return true;
    }

    private static bool TryResolveElementMapping(
        ITypeSymbol srcElem,
        ITypeSymbol dstElem,
        ImmutableHashSet<string> pairKeys,
        out bool useMapper,
        out string? mapExpr)
    {
        useMapper = false;
        mapExpr = null;
        var s = TypeSymbolExtensions.StripNullable(srcElem);
        var d = TypeSymbolExtensions.StripNullable(dstElem);
        if (SymbolEqualityComparer.Default.Equals(s, d))
            return true;

        if (s is INamedTypeSymbol sn && d is INamedTypeSymbol dn && pairKeys.Contains(MappingSymbolExtensions.PairKey(sn, dn)))
        {
            useMapper = true;
            mapExpr = MappingCodeEmitter.MethodName(sn, dn);
            return true;
        }

        return false;
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace LightMapper.SourceGenerators;

internal readonly struct MappingDeclaration
{
    public MappingDeclaration(INamedTypeSymbol source, INamedTypeSymbol destination, Location location)
    {
        Source = source;
        Destination = destination;
        Location = location;
    }

    public INamedTypeSymbol Source { get; }
    public INamedTypeSymbol Destination { get; }
    public Location Location { get; }
}

internal static class MappingDeclarationCollector
{
    public static ImmutableArray<MappingDeclaration> Normalize(ImmutableArray<MappingDeclaration> raw)
    {
        if (raw.IsDefaultOrEmpty)
            return ImmutableArray<MappingDeclaration>.Empty;

        var set = new HashSet<string>(StringComparer.Ordinal);
        var builder = ImmutableArray.CreateBuilder<MappingDeclaration>();
        foreach (var item in raw)
        {
            var key = Key(item.Source, item.Destination);
            if (set.Add(key))
                builder.Add(item);
        }

        return builder.ToImmutable();
    }

    public static ImmutableArray<MappingDeclaration> Collect(
        Compilation compilation,
        ImmutableArray<GeneratorAttributeSyntaxContext> contexts,
        SourceProductionContext production)
    {
        var builder = ImmutableArray.CreateBuilder<MappingDeclaration>();
        foreach (var context in contexts)
        {
            if (context.TargetSymbol is not INamedTypeSymbol sourceType)
                continue;

            if (context.Attributes.IsDefaultOrEmpty)
                continue;

            var attr = context.Attributes[0];
            if (attr.ConstructorArguments.Length < 1)
                continue;

            var arg = attr.ConstructorArguments[0];
            if (arg.Value is not INamedTypeSymbol destinationType)
            {
                production.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidLightMapTarget,
                    attr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None,
                    sourceType.ToDisplayString()));
                continue;
            }

            var bidirectional = false;
            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "Bidirectional" && named.Value.Value is bool b)
                    bidirectional = b;
            }

            builder.Add(new MappingDeclaration(sourceType, destinationType, GetLocation(context)));

            if (bidirectional)
                builder.Add(new MappingDeclaration(destinationType, sourceType, GetLocation(context)));
        }

        return Normalize(builder.ToImmutable());
    }

    private static string Key(INamedTypeSymbol from, INamedTypeSymbol to) =>
        from.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + " -> " +
        to.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static Location GetLocation(GeneratorAttributeSyntaxContext context)
    {
        if (context.Attributes.IsDefaultOrEmpty)
            return Location.None;

        return context.Attributes[0].ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;
    }
}

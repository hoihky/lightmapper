using System.Collections.Immutable;
using LightMapper.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LightMapper.Tests;

public sealed class MappingDeclarationCollectorTests
{
    [Fact]
    public void Normalize_removes_duplicate_source_destination_pairs()
    {
        var compilation = CreateMinimalCompilation();
        var sourceType = compilation.GetTypeByMetadataName("Dup.A")!;
        var destType = compilation.GetTypeByMetadataName("Dup.B")!;
        var loc = Location.None;

        var raw = ImmutableArray.Create(
            new MappingDeclaration(sourceType, destType, loc),
            new MappingDeclaration(sourceType, destType, loc));

        var normalized = MappingDeclarationCollector.Normalize(raw);
        Assert.Single(normalized);
    }

    private static Compilation CreateMinimalCompilation()
    {
        var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        var tree = CSharpSyntaxTree.ParseText(
            """
            namespace Dup;
            public class A { public int X; }
            public class B { public int X; }
            """,
            parse);

        var refs = new List<MetadataReference>(Basic.Reference.Assemblies.Net100.References.All);
        return CSharpCompilation.Create(
            "dup",
            new[] { tree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

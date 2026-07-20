using System.Collections.Immutable;
using LightMapper;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using LightMapper.SourceGenerators;

namespace LightMapper.Tests;

internal static class GeneratorTestDriver
{
    public static (Compilation OutputCompilation, GeneratorDriverRunResult RunResult) Run(
        string source,
        bool addDependencyInjectionReference = false)
    {
        var references = new List<MetadataReference>(Basic.Reference.Assemblies.Net100.References.All);
        references.Add(MetadataReference.CreateFromFile(typeof(LightMapAttribute).Assembly.Location));

        if (addDependencyInjectionReference)
            references.Add(MetadataReference.CreateFromFile(typeof(ServiceCollection).Assembly.Location));

        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview);

        var tree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "LightMapperTestsAssembly",
            syntaxTrees: new[] { tree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        var generator = new LightMapperIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: ImmutableArray.Create(generator.AsSourceGenerator()),
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        return (outputCompilation!, driver.GetRunResult());
    }

    public static string CombineGeneratedSources(GeneratorDriverRunResult runResult)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var result in runResult.Results)
        {
            foreach (var source in result.GeneratedSources)
                sb.AppendLine(source.SyntaxTree.ToString());
        }

        return sb.ToString();
    }

    public static ImmutableArray<Diagnostic> AllGeneratorDiagnostics(GeneratorDriverRunResult runResult)
    {
        var builder = ImmutableArray.CreateBuilder<Diagnostic>();
        foreach (var result in runResult.Results)
            builder.AddRange(result.Diagnostics);

        return builder.ToImmutable();
    }

    public static ImmutableArray<Diagnostic> OutputErrors(Compilation outputCompilation) =>
        outputCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray();
}

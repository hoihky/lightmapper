using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LightMapper.SourceGenerators;

[Generator]
public sealed class LightMapperIncrementalGenerator : IIncrementalGenerator
{
    private const string LightMapMetadataName = "LightMapper.LightMapAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                LightMapMetadataName,
                static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
                static (ctx, _) => ctx)
            .Collect();

        var compilationAndDeclarations = context.CompilationProvider.Combine(declarations);
        var withDi = compilationAndDeclarations.Combine(
            context.CompilationProvider.Select(static (c, _) => HasGenerateDiAttribute(c)));

        context.RegisterSourceOutput(withDi, static (spc, tuple) =>
        {
            var ((compilation, contexts), emitDi) = tuple;
            var collected = MappingDeclarationCollector.Collect(compilation, contexts, spc);
            if (collected.IsDefaultOrEmpty)
                return;

            var emitter = new MappingCodeEmitter(compilation, collected, spc);
            emitter.Emit();

            if (emitDi)
                new DependencyInjectionEmitter(collected).Emit(spc);
        });
    }

    private static bool HasGenerateDiAttribute(Compilation compilation)
    {
        foreach (var attr in compilation.Assembly.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                "global::LightMapper.DependencyInjection.GenerateLightMapperServiceRegistrationsAttribute")
                return true;
        }

        return false;
    }
}

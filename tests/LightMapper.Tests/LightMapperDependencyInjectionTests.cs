namespace LightMapper.Tests;

public sealed class LightMapperDependencyInjectionTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Emits_AddLightMapperMappers_when_assembly_attribute_present()
    {
        var source = Header + """
            using LightMapper.DependencyInjection;
            [assembly: GenerateLightMapperServiceRegistrations]

            namespace Di;

            [LightMap(typeof(PDto))]
            public sealed partial class P
            {
                public int Id { get; set; }
            }

            public sealed partial class PDto
            {
                public int Id { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source, addDependencyInjectionReference: true);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("AddLightMapperMappers", combined);
        Assert.Contains("ServiceCollectionServiceExtensions.AddSingleton", combined);
        Assert.Contains("LightMapper__", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void Does_not_emit_DI_extension_without_assembly_attribute()
    {
        var source = Header + """
            namespace Di2;

            [LightMap(typeof(QDto))]
            public sealed partial class Q
            {
                public int Id { get; set; }
            }

            public sealed partial class QDto
            {
                public int Id { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source, addDependencyInjectionReference: true);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.DoesNotContain("AddLightMapperMappers", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

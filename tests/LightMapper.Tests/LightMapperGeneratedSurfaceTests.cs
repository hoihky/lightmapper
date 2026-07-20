namespace LightMapper.Tests;

public sealed class LightMapperGeneratedSurfaceTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Emits_MapperRegistry_and_ILightMapper_singletons()
    {
        var source = Header + """
            namespace Surf;

            [LightMap(typeof(ZDto))]
            public sealed partial class Z
            {
                public int Id { get; set; }
            }

            public sealed partial class ZDto
            {
                public int Id { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("MapperRegistry", combined);
        Assert.Contains("ILightMapper<", combined);
        Assert.Contains("Instance", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void IReadOnlyList_source_to_list_destination()
    {
        var source = Header + """
            namespace Surf2;

            [LightMap(typeof(PartDto))]
            public sealed partial class Part
            {
                public int N { get; set; }
            }

            public sealed partial class PartDto
            {
                public int N { get; set; }
            }

            [LightMap(typeof(WrapDto))]
            public sealed partial class Wrap
            {
                public IReadOnlyList<Part> Parts { get; set; } = Array.Empty<Part>();
            }

            public sealed partial class WrapDto
            {
                public List<PartDto> Parts { get; set; } = new();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("foreach (var __lm_item in", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

namespace LightMapper.Tests;

public sealed class LightMapperMultiDestinationTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Multiple_destinations_on_one_source_emit_both_maps()
    {
        var source = Header + """
            namespace Multi;

            [LightMap(typeof(DtoA))]
            [LightMap(typeof(DtoB))]
            public sealed partial class Entity
            {
                public int Id { get; set; }
            }

            public sealed partial class DtoA
            {
                public int Id { get; set; }
            }

            public sealed partial class DtoB
            {
                public int Id { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("Map_global__Multi_Entity_to_global__Multi_DtoA", combined);
        Assert.Contains("Map_global__Multi_Entity_to_global__Multi_DtoB", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

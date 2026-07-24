namespace LightMapper.Tests;

public sealed class LightMapperMapToAndHookTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Emits_MapTo_pair_methods_and_generic_MapTo()
    {
        var source = Header + """
            namespace MapTo;

            [LightMap(typeof(Dto))]
            public sealed partial class Entity
            {
                public int N { get; set; }
            }

            public sealed partial class Dto
            {
                public int N { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("MapTo_global__MapTo_Entity_to_global__MapTo_Dto", combined);
        Assert.Contains("internal static void MapTo<TSource, TDestination>", combined);
        Assert.Contains("public static void MapTo<TSource, TDestination>", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void After_map_hook_invokes_ILightMapperAfterMap()
    {
        var source = Header + """
            namespace Hook;

            [LightMap(typeof(Dto))]
            public sealed partial class Entity : ILightMapperAfterMap<Dto>
            {
                public int N { get; set; }

                public void AfterMap(Dto destination) => destination.N *= 2;
            }

            public sealed partial class Dto
            {
                public int N { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("ILightMapperAfterMap<", combined);
        Assert.Contains("AfterMap(", combined);
        Assert.DoesNotContain("if (source is global::LightMapper.ILightMapperAfterMap", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void Emits_public_Maps_facade()
    {
        var source = Header + """
            namespace Facade;

            [LightMap(typeof(Dto))]
            public sealed partial class Entity
            {
                public int N { get; set; }
            }

            public sealed partial class Dto
            {
                public int N { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("public static class Maps", combined);
        Assert.Contains("LightMapDispatch.Map<TSource, TDestination>", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

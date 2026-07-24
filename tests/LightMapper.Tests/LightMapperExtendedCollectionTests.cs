namespace LightMapper.Tests;

public sealed class LightMapperExtendedCollectionTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void IEnumerable_source_to_list_destination()
    {
        var source = Header + """
            namespace Ext.I;

            [LightMap(typeof(EDto))]
            public sealed partial class E
            {
                public int V { get; set; }
            }

            public sealed partial class EDto
            {
                public int V { get; set; }
            }

            [LightMap(typeof(WrapDto))]
            public sealed partial class Wrap
            {
                public IEnumerable<E> Items { get; set; } = Array.Empty<E>();
            }

            public sealed partial class WrapDto
            {
                public List<EDto> Items { get; set; } = new();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("foreach (var __lm_item in", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void List_source_to_HashSet_destination()
    {
        var source = Header + """
            namespace Ext.H;

            [LightMap(typeof(TDto))]
            public sealed partial class T
            {
                public string Key { get; set; } = "";
            }

            public sealed partial class TDto
            {
                public string Key { get; set; } = "";
            }

            [LightMap(typeof(BagDto))]
            public sealed partial class Bag
            {
                public List<T> Items { get; set; } = new();
            }

            public sealed partial class BagDto
            {
                public HashSet<TDto> Items { get; set; } = new();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("HashSet<", combined);
        Assert.Contains(".Add(", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

namespace LightMapper.Tests;

public sealed class LightMapperNestedAndCollectionTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Nested_object_uses_registered_pair()
    {
        var source = Header + """
            namespace Col.N;

            [LightMap(typeof(InnerDto))]
            public sealed partial class Inner
            {
                public int K { get; set; }
            }

            public sealed partial class InnerDto
            {
                public int K { get; set; }
            }

            [LightMap(typeof(OuterDto))]
            public sealed partial class Outer
            {
                public Inner Child { get; set; } = new();
            }

            public sealed partial class OuterDto
            {
                public InnerDto Child { get; set; } = new();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("Child", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void List_to_list_maps_elements()
    {
        var source = Header + """
            namespace Col.L;

            [LightMap(typeof(ItemDto))]
            public sealed partial class Item
            {
                public int Id { get; set; }
            }

            public sealed partial class ItemDto
            {
                public int Id { get; set; }
            }

            [LightMap(typeof(BagDto))]
            public sealed partial class Bag
            {
                public List<Item> Items { get; set; } = new();
            }

            public sealed partial class BagDto
            {
                public List<ItemDto> Items { get; set; } = new();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("foreach (var __lm_item in", combined);
        Assert.Contains(".Add(", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void Array_to_array_maps_elements()
    {
        var source = Header + """
            namespace Col.A;

            [LightMap(typeof(EDto))]
            public sealed partial class E
            {
                public int V { get; set; }
            }

            public sealed partial class EDto
            {
                public int V { get; set; }
            }

            [LightMap(typeof(FDto))]
            public sealed partial class F
            {
                public E[] Items { get; set; } = Array.Empty<E>();
            }

            public sealed partial class FDto
            {
                public EDto[] Items { get; set; } = Array.Empty<EDto>();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("for (var __lm_i = 0;", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void Array_to_IReadOnlyList_maps_elements()
    {
        var source = Header + """
            namespace Col.R;

            [LightMap(typeof(GDto))]
            public sealed partial class G
            {
                public int V { get; set; }
            }

            public sealed partial class GDto
            {
                public int V { get; set; }
            }

            [LightMap(typeof(HDto))]
            public sealed partial class H
            {
                public G[] Items { get; set; } = Array.Empty<G>();
            }

            public sealed partial class HDto
            {
                public IReadOnlyList<GDto> Items { get; set; } = Array.Empty<GDto>();
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("foreach (var __lm_item in", combined);
        Assert.Contains("global::Col.R.GDto", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }
}

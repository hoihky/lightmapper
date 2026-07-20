namespace LightMapper.Tests;

public sealed class LightMapperGeneratorTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void Generates_forward_mapping_and_compiles()
    {
        var source = Header + """
            namespace Gen.A;

            [LightMap(typeof(WidgetDto))]
            public sealed partial class Widget
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public sealed partial class WidgetDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("LightMapDispatch", combined);
        Assert.Contains("Map_", combined);
        Assert.Contains("Widget", combined);
        Assert.Contains("WidgetDto", combined);

        var errors = GeneratorTestDriver.OutputErrors(output);
        Assert.Empty(errors);
    }

    [Fact]
    public void Bidirectional_generates_both_directions()
    {
        var source = Header + """
            namespace Gen.B;

            [LightMap(typeof(BoxDto), Bidirectional = true)]
            public sealed partial class Box
            {
                public int Value { get; set; }
            }

            public sealed partial class BoxDto
            {
                public int Value { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("Map_", combined);
        Assert.Contains("Box", combined);
        Assert.Contains("BoxDto", combined);
        Assert.Equal(1, CountSubstring(combined, "internal static global::Gen.B.BoxDto Map_global__Gen_B_Box_to_global__Gen_B_BoxDto"));
        Assert.Equal(1, CountSubstring(combined, "internal static global::Gen.B.Box Map_global__Gen_B_BoxDto_to_global__Gen_B_Box"));

        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void LightMapIgnore_skips_destination_member()
    {
        var source = Header + """
            namespace Gen.C;

            [LightMap(typeof(IgnoredDto))]
            public sealed partial class IgnoredSource
            {
                public int Keep { get; set; }
                public string Drop { get; set; } = "";
            }

            public sealed partial class IgnoredDto
            {
                public int Keep { get; set; }

                [LightMapIgnore]
                public string Drop { get; set; } = "";
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.DoesNotContain("__target.Drop", combined);
        Assert.Contains("__target.Keep", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void LightMapFrom_uses_alternate_source_member()
    {
        var source = Header + """
            namespace Gen.D;

            [LightMap(typeof(RenameDto))]
            public sealed partial class RenameSource
            {
                public string First { get; set; } = "";
            }

            public sealed partial class RenameDto
            {
                [LightMapFrom("First")]
                public string Given { get; set; } = "";
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);

        Assert.Contains("source.First", combined);
        Assert.Contains("__target.Given", combined);
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    [Fact]
    public void No_lightmap_attributes_produce_no_generated_sources()
    {
        var source = """
            using System;
            namespace Gen.E;
            public sealed class Plain { public int X { get; set; } }
            """;

        var (_, run) = GeneratorTestDriver.Run(source);
        var count = run.Results.Sum(r => r.GeneratedSources.Length);
        Assert.Equal(0, count);
    }

    [Fact]
    public void Duplicate_identical_pairs_are_deduped_single_map_method()
    {
        var source = Header + """
            namespace Gen.F;

            [LightMap(typeof(DupDto))]
            [LightMap(typeof(DupDto))]
            public sealed partial class Dup
            {
                public int N { get; set; }
            }

            public sealed partial class DupDto
            {
                public int N { get; set; }
            }
            """;

        var (output, run) = GeneratorTestDriver.Run(source);
        var combined = GeneratorTestDriver.CombineGeneratedSources(run);
        var needle = "internal static global::Gen.F.DupDto Map_global__Gen_F_Dup_to_global__Gen_F_DupDto";
        Assert.Equal(1, CountSubstring(combined, needle));
        Assert.Empty(GeneratorTestDriver.OutputErrors(output));
    }

    private static int CountSubstring(string haystack, string needle)
    {
        var count = 0;
        var i = 0;
        while ((i = haystack.IndexOf(needle, i, StringComparison.Ordinal)) >= 0)
        {
            count++;
            i += needle.Length;
        }

        return count;
    }
}

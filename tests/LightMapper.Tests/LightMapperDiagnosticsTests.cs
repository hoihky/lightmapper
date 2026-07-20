namespace LightMapper.Tests;

public sealed class LightMapperDiagnosticsTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;

        """;

    [Fact]
    public void LM002_when_property_types_incompatible()
    {
        var source = Header + """
            namespace Diag;

            [LightMap(typeof(BadDto))]
            public sealed partial class BadSource
            {
                public string NotInt { get; set; } = "";
            }

            public sealed partial class BadDto
            {
                public int NotInt { get; set; }
            }
            """;

        var (_, run) = GeneratorTestDriver.Run(source);
        var diags = GeneratorTestDriver.AllGeneratorDiagnostics(run);
        Assert.Contains(diags, d => d.Id == "LM002");
    }

    [Fact]
    public void LM003_when_LightMapFrom_points_to_missing_member()
    {
        var source = Header + """
            namespace Diag2;

            [LightMap(typeof(MissDto))]
            public sealed partial class MissSource
            {
                public int A { get; set; }
            }

            public sealed partial class MissDto
            {
                [LightMapFrom("DoesNotExist")]
                public int B { get; set; }
            }
            """;

        var (_, run) = GeneratorTestDriver.Run(source);
        var diags = GeneratorTestDriver.AllGeneratorDiagnostics(run);
        Assert.Contains(diags, d => d.Id == "LM003");
    }
}

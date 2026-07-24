using System.Reflection;

namespace LightMapper.Tests;

public sealed class LightMapperBehaviorTests
{
    private const string Header = """
        using System;
        using System.Collections.Generic;
        using LightMapper;
        using LightMapper.Generated;

        """;

    [Fact]
    public void Map_and_MapTo_apply_ILightMapperAfterMap()
    {
        var source = Header + """
            namespace Beh;

            [LightMap(typeof(Dto))]
            public sealed partial class Entity : ILightMapperAfterMap<Dto>
            {
                public int N { get; set; }
                public void AfterMap(Dto destination) => destination.N += 10;
            }

            public sealed partial class Dto
            {
                public int N { get; set; }
            }

            public static class Runner
            {
                public static int MapValue()
                {
                    var e = new Entity { N = 5 };
                    return Maps.Map<Entity, Dto>(e).N;
                }

                public static int MapToValue()
                {
                    var e = new Entity { N = 5 };
                    var d = new Dto { N = 0 };
                    Maps.MapTo(e, d);
                    return d.N;
                }
            }
            """;

        var (compilation, _) = GeneratorTestDriver.Run(source);
        var errors = GeneratorTestDriver.OutputErrors(compilation);
        Assert.Empty(errors);

        using var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));

        var assembly = Assembly.Load(stream.ToArray());
        var runner = assembly.GetType("Beh.Runner", throwOnError: true)!;
        var mapResult = (int)runner.GetMethod("MapValue", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, null)!;
        var mapToResult = (int)runner.GetMethod("MapToValue", BindingFlags.Public | BindingFlags.Static)!
            .Invoke(null, null)!;

        Assert.Equal(15, mapResult);
        Assert.Equal(15, mapToResult);
    }
}

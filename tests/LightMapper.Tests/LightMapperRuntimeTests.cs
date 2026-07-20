using LightMapper;

namespace LightMapper.Tests;

public sealed class LightMapperRuntimeTests
{
    [Fact]
    public void LightMapperException_includes_type_names()
    {
        var ex = new LightMapperException(typeof(int), typeof(string));
        Assert.Contains("Int32", ex.Message, StringComparison.Ordinal);
        Assert.Contains("String", ex.Message, StringComparison.Ordinal);
        Assert.Same(typeof(int), ex.SourceType);
        Assert.Same(typeof(string), ex.DestinationType);
    }
}

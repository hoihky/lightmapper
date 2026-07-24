using LightMapper;
using LightMapper.Generated;
using Microsoft.Extensions.DependencyInjection;

namespace LightMapper.Sample;

internal static class Program
{
    private static void Main()
    {
        var order = new Order
        {
            Id = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
            Reference = "ORD-1001",
            Tags = ["priority", "wholesale"],
            Lines =
            [
                new OrderLine { Sku = "SKU-1", Quantity = 2, UnitPrice = 9.99m },
                new OrderLine { Sku = "SKU-2", Quantity = 1, UnitPrice = 4.50m },
            ],
        };

        var dto = Maps.Map<Order, OrderDto>(order);
        Console.WriteLine($"DTO: {dto.Reference} lines={dto.OrderLines.Count}");

        ILightMapper<Order, OrderDto> mapper = MapperRegistry.Get<Order, OrderDto>();
        var dto2 = mapper.Map(order);
        Console.WriteLine($"Mapper: {dto2.Reference}");

        var roundTrip = Maps.Map<OrderDto, Order>(dto2);
        Console.WriteLine($"Round-trip: {roundTrip.Lines.Count} lines, first SKU={roundTrip.Lines[0].Sku}, tags={string.Join(",", roundTrip.Tags)}");

        var services = new ServiceCollection();
        services.AddLightMapperMappers();
        var sp = services.BuildServiceProvider();
        var fromDi = sp.GetRequiredService<ILightMapper<Order, OrderDto>>().Map(order);
        Console.WriteLine($"DI mapper: {fromDi.Reference}, tags={string.Join(",", fromDi.Tags)}");
    }
}

[LightMap(typeof(OrderDto), Bidirectional = true)]
internal sealed partial class Order
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;

    public string[] Tags { get; set; } = [];

    [LightMapFrom("OrderLines")]
    public List<OrderLine> Lines { get; set; } = new();
}

[LightMap(typeof(OrderLineDto), Bidirectional = true)]
internal sealed partial class OrderLine
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

internal sealed partial class OrderDto
{
    [LightMapIgnore]
    public Guid Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    [LightMapFrom("Lines")]
    public List<OrderLineDto> OrderLines { get; set; } = new();
}

internal sealed partial class OrderLineDto
{
    public string Sku { get; set; } = string.Empty;

    [LightMapFrom("Quantity")]
    public int Qty { get; set; }

    public decimal UnitPrice { get; set; }
}

using OrderManagementService.Domain.Models.Orders.History;
using System.Text.Json;

namespace OrderManagementService.Infrastructure.DataAccess.Serializers;

public sealed class OrderHistoryPayloadSerializer
{
    private readonly JsonSerializerOptions _options;

    public OrderHistoryPayloadSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize(OrderHistoryPayload payload)
    {
        return JsonSerializer.Serialize(payload, _options);
    }

    public OrderHistoryPayload Deserialize(string json)
    {
        return JsonSerializer.Deserialize<OrderHistoryPayload>(json, _options)
               ?? throw new InvalidOperationException("Cannot deserialize history payload");
    }
}
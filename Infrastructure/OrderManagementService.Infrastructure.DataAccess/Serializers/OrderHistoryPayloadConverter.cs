using OrderManagementService.Domain.Models.Orders.History;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderManagementService.Infrastructure.DataAccess.Serializers;

public sealed class OrderHistoryPayloadConverter : JsonConverter<OrderHistoryPayload>
{
    public override OrderHistoryPayload? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty("type", out JsonElement typeProp))
        {
            throw new JsonException("OrderHistoryPayload type discriminator is missing");
        }

        string? discriminator = typeProp.GetString();
        string raw = doc.RootElement.GetRawText();

        return discriminator switch
        {
            "created" => JsonSerializer.Deserialize<CreatedOrderHistoryPayload>(raw, options),
            "item_added" => JsonSerializer.Deserialize<ItemAddedHistoryPayload>(raw, options),
            "item_removed" => JsonSerializer.Deserialize<ItemRemovedHistoryPayload>(raw, options),
            "state_changed" => JsonSerializer.Deserialize<StateChangedHistoryPayload>(raw, options),
            _ => throw new JsonException($"Unknown OrderHistoryPayload type '{discriminator}'"),
        };
    }

    public override void Write(Utf8JsonWriter writer, OrderHistoryPayload value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        switch (value)
        {
            case CreatedOrderHistoryPayload created:
                writer.WriteString("type", "created");
                writer.WriteString("createdBy", created.CreatedBy);
                writer.WriteString("createdAt", created.CreatedAt);
                break;

            case ItemAddedHistoryPayload added:
                writer.WriteString("type", "item_added");
                writer.WriteNumber("productId", added.ProductId);
                writer.WriteNumber("quantity", added.Quantity);
                break;

            case ItemRemovedHistoryPayload removed:
                writer.WriteString("type", "item_removed");
                writer.WriteNumber("productId", removed.ProductId);
                writer.WriteNumber("quantity", removed.Quantity);
                break;

            case StateChangedHistoryPayload state:
                writer.WriteString("type", "state_changed");
                writer.WriteString("from", state.From);
                writer.WriteString("to", state.To);
                break;

            default:
                throw new NotSupportedException($"Unsupported OrderHistoryPayload type {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }
}

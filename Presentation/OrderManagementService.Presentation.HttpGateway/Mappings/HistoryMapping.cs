using OrderManagementService.Presentation.HttpGateway.Models;
using Orders;

namespace OrderManagementService.Presentation.HttpGateway.Mappings;

public static class HistoryMapping
{
    public static OrderHistoryItemDto ToDto(this OrderHistoryItem item)
    {
        OrderHistoryPayloadDto payload = item.Payload.PayloadCase switch
        {
            OrderHistoryPayload.PayloadOneofCase.Created => new CreatedOrderHistoryPayloadDto(
                item.Payload.Created.CreatedBy,
                item.Payload.Created.CreatedAt.ToDateTimeOffset()),

            OrderHistoryPayload.PayloadOneofCase.ItemAdded => new ItemAddedOrderHistoryPayloadDto(
                item.Payload.ItemAdded.ProductId,
                item.Payload.ItemAdded.Quantity),

            OrderHistoryPayload.PayloadOneofCase.ItemRemoved => new ItemRemovedOrderHistoryPayloadDto(
                item.Payload.ItemRemoved.ProductId,
                item.Payload.ItemRemoved.Quantity),

            OrderHistoryPayload.PayloadOneofCase.StateChanged => new StateChangedOrderHistoryPayloadDto(
                item.Payload.StateChanged.From,
                item.Payload.StateChanged.To),
            OrderHistoryPayload.PayloadOneofCase.None => throw new NotImplementedException(),
            _ => new CreatedOrderHistoryPayloadDto(string.Empty, DateTimeOffset.UnixEpoch),
        };

        return new OrderHistoryItemDto(
            item.Id,
            item.OrderId,
            item.CreatedAt.ToDateTimeOffset(),
            MapKind(item.Kind),
            payload);
    }

    public static OrderHistoryItemKindDto MapKind(OrderHistoryItemKind kind)
    {
        return kind switch
        {
            OrderHistoryItemKind.Created => OrderHistoryItemKindDto.Created,
            OrderHistoryItemKind.ItemAdded => OrderHistoryItemKindDto.ItemAdded,
            OrderHistoryItemKind.ItemRemoved => OrderHistoryItemKindDto.ItemRemoved,
            OrderHistoryItemKind.StateChanged => OrderHistoryItemKindDto.StateChanged,
            OrderHistoryItemKind.Unspecified => throw new NotImplementedException(),
            _ => OrderHistoryItemKindDto.Created,
        };
    }
}

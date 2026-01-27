using Google.Protobuf.WellKnownTypes;
using OrderManagementService.Domain.Models.Orders;
using OrderManagementService.Domain.Models.Orders.History;
using ProtoCreatedOrderHistoryPayload = Orders.CreatedOrderHistoryPayload;
using ProtoItemAddedHistoryPayload = Orders.ItemAddedHistoryPayload;
using ProtoItemRemovedHistoryPayload = Orders.ItemRemovedHistoryPayload;
using ProtoOrder = Orders.Order;
using ProtoOrderFilter = Orders.OrderFilter;
using ProtoOrderHistoryFilter = Orders.OrderHistoryFilter;
using ProtoOrderHistoryItem = Orders.OrderHistoryItem;
using ProtoOrderHistoryItemKind = Orders.OrderHistoryItemKind;
using ProtoOrderHistoryPayload = Orders.OrderHistoryPayload;
using ProtoOrderState = Orders.OrderState;
using ProtoStateChangedHistoryPayload = Orders.StateChangedHistoryPayload;

namespace OrderManagementService.Presentation.Grpc.Mappings;

public static class OrderMapping
{
    public static ProtoOrderState ToProto(this OrderState state)
    {
        return state switch
        {
            OrderState.Created => ProtoOrderState.Created,
            OrderState.Processing => ProtoOrderState.Processing,
            OrderState.Completed => ProtoOrderState.Completed,
            OrderState.Cancelled => ProtoOrderState.Cancelled,
            _ => ProtoOrderState.Unspecified,
        };
    }

    public static OrderState ToDomain(this ProtoOrderState state)
    {
        return state switch
        {
            ProtoOrderState.Created => OrderState.Created,
            ProtoOrderState.Processing => OrderState.Processing,
            ProtoOrderState.Completed => OrderState.Completed,
            ProtoOrderState.Cancelled => OrderState.Cancelled,
            ProtoOrderState.Unspecified => throw new ArgumentException("Cannot convert Unspecified order state to domain", nameof(state)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };
    }

    public static ProtoOrderHistoryItemKind ToProto(this OrderHistoryItemKind kind)
    {
        return kind switch
        {
            OrderHistoryItemKind.Created => ProtoOrderHistoryItemKind.Created,
            OrderHistoryItemKind.ItemAdded => ProtoOrderHistoryItemKind.ItemAdded,
            OrderHistoryItemKind.ItemRemoved => ProtoOrderHistoryItemKind.ItemRemoved,
            OrderHistoryItemKind.StateChanged => ProtoOrderHistoryItemKind.StateChanged,
            _ => ProtoOrderHistoryItemKind.Unspecified,
        };
    }

    public static OrderHistoryItemKind ToDomain(this ProtoOrderHistoryItemKind kind)
    {
        return kind switch
        {
            ProtoOrderHistoryItemKind.Created => OrderHistoryItemKind.Created,
            ProtoOrderHistoryItemKind.ItemAdded => OrderHistoryItemKind.ItemAdded,
            ProtoOrderHistoryItemKind.ItemRemoved => OrderHistoryItemKind.ItemRemoved,
            ProtoOrderHistoryItemKind.StateChanged => OrderHistoryItemKind.StateChanged,
            ProtoOrderHistoryItemKind.Unspecified => throw new ArgumentException("Cannot convert Unspecified history item kind to domain", nameof(kind)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    public static ProtoOrder ToProto(this Order order)
    {
        return new ProtoOrder
        {
            Id = order.Id,
            State = order.State.ToProto(),
            CreatedAt = Timestamp.FromDateTimeOffset(order.CreatedAt),
            CreatedBy = order.CreatedBy,
        };
    }

    public static ProtoOrderHistoryPayload ToProto(this OrderHistoryPayload payload)
    {
        var result = new ProtoOrderHistoryPayload();
        switch (payload)
        {
            case CreatedOrderHistoryPayload p:
                result.Created = new ProtoCreatedOrderHistoryPayload
                {
                    CreatedBy = p.CreatedBy,
                    CreatedAt = Timestamp.FromDateTimeOffset(p.CreatedAt),
                };
                break;
            case ItemAddedHistoryPayload p:
                result.ItemAdded = new ProtoItemAddedHistoryPayload
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                };
                break;
            case ItemRemovedHistoryPayload p:
                result.ItemRemoved = new ProtoItemRemovedHistoryPayload
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                };
                break;
            case StateChangedHistoryPayload p:
                result.StateChanged = new ProtoStateChangedHistoryPayload
                {
                    From = p.From,
                    To = p.To,
                };
                break;
            default:
                break;
        }

        return result;
    }

    public static ProtoOrderHistoryItem ToProto(this OrderHistoryItem item)
    {
        return new ProtoOrderHistoryItem
        {
            Id = item.Id,
            OrderId = item.OrderId,
            CreatedAt = Timestamp.FromDateTimeOffset(item.CreatedAt),
            Kind = item.Kind.ToProto(),
            Payload = item.Payload.ToProto(),
        };
    }

    public static OrderFilter ToDomain(this ProtoOrderFilter filter)
    {
        return new OrderFilter(
            Ids: filter.Ids.Count > 0 ? filter.Ids.ToArray() : null,
            State: filter.State != ProtoOrderState.Unspecified ? filter.State.ToDomain() : null,
            CreatedBy: string.IsNullOrWhiteSpace(filter.CreatedBy) ? null : filter.CreatedBy,
            PageSize: filter.PageSize,
            PageToken: string.IsNullOrWhiteSpace(filter.PageToken) ? null : filter.PageToken);
    }

    public static OrderHistoryFilter ToDomain(this ProtoOrderHistoryFilter filter)
    {
        return new OrderHistoryFilter(
            OrderIds: filter.OrderIds.Count > 0 ? filter.OrderIds.ToArray() : null,
            Kind: filter.Kind != ProtoOrderHistoryItemKind.Unspecified ? filter.Kind.ToDomain() : null,
            PageSize: filter.PageSize,
            PageToken: string.IsNullOrWhiteSpace(filter.PageToken) ? null : filter.PageToken);
    }
}
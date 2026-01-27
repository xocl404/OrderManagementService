namespace OrderManagementService.Domain.Models.Orders.History;

public enum OrderHistoryItemKind
{
    Created,
    ItemAdded,
    ItemRemoved,
    StateChanged,
}
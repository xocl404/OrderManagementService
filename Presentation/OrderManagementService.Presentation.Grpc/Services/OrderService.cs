using Grpc.Core;
using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Presentation.Grpc.Mappings;
using Orders;
using OrderHistoryFilter = OrderManagementService.Domain.Models.Orders.History.OrderHistoryFilter;
using OrderHistoryItem = OrderManagementService.Domain.Models.Orders.History.OrderHistoryItem;

namespace OrderManagementService.Presentation.Grpc.Services;

public sealed class OrderService : OrdersService.OrdersServiceBase
{
    private readonly IOrderService _orderService;

    public OrderService(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task<CreateOrderResponse> CreateOrder(
        CreateOrderRequest request,
        ServerCallContext context)
    {
        long orderId = await _orderService.CreateAsync(request.CreatedBy, context.CancellationToken);
        return new CreateOrderResponse { OrderId = orderId };
    }

    public override async Task<AddItemResponse> AddItem(
        AddItemRequest request,
        ServerCallContext context)
    {
        await _orderService.AddItemAsync(request.OrderId, request.ProductId, request.Quantity, context.CancellationToken);
        return new AddItemResponse();
    }

    public override async Task<RemoveItemResponse> RemoveItem(
        RemoveItemRequest request,
        ServerCallContext context)
    {
        await _orderService.RemoveItemAsync(request.OrderItemId, context.CancellationToken);
        return new RemoveItemResponse();
    }

    public override async Task<StartProcessingResponse> StartProcessing(
        StartProcessingRequest request,
        ServerCallContext context)
    {
        await _orderService.StartProcessingAsync(request.OrderId, context.CancellationToken);
        return new StartProcessingResponse();
    }

    public override async Task<CompleteResponse> Complete(
        CompleteRequest request,
        ServerCallContext context)
    {
        await _orderService.CompleteAsync(request.OrderId, context.CancellationToken);
        return new CompleteResponse();
    }

    public override async Task<CancelResponse> Cancel(
        CancelRequest request,
        ServerCallContext context)
    {
        await _orderService.CancelAsync(request.OrderId, context.CancellationToken);
        return new CancelResponse();
    }

    public override async Task<GetHistoryResponse> GetHistory(
        GetHistoryRequest request,
        ServerCallContext context)
    {
        OrderHistoryFilter filter = request.Filter?.ToDomain() ?? new OrderHistoryFilter(PageSize: 50);
        PagedResult<OrderHistoryItem> result = await _orderService.GetHistoryAsync(request.OrderId, filter, context.CancellationToken);

        var response = new GetHistoryResponse
        {
            Result = new PagedResultOrderHistoryItem
            {
                NextPageToken = result.NextPageToken ?? string.Empty,
            },
        };

        foreach (OrderHistoryItem item in result.Items)
        {
            response.Result.Items.Add(item.ToProto());
        }

        return response;
    }
}

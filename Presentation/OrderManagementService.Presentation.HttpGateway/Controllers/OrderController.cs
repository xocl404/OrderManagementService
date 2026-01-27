using OrderManagementService.Presentation.HttpGateway.Mappings;
using OrderManagementService.Presentation.HttpGateway.Models;
using OrderManagementService.Presentation.HttpGateway.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Orders;

namespace OrderManagementService.Presentation.HttpGateway.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrderController : ControllerBase
{
    private readonly OrdersService.OrdersServiceClient _client;

    public OrderController(OrdersService.OrdersServiceClient client)
    {
        _client = client;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<long>> CreateOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        CreateOrderResponse response = await _client.CreateOrderAsync(
            new CreateOrderRequest { CreatedBy = request.CreatedBy },
            cancellationToken: cancellationToken);

        return Ok(response.OrderId);
    }

    [HttpPost("{orderId:long}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddItem(long orderId, [FromBody] AddItemRequestDto request, CancellationToken cancellationToken)
    {
        await _client.AddItemAsync(
            new AddItemRequest
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
            },
            cancellationToken: cancellationToken);

        return Ok();
    }

    [HttpDelete("items/{orderItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveItem(long orderItemId, CancellationToken cancellationToken)
    {
        await _client.RemoveItemAsync(
            new RemoveItemRequest { OrderItemId = orderItemId },
            cancellationToken: cancellationToken);

        return Ok();
    }

    [HttpPost("{orderId:long}/start-processing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> StartProcessing(long orderId, CancellationToken cancellationToken)
    {
        await _client.StartProcessingAsync(
            new StartProcessingRequest { OrderId = orderId },
            cancellationToken: cancellationToken);

        return Ok();
    }

    [HttpPost("{orderId:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Cancel(long orderId, CancellationToken cancellationToken)
    {
        await _client.CancelAsync(
            new CancelRequest { OrderId = orderId },
            cancellationToken: cancellationToken);

        return Ok();
    }

    [HttpGet("{orderId:long}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDto<OrderHistoryItemDto>>> GetHistory(
        long orderId,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        GetHistoryResponse response = await _client.GetHistoryAsync(
            new GetHistoryRequest
            {
                OrderId = orderId,
                Filter = new OrderHistoryFilter
                {
                    PageSize = pageSize,
                    PageToken = pageToken ?? string.Empty,
                },
            },
            cancellationToken: cancellationToken);

        OrderHistoryItemDto[] items = response.Result.Items.Select(i => i.ToDto()).ToArray();
        var result = new PagedResultDto<OrderHistoryItemDto>(
            items,
            string.IsNullOrWhiteSpace(response.Result.NextPageToken) ? null : response.Result.NextPageToken);

        return Ok(result);
    }
}

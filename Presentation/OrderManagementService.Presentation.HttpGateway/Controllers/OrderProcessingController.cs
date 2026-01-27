using OrderManagementService.Presentation.HttpGateway.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Orders.ProcessingService.Contracts;

namespace OrderManagementService.Presentation.HttpGateway.Controllers;

[ApiController]
[Route("api/orders/{orderId:long}")]
public sealed class OrderProcessingController : ControllerBase
{
    private readonly OrderService.OrderServiceClient _client;

    public OrderProcessingController(OrderService.OrderServiceClient client)
    {
        _client = client;
    }

    [HttpPost("approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Approve(long orderId, [FromBody] ApproveOrderRequestDto request, CancellationToken cancellationToken)
    {
        var grpcRequest = new ApproveOrderRequest
        {
            OrderId = orderId,
            IsApproved = request.IsApproved,
            ApprovedBy = request.ApprovedBy,
            FailureReason = request.FailureReason ?? string.Empty,
        };

        await _client.ApproveOrderAsync(grpcRequest, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpPost("packing/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> StartPacking(long orderId, [FromBody] StartPackingRequestDto request, CancellationToken cancellationToken)
    {
        var grpcRequest = new StartOrderPackingRequest
        {
            OrderId = orderId,
            PackingBy = request.PackingBy,
        };

        await _client.StartOrderPackingAsync(grpcRequest, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpPost("packing/finish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> FinishPacking(long orderId, [FromBody] FinishPackingRequestDto request, CancellationToken cancellationToken)
    {
        var grpcRequest = new FinishOrderPackingRequest
        {
            OrderId = orderId,
            IsSuccessful = request.IsSuccessful,
            FailureReason = request.FailureReason ?? string.Empty,
        };

        await _client.FinishOrderPackingAsync(grpcRequest, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpPost("delivery/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> StartDelivery(long orderId, [FromBody] StartDeliveryRequestDto request, CancellationToken cancellationToken)
    {
        var grpcRequest = new StartOrderDeliveryRequest
        {
            OrderId = orderId,
            DeliveredBy = request.DeliveredBy,
        };

        await _client.StartOrderDeliveryAsync(grpcRequest, cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpPost("delivery/finish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> FinishDelivery(long orderId, [FromBody] FinishDeliveryRequestDto request, CancellationToken cancellationToken)
    {
        var grpcRequest = new FinishOrderDeliveryRequest
        {
            OrderId = orderId,
            IsSuccessful = request.IsSuccessful,
            FailureReason = request.FailureReason ?? string.Empty,
        };

        await _client.FinishOrderDeliveryAsync(grpcRequest, cancellationToken: cancellationToken);
        return Ok();
    }
}

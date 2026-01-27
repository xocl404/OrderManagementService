using Confluent.Kafka;
using OrderManagementService.Application.Abstractions;
using OrderManagementService.Infrastructure.Kafka.Contracts;
using Orders.Kafka.Contracts;

namespace OrderManagementService.Infrastructure.Kafka.Handlers;

public sealed class OrderProcessingMessageHandler : IKafkaMessageHandler<byte[], byte[]>
{
    private readonly IOrderProcessingService _orders;

    public OrderProcessingMessageHandler(IOrderProcessingService orders)
    {
        _orders = orders;
    }

    public async Task HandleAsync(IReadOnlyList<ConsumeResult<byte[], byte[]>> messages, CancellationToken cancellationToken)
    {
        foreach (ConsumeResult<byte[], byte[]> message in messages)
        {
            OrderProcessingValue value = OrderProcessingValue.Parser.ParseFrom(message.Message.Value);
            switch (value.EventCase)
            {
                case OrderProcessingValue.EventOneofCase.ApprovalReceived:
                {
                    string toState = value.ApprovalReceived.IsApproved
                        ? $"processing:approval:approved:{value.ApprovalReceived.CreatedBy}"
                        : "processing:approval:rejected";

                    await _orders.AddHistoryEventAsync(
                        value.ApprovalReceived.OrderId,
                        "processing",
                        toState,
                        cancellationToken);

                    if (!value.ApprovalReceived.IsApproved)
                    {
                        await _orders.CancelFromProcessorAsync(value.ApprovalReceived.OrderId, cancellationToken);
                    }

                    break;
                }

                case OrderProcessingValue.EventOneofCase.PackingStarted:
                {
                    await _orders.AddHistoryEventAsync(
                        value.PackingStarted.OrderId,
                        "processing",
                        $"processing:packing_started:{value.PackingStarted.PackingBy}",
                        cancellationToken);
                    break;
                }

                case OrderProcessingValue.EventOneofCase.PackingFinished:
                {
                    string failure = value.PackingFinished.FailureReason ?? "unknown";
                    string toState = value.PackingFinished.IsFinishedSuccessfully
                        ? "processing:packing_finished:success"
                        : $"processing:packing_finished:failed:{failure}";

                    await _orders.AddHistoryEventAsync(
                        value.PackingFinished.OrderId,
                        "processing",
                        toState,
                        cancellationToken);

                    if (!value.PackingFinished.IsFinishedSuccessfully)
                    {
                        await _orders.CancelFromProcessorAsync(value.PackingFinished.OrderId, cancellationToken);
                    }

                    break;
                }

                case OrderProcessingValue.EventOneofCase.DeliveryStarted:
                {
                    await _orders.AddHistoryEventAsync(
                        value.DeliveryStarted.OrderId,
                        "processing",
                        $"processing:delivery_started:{value.DeliveryStarted.DeliveredBy}",
                        cancellationToken);
                    break;
                }

                case OrderProcessingValue.EventOneofCase.DeliveryFinished:
                {
                    string failure = value.DeliveryFinished.FailureReason ?? "unknown";
                    string toState = value.DeliveryFinished.IsFinishedSuccessfully
                        ? "processing:delivery_finished:success"
                        : $"processing:delivery_finished:failed:{failure}";

                    await _orders.AddHistoryEventAsync(
                        value.DeliveryFinished.OrderId,
                        "processing",
                        toState,
                        cancellationToken);

                    if (value.DeliveryFinished.IsFinishedSuccessfully)
                    {
                        await _orders.CompleteFromProcessorAsync(value.DeliveryFinished.OrderId, cancellationToken);
                    }
                    else
                    {
                        await _orders.CancelFromProcessorAsync(value.DeliveryFinished.OrderId, cancellationToken);
                    }

                    break;
                }
            }
        }
    }
}
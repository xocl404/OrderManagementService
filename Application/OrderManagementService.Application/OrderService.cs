using Google.Protobuf;
using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Exceptions;
using OrderManagementService.Domain.Models.Orders;
using OrderManagementService.Domain.Models.Orders.History;
using OrderManagementService.Application.Abstractions;
using OrderManagementService.Infrastructure.Kafka.Contracts;
using OrderManagementService.Infrastructure.Kafka.Options;
using Microsoft.Extensions.Options;
using Orders.Kafka.Contracts;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace OrderManagementService.Application;

public class OrderService : IOrderService, IOrderProcessingService
{
    private readonly IOrderRepository _orders;

    private readonly IOrderItemRepository _orderItems;

    private readonly IOrderHistoryRepository _history;

    private readonly ITransactionFactory _transactionFactory;

    private readonly IKafkaProducer<byte[], byte[]> _kafka;

    private readonly KafkaTopicsOptions _topics;

    public OrderService(
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IOrderHistoryRepository history,
        ITransactionFactory transactionFactory,
        IKafkaProducer<byte[], byte[]> kafka,
        IOptions<KafkaTopicsOptions> topics)
    {
        _orders = orders;
        _orderItems = orderItems;
        _history = history;
        _transactionFactory = transactionFactory;
        _kafka = kafka;
        _topics = topics.Value;
    }

    public async Task<long> CreateAsync(string createdBy, CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using ITransaction transaction = await _transactionFactory.BeginAsync(cancellationToken);
        long orderId = await _orders.AddAsync(OrderState.Created, now, createdBy, cancellationToken, transaction.Transaction);
        await _history.AddAsync(orderId, OrderHistoryItemKind.Created, new CreatedOrderHistoryPayload(createdBy, now), now, cancellationToken, transaction.Transaction);
        await transaction.CommitAsync(cancellationToken);

        var createdEvent = new OrderCreationValue
        {
            OrderCreated = new OrderCreationValue.Types.OrderCreated
            {
                OrderId = orderId,
                CreatedAt = Timestamp.FromDateTimeOffset(now),
            },
        };
        byte[] key = new OrderCreationKey { OrderId = orderId }.ToByteArray();
        await _kafka.ProduceAsync(_topics.OrderCreation, key, createdEvent.ToByteArray(), cancellationToken);

        return orderId;
    }

    public async Task<bool> AddItemAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        EnsureState(order, OrderState.Created, "add item");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using ITransaction transaction = await _transactionFactory.BeginAsync(cancellationToken);
        await _orderItems.AddAsync(orderId, productId, quantity, cancellationToken, transaction.Transaction);
        await _history.AddAsync(orderId, OrderHistoryItemKind.ItemAdded, new ItemAddedHistoryPayload(productId, quantity), now, cancellationToken, transaction.Transaction);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveItemAsync(long orderItemId, CancellationToken cancellationToken)
    {
        OrderItem? item = await _orderItems.GetAsync(orderItemId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        Order order = await GetOrderOrThrowAsync(item.OrderId, cancellationToken);
        EnsureState(order, OrderState.Created, "remove item");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using ITransaction transaction = await _transactionFactory.BeginAsync(cancellationToken);
        bool deleted = await _orderItems.SoftDeleteAsync(orderItemId, cancellationToken, transaction.Transaction);
        if (!deleted)
        {
            return false;
        }

        await _history.AddAsync(order.Id, OrderHistoryItemKind.ItemRemoved, new ItemRemovedHistoryPayload(item.ProductId, item.Quantity), now, cancellationToken, transaction.Transaction);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartProcessingAsync(long orderId, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        EnsureState(order, OrderState.Created, "start processing");

        bool changed = await ChangeStateAsync(order, OrderState.Processing, cancellationToken);
        if (!changed) return false;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        var startedEvent = new OrderCreationValue
        {
            OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
            {
                OrderId = orderId,
                StartedAt = Timestamp.FromDateTimeOffset(now),
            },
        };
        byte[] key = new OrderCreationKey { OrderId = orderId }.ToByteArray();
        await _kafka.ProduceAsync(_topics.OrderCreation, key, startedEvent.ToByteArray(), cancellationToken);

        return true;
    }

    public Task<bool> CompleteAsync(long orderId, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Manual complete is not allowed.");
    }

    public async Task<bool> CancelAsync(long orderId, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        EnsureState(order, OrderState.Created, "cancel");
        return await ChangeStateAsync(order, OrderState.Cancelled, cancellationToken);
    }

    public async Task CancelFromProcessorAsync(long orderId, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        if (order.State == OrderState.Cancelled)
        {
            return;
        }

        await ChangeStateAsync(order, OrderState.Cancelled, cancellationToken);
    }

    public async Task CompleteFromProcessorAsync(long orderId, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        if (order.State is OrderState.Completed or OrderState.Cancelled)
        {
            return;
        }

        if (order.State != OrderState.Processing)
        {
            throw new InvalidOrderStateException(order.Id, order.State, "complete");
        }

        await ChangeStateAsync(order, OrderState.Completed, cancellationToken);
    }

    public async Task AddHistoryEventAsync(long orderId, string fromState, string toState, CancellationToken cancellationToken)
    {
        Order order = await GetOrderOrThrowAsync(orderId, cancellationToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using ITransaction transaction = await _transactionFactory.BeginAsync(cancellationToken);
        await _history.AddAsync(order.Id, OrderHistoryItemKind.StateChanged, new StateChangedHistoryPayload(fromState, toState), now, cancellationToken, transaction.Transaction);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<PagedResult<OrderHistoryItem>> GetHistoryAsync(long orderId, OrderHistoryFilter filter, CancellationToken cancellationToken)
    {
        await GetOrderOrThrowAsync(orderId, cancellationToken);
        OrderHistoryFilter effectiveFilter = filter with
        {
            OrderIds = filter.OrderIds is null ? new[] { orderId } : filter.OrderIds.Append(orderId).ToArray(),
        };

        return await _history.QueryAsync(effectiveFilter, cancellationToken);
    }

    private async Task<Order> GetOrderOrThrowAsync(long orderId, CancellationToken cancellationToken)
    {
        Order? order = await _orders.GetAsync(orderId, cancellationToken);
        return order ?? throw new OrderNotFoundException(orderId);
    }

    private void EnsureState(Order order, OrderState expected, string operation)
    {
        if (order.State != expected)
        {
            throw new InvalidOrderStateException(order.Id, order.State, operation);
        }
    }

    private async Task<bool> ChangeStateAsync(Order order, OrderState newState, CancellationToken cancellationToken)
    {
        if (order.State == newState)
        {
            return true;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using ITransaction transaction = await _transactionFactory.BeginAsync(cancellationToken);
        bool updated = await _orders.UpdateStateAsync(order.Id, newState, cancellationToken, transaction.Transaction);
        if (!updated)
        {
            return false;
        }

        await _history.AddAsync(order.Id, OrderHistoryItemKind.StateChanged, new StateChangedHistoryPayload(order.State.ToString(), newState.ToString()), now, cancellationToken, transaction.Transaction);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}

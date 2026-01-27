using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Models.Orders.History;
using OrderManagementService.Infrastructure.DataAccess.Serializers;
using Npgsql;
using System.Data.Common;
using System.Text;

namespace OrderManagementService.Infrastructure.DataAccess.Repositories;

public sealed class OrderHistoryRepository : IOrderHistoryRepository
{
    private const string InsertOrderHistorySql =
        """
        insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
        values (@orderId, @createdAt, @kind::order_history_item_kind, @payload::jsonb)
        returning order_history_item_id;
        """;

    private readonly NpgsqlDataSource _dataSource;
    private readonly OrderHistoryPayloadSerializer _serializer;

    public OrderHistoryRepository(NpgsqlDataSource dataSource, OrderHistoryPayloadSerializer serializer)
    {
        _dataSource = dataSource;
        _serializer = serializer;
    }

    public async Task<long> AddAsync(
        long orderId,
        OrderHistoryItemKind kind,
        OrderHistoryPayload payload,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken,
        DbTransaction? transaction = null)
    {
        string payloadJson = _serializer.Serialize(payload);
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = InsertOrderHistorySql;
        cmd.Parameters.AddWithValue("orderId", orderId);
        cmd.Parameters.AddWithValue("createdAt", createdAt);
        cmd.Parameters.AddWithValue("kind", ToDbKind(kind));
        cmd.Parameters.AddWithValue("payload", payloadJson);
        object? result = await cmd.ExecuteScalarAsync(cancellationToken);

        return result is null ? 0 : (long)result;
    }

    public async Task<PagedResult<OrderHistoryItem>> QueryAsync(OrderHistoryFilter filter, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(
            """
            select order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
            from order_history
            """);
        var parameters = new List<NpgsqlParameter>();
        var conditions = new List<string>();

        if (filter.OrderIds is not null && filter.OrderIds.Count > 0)
        {
            conditions.Add("order_id = any(@orderIds)");
            parameters.Add(new NpgsqlParameter("orderIds", filter.OrderIds.ToArray()));
        }

        if (filter.Kind is not null)
        {
            conditions.Add("order_history_item_kind = @kind::order_history_item_kind");
            parameters.Add(new NpgsqlParameter("kind", ToDbKind(filter.Kind.Value)));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageToken) && long.TryParse(filter.PageToken, out long lastId))
        {
            conditions.Add("order_history_item_id > @lastId");
            parameters.Add(new NpgsqlParameter("lastId", lastId));
        }

        if (conditions.Count > 0)
        {
            sql.Append(" where ").Append(string.Join(" and ", conditions));
        }

        sql.Append(" order by order_history_item_id ");
        sql.Append(" limit @limit");
        parameters.Add(new NpgsqlParameter("limit", filter.PageSize));

        await using NpgsqlCommand cmd = _dataSource.CreateCommand(sql.ToString());
        cmd.Parameters.AddRange(parameters.ToArray());

        var items = new List<OrderHistoryItem>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            long orderId = reader.GetInt64(1);
            DateTimeOffset createdAt = await reader.GetFieldValueAsync<DateTimeOffset>(2, cancellationToken);
            OrderHistoryItemKind kind = ParseKind(reader.GetString(3));
            string payloadJson = reader.GetString(4);
            OrderHistoryPayload payload = _serializer.Deserialize(payloadJson);
            items.Add(new OrderHistoryItem(id, orderId, createdAt, kind, payload));
        }

        string? nextPage = items.Count == filter.PageSize ? items.Last().Id.ToString() : null;
        return new PagedResult<OrderHistoryItem>(items, nextPage);
    }

    private static string ToDbKind(OrderHistoryItemKind kind)
    {
        return kind switch
        {
            OrderHistoryItemKind.Created => "created",
            OrderHistoryItemKind.ItemAdded => "item_added",
            OrderHistoryItemKind.ItemRemoved => "item_removed",
            OrderHistoryItemKind.StateChanged => "state_changed",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    private static OrderHistoryItemKind ParseKind(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "created" => OrderHistoryItemKind.Created,
            "item_added" => OrderHistoryItemKind.ItemAdded,
            "item_removed" => OrderHistoryItemKind.ItemRemoved,
            "state_changed" => OrderHistoryItemKind.StateChanged,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    private NpgsqlCommand CreateCommand(DbTransaction? transaction)
    {
        if (transaction is not NpgsqlTransaction npgsqlTransaction) return _dataSource.CreateCommand();

        NpgsqlConnection connection = npgsqlTransaction.Connection
                                      ?? throw new InvalidOperationException("Transaction has no open connection");

        NpgsqlCommand cmd = connection.CreateCommand();
        cmd.Transaction = npgsqlTransaction;
        return cmd;
    }
}
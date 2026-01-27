using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Models.Orders;
using Npgsql;
using System.Data.Common;
using System.Text;

namespace OrderManagementService.Infrastructure.DataAccess.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private const string InsertOrderSql =
        """
        insert into orders (order_state, order_created_at, order_created_by)
        values (@state::order_state, @createdAt, @createdBy)
        returning order_id;
        """;

    private const string UpdateOrderStateSql =
        """
        update orders
        set order_state = @state::order_state
        where order_id = @id;
        """;

    private const string GetOrderSql =
        """
        select order_id, order_state, order_created_at, order_created_by
        from orders
        where order_id = @id;
        """;

    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> AddAsync(OrderState state, DateTimeOffset createdAt, string createdBy, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = InsertOrderSql;
        cmd.Parameters.AddWithValue("state", ToDbState(state));
        cmd.Parameters.AddWithValue("createdAt", createdAt);
        cmd.Parameters.AddWithValue("createdBy", createdBy);
        object? result = await cmd.ExecuteScalarAsync(cancellationToken);

        return result is null ? 0 : (long)result;
    }

    public async Task<bool> UpdateStateAsync(long orderId, OrderState newState, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = UpdateOrderStateSql;
        cmd.Parameters.AddWithValue("state", ToDbState(newState));
        cmd.Parameters.AddWithValue("id", orderId);

        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<PagedResult<Order>> QueryAsync(OrderFilter filter, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(
            """
            select order_id, order_state, order_created_at, order_created_by
            from orders
            """);
        var parameters = new List<NpgsqlParameter>();
        var conditions = new List<string>();

        if (filter.Ids is not null && filter.Ids.Count > 0)
        {
            conditions.Add("order_id = any(@ids)");
            parameters.Add(new NpgsqlParameter("ids", filter.Ids.ToArray()));
        }

        if (filter.State is not null)
        {
            conditions.Add("order_state = @state::order_state");
            parameters.Add(new NpgsqlParameter("state", ToDbState(filter.State.Value)));
        }

        if (!string.IsNullOrWhiteSpace(filter.CreatedBy))
        {
            conditions.Add("order_created_by = @createdBy");
            parameters.Add(new NpgsqlParameter("createdBy", filter.CreatedBy));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageToken) && long.TryParse(filter.PageToken, out long lastId))
        {
            conditions.Add("order_id > @lastId");
            parameters.Add(new NpgsqlParameter("lastId", lastId));
        }

        if (conditions.Count > 0)
        {
            sql.Append(" where ").Append(string.Join(" and ", conditions));
        }

        sql.Append(" order by order_id ");
        sql.Append(" limit @limit");
        parameters.Add(new NpgsqlParameter("limit", filter.PageSize));

        await using NpgsqlCommand cmd = _dataSource.CreateCommand(sql.ToString());
        cmd.Parameters.AddRange(parameters.ToArray());

        var items = new List<Order>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            OrderState state = ParseState(reader.GetString(1));
            DateTimeOffset createdAt = await reader.GetFieldValueAsync<DateTimeOffset>(2, cancellationToken);
            string createdBy = reader.GetString(3);
            items.Add(new Order(id, state, createdAt, createdBy));
        }

        string? nextPage = items.Count == filter.PageSize ? items.Last().Id.ToString() : null;
        return new PagedResult<Order>(items, nextPage);
    }

    public async Task<Order?> GetAsync(long orderId, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = GetOrderSql;
        cmd.Parameters.AddWithValue("id", orderId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            OrderState state = ParseState(reader.GetString(1));
            DateTimeOffset createdAt = await reader.GetFieldValueAsync<DateTimeOffset>(2, cancellationToken);
            string createdBy = reader.GetString(3);

            return new Order(id, state, createdAt, createdBy);
        }

        return null;
    }

    private static OrderState ParseState(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "created" => OrderState.Created,
            "processing" => OrderState.Processing,
            "completed" => OrderState.Completed,
            "cancelled" => OrderState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    private static string ToDbState(OrderState state)
    {
        return state switch
        {
            OrderState.Created => "created",
            OrderState.Processing => "processing",
            OrderState.Completed => "completed",
            OrderState.Cancelled => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
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

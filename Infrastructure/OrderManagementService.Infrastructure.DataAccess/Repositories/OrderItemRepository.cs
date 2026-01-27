using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Models.Orders;
using Npgsql;
using System.Data.Common;
using System.Text;

namespace OrderManagementService.Infrastructure.DataAccess.Repositories;

public sealed class OrderItemRepository : IOrderItemRepository
{
    private const string InsertOrderItemSql =
        """
        insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
        values (@orderId, @productId, @quantity, false)
        returning order_item_id;
        """;

    private const string SoftDeleteOrderItemSql =
        """
        update order_items
        set order_item_deleted = true
        where order_item_id = @id;
        """;

    private const string GetOrderItemSql =
        """
        select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
        from order_items
        where order_item_id = @id;
        """;

    private readonly NpgsqlDataSource _dataSource;

    public OrderItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> AddAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = InsertOrderItemSql;
        cmd.Parameters.AddWithValue("orderId", orderId);
        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.AddWithValue("quantity", quantity);
        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null ? 0 : (long)result;
    }

    public async Task<bool> SoftDeleteAsync(long orderItemId, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = SoftDeleteOrderItemSql;
        cmd.Parameters.AddWithValue("id", orderItemId);
        return await cmd.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<PagedResult<OrderItem>> QueryAsync(OrderItemFilter filter, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(
            """
            select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
            from order_items
            """);
        var parameters = new List<NpgsqlParameter>();
        var conditions = new List<string>();

        if (filter.OrderIds is not null && filter.OrderIds.Count > 0)
        {
            conditions.Add("order_id = any(@orderIds)");
            parameters.Add(new NpgsqlParameter("orderIds", filter.OrderIds.ToArray()));
        }

        if (filter.ProductIds is not null && filter.ProductIds.Count > 0)
        {
            conditions.Add("product_id = any(@productIds)");
            parameters.Add(new NpgsqlParameter("productIds", filter.ProductIds.ToArray()));
        }

        if (filter.IsDeleted is not null)
        {
            conditions.Add("order_item_deleted = @deleted");
            parameters.Add(new NpgsqlParameter("deleted", filter.IsDeleted.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageToken) && long.TryParse(filter.PageToken, out long lastId))
        {
            conditions.Add("order_item_id > @lastId");
            parameters.Add(new NpgsqlParameter("lastId", lastId));
        }

        if (conditions.Count > 0)
        {
            sql.Append(" where ").Append(string.Join(" and ", conditions));
        }

        sql.Append(" order by order_item_id ");
        sql.Append(" limit @limit");
        parameters.Add(new NpgsqlParameter("limit", filter.PageSize));

        await using NpgsqlCommand cmd = _dataSource.CreateCommand(sql.ToString());
        cmd.Parameters.AddRange(parameters.ToArray());

        var items = new List<OrderItem>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            long orderId = reader.GetInt64(1);
            long productId = reader.GetInt64(2);
            int quantity = reader.GetInt32(3);
            bool deleted = reader.GetBoolean(4);

            items.Add(new OrderItem(id, orderId, productId, quantity, deleted));
        }

        string? nextPage = items.Count == filter.PageSize ? items.Last().Id.ToString() : null;
        return new PagedResult<OrderItem>(items, nextPage);
    }

    public async Task<OrderItem?> GetAsync(long orderItemId, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = GetOrderItemSql;
        cmd.Parameters.AddWithValue("id", orderItemId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            long orderId = reader.GetInt64(1);
            long productId = reader.GetInt64(2);
            int quantity = reader.GetInt32(3);
            bool deleted = reader.GetBoolean(4);

            return new OrderItem(id, orderId, productId, quantity, deleted);
        }

        return null;
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

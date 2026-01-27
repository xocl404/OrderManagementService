using OrderManagementService.Domain;
using OrderManagementService.Domain.Abstractions;
using OrderManagementService.Domain.Models.Products;
using Npgsql;
using System.Data.Common;
using System.Text;

namespace OrderManagementService.Infrastructure.DataAccess.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private const string InsertProductSql =
        """
        insert into products (product_name, product_price)
        values (@name, @price)
        returning product_id;
        """;

    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> AddAsync(string name, decimal price, CancellationToken cancellationToken, DbTransaction? transaction = null)
    {
        await using NpgsqlCommand cmd = CreateCommand(transaction);
        cmd.CommandText = InsertProductSql;
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("price", price);
        object? result = await cmd.ExecuteScalarAsync(cancellationToken);

        return result is null ? 0 : (long)result;
    }

    public async Task<PagedResult<Product>> QueryAsync(ProductFilter filter, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(
            """
            select product_id, product_name, product_price
            from products
            """);
        var parameters = new List<NpgsqlParameter>();
        var conditions = new List<string>();

        if (filter.Ids is not null && filter.Ids.Count > 0)
        {
            conditions.Add("product_id = any(@ids)");
            parameters.Add(new NpgsqlParameter("ids", filter.Ids.ToArray()));
        }

        if (filter.MinPrice is not null)
        {
            conditions.Add("product_price >= @minPrice");
            parameters.Add(new NpgsqlParameter("minPrice", filter.MinPrice.Value));
        }

        if (filter.MaxPrice is not null)
        {
            conditions.Add("product_price <= @maxPrice");
            parameters.Add(new NpgsqlParameter("maxPrice", filter.MaxPrice.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.NameSubstring))
        {
            conditions.Add("product_name ilike @name");
            parameters.Add(new NpgsqlParameter("name", $"%{filter.NameSubstring}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageToken) && long.TryParse(filter.PageToken, out long lastId))
        {
            conditions.Add("product_id > @lastId");
            parameters.Add(new NpgsqlParameter("lastId", lastId));
        }

        if (conditions.Count > 0)
        {
            sql.Append(" where ").Append(string.Join(" and ", conditions));
        }

        sql.Append(" order by product_id ");
        sql.Append(" limit @limit");
        parameters.Add(new NpgsqlParameter("limit", filter.PageSize));

        await using NpgsqlCommand cmd = _dataSource.CreateCommand(sql.ToString());
        cmd.Parameters.AddRange(parameters.ToArray());

        var items = new List<Product>();
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            string name = reader.GetString(1);
            decimal price = await reader.GetFieldValueAsync<decimal>(2, cancellationToken);
            items.Add(new Product(id, name, price));
        }

        string? nextPage = items.Count == filter.PageSize ? items.Last().Id.ToString() : null;
        return new PagedResult<Product>(items, nextPage);
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

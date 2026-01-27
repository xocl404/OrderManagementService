using OrderManagementService.Application.Abstractions;
using Npgsql;
using System.Data.Common;

namespace OrderManagementService.Infrastructure.DataAccess.Transactions;

public sealed class NpgsqlTransactionWrapper : ITransaction
{
    private readonly NpgsqlConnection _connection;

    private readonly NpgsqlTransaction _transaction;

    public NpgsqlTransactionWrapper(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public DbTransaction Transaction => _transaction;

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
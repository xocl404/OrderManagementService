using OrderManagementService.Application.Abstractions;
using Npgsql;

namespace OrderManagementService.Infrastructure.DataAccess.Transactions;

public sealed class TransactionFactory : ITransactionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public TransactionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<ITransaction> BeginAsync(CancellationToken cancellationToken)
    {
        NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
        return new NpgsqlTransactionWrapper(connection, transaction);
    }
}
namespace OrderManagementService.Application.Abstractions;

public interface ITransactionFactory
{
    Task<ITransaction> BeginAsync(CancellationToken cancellationToken);
}
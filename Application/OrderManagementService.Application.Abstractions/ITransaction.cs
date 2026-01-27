using System.Data.Common;

namespace OrderManagementService.Application.Abstractions;

public interface ITransaction : IAsyncDisposable
{
    DbTransaction Transaction { get; }

    Task CommitAsync(CancellationToken cancellationToken);
}
using ConcertTicket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ConcertTicket.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
    }

    public async Task CommitTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction =
            _dbContext.Database.CurrentTransaction;

        if (transaction is null)
            return;

        await transaction.CommitAsync(
            cancellationToken);

        await transaction.DisposeAsync();
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction =
            _dbContext.Database.CurrentTransaction;

        if (transaction is null)
            return;

        await transaction.RollbackAsync(
            cancellationToken);

        await transaction.DisposeAsync();
    }
}
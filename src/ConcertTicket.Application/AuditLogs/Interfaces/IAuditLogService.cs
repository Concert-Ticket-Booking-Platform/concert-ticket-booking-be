using ConcertTicket.Application.AuditLogs.DTOs;
using ConcertTicket.Application.Common.Models;

namespace ConcertTicket.Application.AuditLogs.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AdminAuditLogDto>> GetAllAsync(
        AuditLogQueryRequest request,
        CancellationToken cancellationToken = default);

    Task WriteAsync(
        Guid actorId,
        string actorType,
        string action,
        string resourceType,
        Guid resourceId,
        object? metadata = null,
        CancellationToken cancellationToken = default);
}

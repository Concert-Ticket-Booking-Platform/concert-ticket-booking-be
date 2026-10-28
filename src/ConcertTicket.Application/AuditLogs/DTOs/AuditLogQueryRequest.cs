namespace ConcertTicket.Application.AuditLogs.DTOs;

public sealed record AuditLogQueryRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? ActorId = null,
    string? Action = null,
    string? ResourceType = null,
    Guid? ResourceId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null
);

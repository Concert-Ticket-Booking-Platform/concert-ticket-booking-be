namespace ConcertTicket.Application.AuditLogs.DTOs;

public sealed record AdminAuditLogDto(
    Guid Id,
    string ActorType,
    Guid ActorId,
    string? ActorUsername,
    string Action,
    string ResourceType,
    Guid ResourceId,
    string? Metadata,
    DateTimeOffset CreatedAt
);

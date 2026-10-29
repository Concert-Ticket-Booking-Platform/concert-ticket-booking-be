using ConcertTicket.Application.AuditLogs.DTOs;
using ConcertTicket.Application.AuditLogs.Interfaces;
using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Common.Models;
using ConcertTicket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace ConcertTicket.Application.AuditLogs.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _dbContext;

    public AuditLogService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        Guid actorId,
        string actorType,
        string action,
        string resourceType,
        Guid resourceId,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            ActorType = actorType,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Metadata = metadata is null
                ? null
                : JsonSerializer.Serialize(metadata),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.AuditLogs.Add(log);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminAuditLogDto>> GetAllAsync(
    AuditLogQueryRequest request,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (request.ActorId.HasValue)
        {
            query = query.Where(
                x => x.ActorId == request.ActorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();

            query = query.Where(
                x => x.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(request.ResourceType))
        {
            var resourceType = request.ResourceType.Trim();

            query = query.Where(
                x => x.ResourceType == resourceType);
        }

        if (request.ResourceId.HasValue)
        {
            query = query.Where(
                x => x.ResourceId == request.ResourceId.Value);
        }

        if (request.From.HasValue)
        {
            query = query.Where(
                x => x.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(
                x => x.CreatedAt <= request.To.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminAuditLogDto(
                x.Id,
                x.ActorType,
                x.ActorId,
                x.Actor.Username,
                x.Action,
                x.ResourceType,
                x.ResourceId,
                x.Metadata,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminAuditLogDto>(
            Items: logs,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }
}

using ConcertTicket.Application.AuditLogs.DTOs;
using ConcertTicket.Application.AuditLogs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api.Controllers;

[ApiController]
[Route("api/v1/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public sealed class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AdminAuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? actorId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? resourceType = null,
        [FromQuery] Guid? resourceId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from > to)
        {
            return BadRequest(new
            {
                message = "'from' must be earlier than or equal to 'to'."
            });
        }

        var request = new AuditLogQueryRequest(
            page,
            pageSize,
            actorId,
            action,
            resourceType,
            resourceId,
            from,
            to);

        var result = await _auditLogService.GetAllAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}

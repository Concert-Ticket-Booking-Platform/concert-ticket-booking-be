namespace ConcertTicket.Application.Users.DTOs;

public sealed record UpdateAdminUserStatusRequest(
    bool IsActive
);

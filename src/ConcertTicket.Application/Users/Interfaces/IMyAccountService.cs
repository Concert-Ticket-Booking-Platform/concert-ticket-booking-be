using ConcertTicket.Application.Users.DTOs;

namespace ConcertTicket.Application.Users.Interfaces;

public interface IMyAccountService
{
    Task<MyProfileDto> GetMyProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<MyProfileDto> UpdateMyProfileAsync(
        Guid userId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default);

    Task ChangeMyPasswordAsync(
        Guid userId,
        ChangeMyPasswordRequest request,
        CancellationToken cancellationToken = default);
}

using System.ComponentModel.DataAnnotations;

namespace ConcertTicket.Application.Users.DTOs;

public sealed record UpdateMyProfileRequest(
    [Required]
    [MaxLength(100)]
    string Username,

    [Phone]
    [MaxLength(30)]
    string? Phone,

    [MaxLength(100)]
    string? FirstName,

    [MaxLength(100)]
    string? LastName,

    [Url]
    [MaxLength(1000)]
    string? AvatarUrl
);

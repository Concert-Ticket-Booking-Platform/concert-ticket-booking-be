using System.ComponentModel.DataAnnotations;

namespace ConcertTicket.Application.Users.DTOs;

public sealed record ChangeMyPasswordRequest(
    [Required]
    string CurrentPassword,

    [Required]
    [MaxLength(128)]
    string NewPassword
);

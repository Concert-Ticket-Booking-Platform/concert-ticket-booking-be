using ConcertTicket.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ConcertTicket.Application.Users.DTOs;

public sealed record CreateAdminUserRequest(
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    string Email,

    [Required]
    [MaxLength(100)]
    string Username,

    [Required]
    [MaxLength(128)]
    string Password,

    [Required]
    RoleType Role
);

using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Concerts.DTOs;

public sealed record UpdateConcertStatusRequest(
    ConcertStatus Status);
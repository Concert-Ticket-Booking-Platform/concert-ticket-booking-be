using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.TicketCategories.DTOs;

public sealed record UpdateTicketCategoryStatusRequest(
    TicketCategoryStatus Status);

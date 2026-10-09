using ConcertTicket.Domain.Enums;

namespace ConcertTicket.Application.Vouchers.DTOs;

public sealed record UpdateVoucherStatusRequest(VoucherStatus Status);

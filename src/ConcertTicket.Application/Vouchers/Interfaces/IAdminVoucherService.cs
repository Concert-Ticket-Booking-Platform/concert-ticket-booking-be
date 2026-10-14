using ConcertTicket.Application.Common.Models;
using ConcertTicket.Application.Vouchers.DTOs;

namespace ConcertTicket.Application.Vouchers.Interfaces;

public interface IAdminVoucherService
{
    Task<AdminVoucherDto> CreateAsync(
        CreateVoucherRequest request,
        CancellationToken cancellationToken = default);

    //Task<IReadOnlyList<AdminVoucherDto>> GetAllAsync(
    //    CancellationToken cancellationToken = default);

    Task<PagedResult<AdminVoucherDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<AdminVoucherDto> GetByIdAsync(
        Guid voucherId,
        CancellationToken cancellationToken = default);

    Task<AdminVoucherDto> UpdateAsync(
        Guid voucherId,
        UpdateVoucherRequest request,
        CancellationToken cancellationToken = default);

    Task<AdminVoucherDto> UpdateStatusAsync(
        Guid voucherId,
        UpdateVoucherStatusRequest request,
        CancellationToken cancellationToken = default);
}

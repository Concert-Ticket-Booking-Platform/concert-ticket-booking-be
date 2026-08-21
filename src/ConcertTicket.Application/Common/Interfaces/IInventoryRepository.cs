namespace ConcertTicket.Application.Common.Interfaces
{
    public interface IInventoryRepository
    {
        Task<int> ReserveAsync(
        Guid ticketCategoryId,
        int quantity,
        CancellationToken cancellationToken = default);

        Task<int> ReleaseAsync(
            Guid ticketCategoryId,
            int quantity,
            CancellationToken cancellationToken = default);

        Task<int> ConfirmAsync(
            Guid ticketCategoryId,
            int quantity,
            CancellationToken cancellationToken = default);
    }
}

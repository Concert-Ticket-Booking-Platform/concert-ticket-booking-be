namespace ConcertTicket.Domain.Enums
{
    public enum BookingStatus
    {
        Received = 1,
        WaitingForPayment = 2,
        Paid = 3,
        Completed = 4,
        Expired = 5,
        Cancelled = 6
    }
}

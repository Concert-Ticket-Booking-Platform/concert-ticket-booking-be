namespace ConcertTicket.Domain.Enums
{
    public enum RoleType
    {
        /// <summary>
        /// Customer role, a user who can purchase tickets and attend events.
        /// </summary>
        Customer = 1,

        /// <summary>
        /// Operator role, responsible for managing events and ticket sales.
        /// </summary>
        Operator = 2,

        /// <summary>
        /// Admin role, with full access to the system and its settings.
        /// </summary>
        Admin = 3
    }
}

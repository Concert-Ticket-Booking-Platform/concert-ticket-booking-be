using System.Security.Cryptography;
using System.Text;

namespace ConcertTicket.Infrastructure.Payment.MoMo;

public static class MoMoHelper
{
    public static string HmacSha256(
        string secretKey,
        string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);

        return Convert.ToHexString(
            hmac.ComputeHash(dataBytes))
            .ToLowerInvariant();
    }

    public static bool VerifySignature(
        string secretKey,
        string data,
        string receivedSignature)
    {
        if (string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(receivedSignature))
        {
            return false;
        }

        var expectedSignature = HmacSha256(secretKey, data);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(
                receivedSignature.Trim().ToLowerInvariant()));
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace ConcertTicket.Infrastructure.Payment.VnPay;

public static class VnPayHelper
{
    public static string HmacSha512(
        string key,
        string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);

        var hash = hmac.ComputeHash(dataBytes);

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    public static string BuildHashData(
        SortedDictionary<string, string> parameters)
    {
        var builder = new StringBuilder();

        foreach (var pair in parameters)
        {
            if (string.IsNullOrEmpty(pair.Value))
                continue;

            if (builder.Length > 0)
                builder.Append('&');

            // Use application/x-www-form-urlencoded encoding for hash data (spaces => '+')
            builder.Append(WebUtility.UrlEncode(pair.Key));
            builder.Append('=');
            builder.Append(WebUtility.UrlEncode(pair.Value));
        }

        return builder.ToString();
    }

    public static string BuildQueryString(
        SortedDictionary<string, string> parameters)
    {
        var builder = new StringBuilder();

        foreach (var pair in parameters)
        {
            if (string.IsNullOrEmpty(pair.Value))
                continue;

            if (builder.Length > 0)
                builder.Append('&');

            builder.Append(WebUtility.UrlEncode(pair.Key));
            builder.Append('=');
            builder.Append(WebUtility.UrlEncode(pair.Value));
        }

        return builder.ToString();
    }

    public static bool VerifySignature(
        string hashSecret,
        IDictionary<string, string> parameters,
        string receivedSignature)
    {
        if (string.IsNullOrWhiteSpace(hashSecret) ||
            string.IsNullOrWhiteSpace(receivedSignature))
        {
            return false;
        }

        var sortedParameters =
            new SortedDictionary<string, string>(
                StringComparer.Ordinal);

        foreach (var pair in parameters)
        {
            if (pair.Key.Equals(
                    "vnp_SecureHash",
                    StringComparison.OrdinalIgnoreCase) ||
                pair.Key.Equals(
                    "vnp_SecureHashType",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(pair.Value))
                continue;

            sortedParameters[pair.Key] = pair.Value;
        }

        var hashData = BuildHashData(sortedParameters);

        var expectedSignature = HmacSha512(hashSecret, hashData);

        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
        var receivedBytes = Encoding.UTF8.GetBytes(receivedSignature.Trim().ToLowerInvariant());

        return CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }
}
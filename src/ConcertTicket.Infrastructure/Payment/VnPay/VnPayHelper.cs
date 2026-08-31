using System.Security.Cryptography;
using System.Text;

namespace ConcertTicket.Infrastructure.Payment.VnPay;

public static class VnPayHelper
{
    public static string HmacSha512(
        string key,
        string data)
    {
        var keyBytes =
            Encoding.UTF8.GetBytes(key);

        var dataBytes =
            Encoding.UTF8.GetBytes(data);

        using var hmac =
            new HMACSHA512(keyBytes);

        var hash =
            hmac.ComputeHash(dataBytes);

        return Convert.ToHexString(hash)
            .ToLowerInvariant();
    }

    public static string BuildQueryString(
        SortedDictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Value))
                .Select(x =>
                    $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    }

    //public static string BuildHashData(
    //    SortedDictionary<string, string> parameters)
    //{
    //    return string.Join(
    //        "&",
    //        parameters
    //            .Where(x =>
    //                !string.IsNullOrWhiteSpace(x.Value))
    //            .Select(x =>
    //                $"{x.Key}={x.Value}"));
    //}

    public static string BuildHashData(
        SortedDictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x =>
                    $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
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

        var hashParameters =
            new SortedDictionary<string, string>(
                StringComparer.Ordinal);

        foreach (var parameter in parameters)
        {
            if (string.Equals(
                    parameter.Key,
                    "vnp_SecureHash",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    parameter.Key,
                    "vnp_SecureHashType",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(parameter.Value))
            {
                continue;
            }

            hashParameters[parameter.Key] = parameter.Value;
        }

        var hashData = BuildHashData(hashParameters);

        var expectedSignature =
            HmacSha512(
                hashSecret,
                hashData);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(
                receivedSignature.Trim().ToLowerInvariant()));
    }
}
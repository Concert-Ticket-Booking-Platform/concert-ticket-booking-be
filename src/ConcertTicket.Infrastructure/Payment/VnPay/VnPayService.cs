using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Domain.Enums;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ConcertTicket.Infrastructure.Payment.VnPay;

public sealed class VnPayService : IPaymentProvider
{
    private readonly VnPayOptions _options;

    public VnPayService(
        IOptions<VnPayOptions> options)
    {
        _options = options.Value;
    }

    public PaymentProvider Provider =>
        PaymentProvider.VNPay;

    public Task<PaymentCreationResult> CreatePaymentAsync(
        PaymentCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        if (request.Amount <= 0)
        {
            return Task.FromResult(
                new PaymentCreationResult(
                    false,
                    string.Empty,
                    "Payment amount must be greater than zero."));
        }

        //if (string.IsNullOrWhiteSpace(request.IpAddress))
        //{
        //    return Task.FromResult(
        //        new PaymentCreationResult(
        //            false,
        //            string.Empty,
        //            "IP address is required."));
        //}

        var createDate =
            DateTimeOffset.UtcNow
                .ToOffset(TimeSpan.FromHours(7));

        var expireDate =
            request.ExpiresAt
                .ToOffset(TimeSpan.FromHours(7));

        if (expireDate <= createDate)
        {
            return Task.FromResult(
                new PaymentCreationResult(
                    false,
                    string.Empty,
                    "Payment expiration time is invalid."));
        }

        var amount =
            Convert.ToInt64(
                decimal.Round(
                    request.Amount * 100m,
                    0,
                    MidpointRounding.AwayFromZero));

        var parameters =
            new SortedDictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["vnp_Version"] = "2.1.0",

                ["vnp_Command"] = "pay",

                ["vnp_TmnCode"] = _options.TmnCode,

                ["vnp_Amount"] = amount.ToString(CultureInfo.InvariantCulture),

                ["vnp_CurrCode"] = "VND",

                ["vnp_TxnRef"] = request.OrderCode.ToString(CultureInfo.InvariantCulture),

                ["vnp_OrderInfo"] = request.Description,

                ["vnp_OrderType"] = "other",

                ["vnp_Locale"] = "vn",

                ["vnp_ReturnUrl"] = request.ReturnUrl,

                ["vnp_IpAddr"] = request.IpAddress,

                ["vnp_CreateDate"] = createDate.ToString("yyyyMMddHHmmss"),

                ["vnp_ExpireDate"] = expireDate.ToString("yyyyMMddHHmmss")
            };

        var hashData = VnPayHelper.BuildHashData(parameters);

        var secureHash = VnPayHelper.HmacSha512(
                _options.HashSecret,
                hashData);

        var query = VnPayHelper.BuildQueryString(parameters);

        // VNPay expects vnp_SecureHashType parameter present in query (not part of signed data)
        var paymentUrl =
            $"{_options.BaseUrl}?{query}" +
            $"&vnp_SecureHash={secureHash}";

        return Task.FromResult(
            new PaymentCreationResult(
                true,
                paymentUrl,
                null));
    }

    public Task<PaymentCallbackResult> ProcessCallbackAsync(
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        if (parameters is null ||
            parameters.Count == 0)
        {
            return Task.FromResult(
                new PaymentCallbackResult(
                    false,
                    false,
                    null,
                    null));
        }

        if (!parameters.TryGetValue(
                "vnp_SecureHash",
                out var receivedSignature))
        {
            return Task.FromResult(
                new PaymentCallbackResult(
                    false,
                    false,
                    null,
                    null));
        }

        var isValid =
            VnPayHelper.VerifySignature(
                _options.HashSecret,
                parameters,
                receivedSignature);

        if (!isValid)
        {
            return Task.FromResult(
                new PaymentCallbackResult(
                    false,
                    false,
                    null,
                    null));
        }

        parameters.TryGetValue(
            "vnp_TransactionNo",
            out var transactionReference);

        parameters.TryGetValue(
            "vnp_ResponseCode",
            out var responseCode);

        parameters.TryGetValue(
            "vnp_TransactionStatus",
            out var transactionStatus);

        var isSuccess =
            string.Equals(
                responseCode,
                "00",
                StringComparison.Ordinal) &&
            string.Equals(
                transactionStatus,
                "00",
                StringComparison.Ordinal);

        var responsePayload =
            System.Text.Json.JsonSerializer.Serialize(
                parameters);

        return Task.FromResult(
            new PaymentCallbackResult(
                true,
                isSuccess,
                transactionReference,
                responsePayload));
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
                _options.TmnCode))
        {
            throw new InvalidOperationException(
                "VNPay TmnCode is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.HashSecret))
        {
            throw new InvalidOperationException(
                "VNPay HashSecret is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.BaseUrl))
        {
            throw new InvalidOperationException(
                "VNPay BaseUrl is not configured.");
        }
    }
}

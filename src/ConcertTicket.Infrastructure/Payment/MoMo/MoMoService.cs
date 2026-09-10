using ConcertTicket.Application.Common.Interfaces;
using ConcertTicket.Application.Payments.DTOs;
using ConcertTicket.Domain.Enums;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConcertTicket.Infrastructure.Payment.MoMo;

public sealed class MoMoService : IPaymentProvider
{
    private const string RequestType = "captureWallet";

    private readonly MoMoOptions _options;
    private readonly HttpClient _httpClient;

    public MoMoService(
        IOptions<MoMoOptions> options,
        HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public PaymentProvider Provider => PaymentProvider.MoMo;

    public async Task<PaymentCreationResult> CreatePaymentAsync(
        PaymentCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        if (request.Amount <= 0)
        {
            return new PaymentCreationResult(
                false,
                string.Empty,
                "Payment amount must be greater than zero.");
        }

        var amount = decimal.ToInt64(
            decimal.Round(
                request.Amount,
                0,
                MidpointRounding.AwayFromZero));

        var orderId =
            request.OrderCode.ToString(
                CultureInfo.InvariantCulture);

        var requestId = request.PaymentId.ToString("N");

        var extraData = string.Empty;

        var signatureData =
            $"accessKey={_options.AccessKey}" +
            $"&amount={amount}" +
            $"&extraData={extraData}" +
            $"&ipnUrl={_options.IpnUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={request.Description}" +
            $"&partnerCode={_options.PartnerCode}" +
            $"&redirectUrl={_options.ReturnUrl}" +
            $"&requestId={requestId}" +
            $"&requestType={RequestType}";

        var signature =
            MoMoHelper.HmacSha256(
                _options.SecretKey,
                signatureData);

        var payload =
            new MoMoCreatePaymentRequest(
                PartnerCode: _options.PartnerCode,
                RequestType: RequestType,
                IpnUrl: _options.IpnUrl,
                RedirectUrl: _options.ReturnUrl,
                OrderId: orderId,
                Amount: amount,
                OrderInfo: request.Description,
                RequestId: requestId,
                ExtraData: extraData,
                Lang: "vi",
                Signature: signature);

        using var response =
            await _httpClient.PostAsJsonAsync(
                _options.PaymentUrl,
                payload,
                cancellationToken);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<MoMoCreatePaymentResponse>(
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new PaymentCreationResult(
                false,
                string.Empty,
                $"MoMo returned HTTP {(int)response.StatusCode}.");
        }

        if (responseBody is null)
        {
            return new PaymentCreationResult(
                false,
                string.Empty,
                "MoMo returned an empty response.");
        }

        if (responseBody.ResultCode != 0)
        {
            return new PaymentCreationResult(
                false,
                string.Empty,
                responseBody.Message);
        }

        if (string.IsNullOrWhiteSpace(
                responseBody.PayUrl))
        {
            return new PaymentCreationResult(
                false,
                string.Empty,
                "MoMo did not return a payment URL.");
        }

        return new PaymentCreationResult(
            true,
            responseBody.PayUrl,
            null);
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
                InvalidCallback());
        }

        if (!TryGetRequired(
                parameters,
                "signature",
                out var receivedSignature))
        {
            return Task.FromResult(
                InvalidCallback());
        }

        if (!TryGetRequired(
                parameters,
                "partnerCode",
                out var partnerCode) ||
            !string.Equals(
                partnerCode,
                _options.PartnerCode,
                StringComparison.Ordinal))
        {
            return Task.FromResult(
                InvalidCallback());
        }

        if (!TryGetRequired(
                parameters,
                "orderId",
                out var orderId))
        {
            return Task.FromResult(
                InvalidCallback());
        }

        if (!TryGetLong(
                parameters,
                "amount",
                out var amount))
        {
            return Task.FromResult(
                InvalidCallback());
        }

        var requestId =
            Get(parameters, "requestId");

        var orderInfo =
            Get(parameters, "orderInfo");

        var orderType =
            Get(parameters, "orderType");

        var transId =
            Get(parameters, "transId");

        var resultCode =
            Get(parameters, "resultCode");

        var message =
            Get(parameters, "message");

        var payType =
            Get(parameters, "payType");

        var responseTime =
            Get(parameters, "responseTime");

        var extraData =
            Get(parameters, "extraData");

        var signatureData =
            $"accessKey={_options.AccessKey}" +
            $"&amount={amount}" +
            $"&extraData={extraData}" +
            $"&message={message}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&orderType={orderType}" +
            $"&partnerCode={partnerCode}" +
            $"&payType={payType}" +
            $"&requestId={requestId}" +
            $"&responseTime={responseTime}" +
            $"&resultCode={resultCode}" +
            $"&transId={transId}";

        var isValid =
            MoMoHelper.VerifySignature(
                _options.SecretKey,
                signatureData,
                receivedSignature);

        if (!isValid)
        {
            return Task.FromResult(
                InvalidCallback());
        }

        var isSuccess =
            string.Equals(
                resultCode,
                "0",
                StringComparison.Ordinal);

        var responsePayload =
            JsonSerializer.Serialize(parameters);

        long? transactionReference = null;

        if (long.TryParse(
                transId,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsedTransId))
        {
            transactionReference = parsedTransId;
        }

        return Task.FromResult(
            new PaymentCallbackResult(
                IsValid: true,
                IsSuccess: isSuccess,
                Amount: amount,
                TransactionReference:
                    transactionReference?.ToString(
                        CultureInfo.InvariantCulture),
                ResponsePayload:
                    responsePayload));
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
                _options.PartnerCode))
        {
            throw new InvalidOperationException(
                "MoMo PartnerCode is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.AccessKey))
        {
            throw new InvalidOperationException(
                "MoMo AccessKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.SecretKey))
        {
            throw new InvalidOperationException(
                "MoMo SecretKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.PaymentUrl))
        {
            throw new InvalidOperationException(
                "MoMo PaymentUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.ReturnUrl))
        {
            throw new InvalidOperationException(
                "MoMo ReturnUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.IpnUrl))
        {
            throw new InvalidOperationException(
                "MoMo IpnUrl is not configured.");
        }
    }

    private static PaymentCallbackResult InvalidCallback()
    {
        return new PaymentCallbackResult(
            IsValid: false,
            IsSuccess: false,
            Amount: null,
            TransactionReference: null,
            ResponsePayload: null);
    }

    private static string Get(
        IDictionary<string, string> parameters,
        string key)
    {
        return parameters.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetRequired(
        IDictionary<string, string> parameters,
        string key,
        out string value)
    {
        if (parameters.TryGetValue(key, out var result) &&
            !string.IsNullOrWhiteSpace(result))
        {
            value = result;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetLong(
        IDictionary<string, string> parameters,
        string key,
        out long value)
    {
        return long.TryParse(
            Get(parameters, key),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);
    }
}

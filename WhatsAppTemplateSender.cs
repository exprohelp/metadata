using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class WhatsAppTemplateSender
{
    /// <summary>
    /// Sends the "diagnosticMKt" WhatsApp template message with a document header.
    /// </summary>
    /// <param name="httpClient">Configured HttpClient instance.</param>
    /// <param name="phoneNumberId">Meta WhatsApp Phone Number ID.</param>
    /// <param name="accessToken">Meta Graph API Bearer token.</param>
    /// <param name="toPhoneNumber">Recipient phone number in international format (e.g. 9198XXXXXXXX).</param>
    /// <param name="customerName">Template variable {{1}}.</param>
    /// <param name="doctorConsultationDiscount">Template variable {{2}} (e.g. "20%").</param>
    /// <param name="doctorCouponCode">Template variable {{3}}.</param>
    /// <param name="healthCheckupDiscount">Template variable {{4}} (e.g. "15%").</param>
    /// <param name="diagnosticCouponCode">Template variable {{5}}.</param>
    /// <param name="languageCode">Template language code (default: en).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Raw API response body as string.</returns>
    public static async Task<string> SendDiagnosticMktTemplateAsync(
        HttpClient httpClient,
        string phoneNumberId,
        string accessToken,
        string toPhoneNumber,
        string customerName,
        string doctorConsultationDiscount,
        string doctorCouponCode,
        string healthCheckupDiscount,
        string diagnosticCouponCode,
        string languageCode = "en",
        CancellationToken cancellationToken = default)
    {
        if (httpClient is null) throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrWhiteSpace(phoneNumberId)) throw new ArgumentException("Value is required.", nameof(phoneNumberId));
        if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Value is required.", nameof(accessToken));
        if (string.IsNullOrWhiteSpace(toPhoneNumber)) throw new ArgumentException("Value is required.", nameof(toPhoneNumber));

        var requestUri = $"https://graph.facebook.com/v20.0/{phoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "template",
            template = new
            {
                name = "diagnosticMKt",
                language = new
                {
                    code = languageCode
                },
                components = new object[]
                {
                    new
                    {
                        type = "header",
                        parameters = new object[]
                        {
                            new
                            {
                                type = "document",
                                document = new
                                {
                                    link = "https://chandandocs.s3.ap-south-1.amazonaws.com/ChandanProfile.pdf",
                                    filename = "ChandanProfile.pdf"
                                }
                            }
                        }
                    },
                    new
                    {
                        type = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = customerName },
                            new { type = "text", text = doctorConsultationDiscount },
                            new { type = "text", text = doctorCouponCode },
                            new { type = "text", text = healthCheckupDiscount },
                            new { type = "text", text = diagnosticCouponCode }
                        }
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"WhatsApp template send failed ({(int)response.StatusCode} {response.ReasonPhrase}): {responseBody}");
        }

        return responseBody;
    }
}

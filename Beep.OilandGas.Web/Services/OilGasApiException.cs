using System.Net;
using System.Text.Json;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Http;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// The OilGas API answered a request with something other than success: its status, its own sentence about it, and —
    /// when it failed rather than refused — the reference it filed that failure under (OILGAS-CATCH-01).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A page words this through <see cref="OilGasApiFailures"/>, never through the exception's message: the message is for
    /// the record; the person reads the API's sentence or, for a failure, what did not happen and the reference.
    /// </para>
    /// <para>
    /// <see cref="ApiClient"/> threw an <see cref="HttpRequestException"/> carrying the API's sentence as its message, which
    /// a page may not show, and its bool-returning calls answered every refusal, failure and lost connection alike as
    /// <c>false</c>, logged and reported nowhere. An <see cref="HttpRequestException"/> now means what it means in .NET: the
    /// request did not complete. The pattern is the Events application's (<c>EventsApiException</c>).
    /// </para>
    /// </remarks>
    public sealed class OilGasApiException : Exception
    {
        public OilGasApiException(HttpStatusCode statusCode, string? sentence, string? reference)
            : base(
                $"OilGas API returned {(int)statusCode}"
                + (string.IsNullOrWhiteSpace(sentence) ? "." : $": {sentence}")
                + (reference is null ? string.Empty : $" (its reference {reference})"))
        {
            StatusCode = statusCode;
            Sentence = string.IsNullOrWhiteSpace(sentence) ? null : sentence;
            Reference = reference;
        }

        public HttpStatusCode StatusCode { get; }

        /// <summary>What the API said, written for the person — a refusal's own words; null when it said nothing.</summary>
        public string? Sentence { get; }

        /// <summary>The reference the API filed its own failure under (its problem details' <c>reference</c>), when it failed.</summary>
        public string? Reference { get; }

        /// <summary>Whether the API failed rather than refused.</summary>
        public bool IsServerFailure => (int)StatusCode >= 500;

        /// <summary>The answer a non-success response carries — its sentence and its reference — read once, for every call.</summary>
        /// <remarks>
        /// The sentence is the problem details' <c>detail</c> (the API's refusal handler), or the <c>error</c> a controller
        /// answers with itself; the shared reader prefers <c>detail</c> to the generic <c>title</c>.
        /// </remarks>
        public static async Task<OilGasApiException> ReadAsync(
            HttpResponseMessage response, IFailureReporter failures, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(failures);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var reference = ReferenceIn(body, failures);

            // A failure's problem details carry only the framework's generic title; the person is told what did not happen
            // and the reference instead, so no sentence is taken from it.
            var sentence = (int)response.StatusCode >= 500 ? null : SentenceIn(body, response.StatusCode, failures);
            return new OilGasApiException(response.StatusCode, sentence, reference);
        }

        /// <summary>
        /// The API's own words about its answer, or null when it gave none: no body, or only the status's standard title (a
        /// bare problem answer's "Forbidden"). Those are the protocol's English, not a sentence for the person — the page
        /// then says what did not happen, with the reference.
        /// </summary>
        private static string? SentenceIn(string body, HttpStatusCode status, IFailureReporter failures)
        {
            var sentence = HttpResponseErrorMessage.TryReadFromBody(body, reasonPhrase: null, failures);
            return string.Equals(sentence, Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase((int)status), StringComparison.OrdinalIgnoreCase)
                ? null
                : sentence;
        }

        /// <summary>
        /// The reference the API filed its own failure under — its problem details' <c>reference</c> (the diagnostics
        /// library adds it to every problem answer) — or null for an answer that carries none.
        /// </summary>
        private static string? ReferenceIn(string body, IFailureReporter failures)
        {
            if (!body.TrimStart().StartsWith('{'))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                return document.RootElement.TryGetProperty("reference", out var reference)
                    && reference.ValueKind == JsonValueKind.String
                    && FailureReferenceFormat.IsWellFormed(reference.GetString())
                        ? reference.GetString()
                        : null;
            }
            catch (JsonException unreadable)
            {
                // System.Text.Json has no question to ask first: an answer that opens like JSON and is not.
                failures.ReportHandled(
                    unreadable,
                    "reading the reference from an OilGas API error answer",
                    consequence: "the person is shown this application's own reference instead of the API's",
                    FailureSeverity.Degraded);
                return null;
            }
        }
    }
}

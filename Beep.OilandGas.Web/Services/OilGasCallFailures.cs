using System.Net.Http;
using System.Text.Json;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Notifications;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// What a web service or a page's state answers with when work it did for a person failed and its contract is a result
    /// that says so — a <c>Success = false</c> result with its message, an <c>Error</c> the page renders — and the record of
    /// it (OILGAS-CATCH-01).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The services answered these failures with the exception's own text (<c>ErrorDetails = ex.Message</c>), logged and
    /// reported nowhere, so a fault's words reached the person and nothing reached the store. Every such answer is now a
    /// sentence the person can act on, and the failure is in the store under the reference the sentence quotes.
    /// </para>
    /// <para>
    /// An answer from the API (<see cref="OilGasApiException"/>) is said through <see cref="OilGasApiFailures"/>, in the
    /// API's own words when it refused and as what did not happen with its reference when it failed. Anything else — a
    /// request that did not complete, an answer that could not be read, a fault in this application — is reported here and
    /// said as what did not happen with the reference.
    /// </para>
    /// </remarks>
    public sealed class OilGasCallFailures(OilGasApiFailures answers, IFailureReporter failures, FailureWording wording)
    {
        /// <summary>
        /// Whether <paramref name="exception"/> is a call to the API that did not succeed: the API answered something other
        /// than success, the request did not complete, it timed out, or its answer could not be read. A service whose
        /// contract is a failed result catches exactly these; anything else is a fault in this application and propagates.
        /// </summary>
        public static bool IsCallFailure(Exception exception) =>
            exception is OilGasApiException
                or HttpRequestException
                or JsonException
                or TaskCanceledException { InnerException: TimeoutException };

        /// <summary>Reports <paramref name="failure"/> and returns the sentence to answer the person with.</summary>
        /// <param name="operation">What was being attempted, for the record: "saving the database connection".</param>
        /// <param name="whatDidNotHappen">In the person's terms, without a full stop: "The connection was not saved".</param>
        public string Explain(Exception failure, string operation, string whatDidNotHappen)
        {
            ArgumentNullException.ThrowIfNull(failure);
            ArgumentException.ThrowIfNullOrWhiteSpace(operation);
            ArgumentException.ThrowIfNullOrWhiteSpace(whatDidNotHappen);

            return failure is OilGasApiException answer
                ? answers.Explain(answer, operation, whatDidNotHappen)
                : wording.Reported(
                    whatDidNotHappen,
                    failures.ReportHandled(
                        failure,
                        operation,
                        consequence: $"{whatDidNotHappen}; the person was answered with a failed result quoting this reference"));
        }

        /// <summary>
        /// Reports <paramref name="failure"/> as told: the caller answers the person with <paramref name="sentence"/>, its
        /// own words for this case (a refusal it recognises, such as an expired session), and the record says so.
        /// </summary>
        public string Told(Exception failure, string operation, string sentence)
        {
            ArgumentNullException.ThrowIfNull(failure);
            ArgumentException.ThrowIfNullOrWhiteSpace(operation);
            ArgumentException.ThrowIfNullOrWhiteSpace(sentence);

            failures.ReportHandled(failure, operation, consequence: $"the person was told: {sentence}", FailureSeverity.Degraded);
            return sentence;
        }
    }
}

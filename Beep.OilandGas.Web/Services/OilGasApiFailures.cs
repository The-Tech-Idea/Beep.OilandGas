using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Notifications;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// What a person is told about an OilGas API answer that was not a success, and the record of it (OILGAS-CATCH-01).
    /// </summary>
    /// <remarks>
    /// The one wording for every page, dialog and component. A refusal is said in the API's own words. A failure — the API
    /// answered 5xx, or said nothing — is said as what did not happen, with the reference the API filed it under (it
    /// reported its own failure), or this record's when it gave none. Every answer is recorded here at
    /// <see cref="FailureSeverity.Degraded"/>: the person was told, and a server failure's own record is the API's.
    /// </remarks>
    public sealed class OilGasApiFailures(IFailureReporter failures, FailureWording wording)
    {
        /// <param name="answer">What the API answered.</param>
        /// <param name="operation">What was being attempted, for the record: "saving the well test".</param>
        /// <param name="whatDidNotHappen">In the person's terms, without a full stop, for a failure: "The well test was not saved".</param>
        /// <returns>The sentence to show the person.</returns>
        /// <remarks>It reports itself, so the analyzer that holds every catch to a report (BEEP0003) sees a catch calling it.</remarks>
        public string Explain(OilGasApiException answer, string operation, string whatDidNotHappen)
        {
            ArgumentNullException.ThrowIfNull(answer);
            ArgumentException.ThrowIfNullOrWhiteSpace(operation);
            ArgumentException.ThrowIfNullOrWhiteSpace(whatDidNotHappen);

            var told = answer.IsServerFailure || answer.Sentence is null
                ? $"{whatDidNotHappen}; the person was shown the reference"
                : "the person was shown the API's refusal";
            var reference = failures.ReportHandled(answer, operation, told, FailureSeverity.Degraded);

            return answer.IsServerFailure || answer.Sentence is null
                ? wording.Reported(whatDidNotHappen, answer.Reference ?? reference)
                : answer.Sentence;
        }
    }
}

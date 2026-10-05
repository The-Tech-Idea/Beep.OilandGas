using System;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.PPDM39.DataManagement.Core
{
    /// <summary>
    /// Reports a caught failure through the application's failure service and words what a result tells its caller about
    /// it: what did not happen, and the reference the full exception is stored under — never the exception's own text.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. The setup, seeding and import services answer with result objects (<c>Success</c>,
    /// <c>Message</c>, <c>Errors</c>) that the API returns and the web shows, and their catches put the exception's message
    /// into them: a provider's or the framework's words reached the person, and the failure itself was in a log line at best.
    /// The sentence is the shared diagnostics library's English wording (<c>FailureWording.Reported</c>), which the web shows
    /// for its own failures, so a person meets one form of words wherever the failure happened.
    /// </remarks>
    internal static class ReportedFailure
    {
        /// <summary>Reports <paramref name="exception"/> and returns the sentence the caller's result carries.</summary>
        /// <param name="failures">The application's failure service.</param>
        /// <param name="exception">What went wrong.</param>
        /// <param name="operation">What was being attempted, in words an operator recognises.</param>
        /// <param name="whatDidNotHappen">What the caller asked for and did not get, as a sentence.</param>
        /// <param name="severity">How much it matters.</param>
        [ReportsFailure]
        public static string Sentence(IFailureReporter failures, Exception exception, string operation,
            string whatDidNotHappen, FailureSeverity severity = FailureSeverity.Error)
        {
            ArgumentNullException.ThrowIfNull(failures);
            var reference = failures.ReportHandled(exception, operation,
                $"{whatDidNotHappen.TrimEnd().TrimEnd('.')}; the caller's result says so and carries the reference", severity);
            return Words(whatDidNotHappen, reference);
        }

        /// <summary>The sentence for a failure already reported under <paramref name="reference"/>.</summary>
        public static string Words(string whatDidNotHappen, string reference) =>
            $"{whatDidNotHappen.TrimEnd().TrimEnd('.').TrimEnd()}. Something went wrong on our side and it has been reported; " +
            $"if you contact support, quote reference {reference}.";
    }
}

using Beep.OilandGas.ApiService.Exceptions;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Posting the journal entry of a record a controller has already saved (OILGAS-CATCH-01).
    /// </summary>
    /// <remarks>
    /// The record and its journal entry are written in two steps, so a posting that fails leaves the record saved and
    /// unposted. The controllers caught a <see cref="GLPostingException"/> to say so — and nothing threw one, so the catch
    /// never ran and a failed posting was a bare failure that did not name the record. Here the posting's failure, whatever
    /// it was, becomes a <see cref="GLPostingException"/> naming the saved record and its module, with the cause inside: the
    /// API's handler reports it and answers 500 with its reference, and the reference leads whoever reads the failure store
    /// to the record that needs its entry.
    /// </remarks>
    public static class LedgerPosting
    {
        /// <summary>Posts a saved record's journal entry; a failure is raised as a <see cref="GLPostingException"/>.</summary>
        /// <param name="post">The posting, answering the journal entry's id.</param>
        /// <param name="savedRecord">The record already saved, in words ("Invoice INV-7").</param>
        /// <param name="transactionId">The saved record's id.</param>
        /// <param name="sourceModule">The module the entry is posted for.</param>
        public static async Task<string> PostAsync(Func<Task<string>> post, string savedRecord, string? transactionId, string sourceModule)
        {
            ArgumentNullException.ThrowIfNull(post);

            try
            {
                return await post();
            }
            // Whatever stopped the posting — the journal service, the database, a missing account — the record above it is
            // already saved, and the failure has to say which record is left without its entry. Cancellation is not a
            // posting failure and goes on as itself.
            catch (Exception postingFailure) when (postingFailure is not OperationCanceledException)
            {
                throw new GLPostingException($"{savedRecord} was saved, but its journal entry was not posted.",
                    transactionId ?? string.Empty, sourceModule, postingFailure);
            }
        }
    }
}

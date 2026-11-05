namespace Beep.OilandGas.Web.Services;

/// <summary>Transient knowledge about a pending original, separate from its authoritative business receipt.</summary>
internal sealed class RetainedOriginalFeedback
{
    public bool TabCopyConfirmed { get; private set; }
    public bool SubmissionMayHaveBeenSent { get; private set; }
    private bool saving;
    private bool? recorded;

    public void Reset() { TabCopyConfirmed = false; SubmissionMayHaveBeenSent = false; recorded = null; saving = false; }
    public void BeginSaving() { Reset(); saving = true; }
    public void ReadFromTab() { Reset(); TabCopyConfirmed = true; SubmissionMayHaveBeenSent = true; }
    public void ConfirmSaved() { saving = false; TabCopyConfirmed = true; }
    public void Failed() => saving = false;
    public void AttemptSubmission() => SubmissionMayHaveBeenSent = true;
    public void ConfirmReceiptAndBeginClear(bool wasRecorded)
    {
        recorded = wasRecorded;
        saving = false;
        // Removal is in progress; do not promise that a saved copy still exists if its acknowledgement is lost.
        TabCopyConfirmed = false;
    }
    public string Title(string subject) => saving ? "Saving original " + subject + " in this tab" :
        recorded.HasValue ? "Original " + subject + " awaiting input cleanup" :
        TabCopyConfirmed ? "Retained original " + subject : "Original " + subject + " kept on this page";
    public string Explanation => saving ? "Saving the original in this tab. No submission has been sent." : FailureExplanation;
    public string FailureExplanation => recorded.HasValue ?
        "The last validated receipt confirmed this original was " + (recorded.Value ? "recorded" : "absent") +
            ". Clearing the tab input is not confirmed. Keep this page open and recover again to finish." :
        !SubmissionMayHaveBeenSent ?
            "This tab could not confirm saving the original. No submission was sent from this page. Keep this page open and recover the original before trying again." :
        TabCopyConfirmed ?
            "The original is saved in this tab. A submission may have reached the server. Recover it before preparing another." :
            "The saved tab copy is unconfirmed. A submission may have reached the server. Keep this page open and recover its original.";
}

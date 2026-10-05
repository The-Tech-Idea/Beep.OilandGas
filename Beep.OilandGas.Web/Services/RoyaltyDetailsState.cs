using System.Net;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Web.Services;

/// <summary>Page state with independent payment/review errors and stale-response protection.</summary>
public sealed class RoyaltyDetailsState(IAccountingServiceClient client, OilGasCallFailures failures) : IDisposable
{
    public ROYALTY_CALCULATION? Calculation { get; private set; }
    public List<ROYALTY_PAYMENT>? Payments { get; private set; }
    public List<RoyaltyPostingReview>? Review { get; private set; }
    public bool Loading { get; private set; }
    public bool Reviewing { get; private set; }
    public string? Error { get; private set; }
    public string? PaymentError { get; private set; }
    public string? ReviewError { get; private set; }
    private CancellationTokenSource? _request;
    private int _version;
    private bool _disposed;

    public async Task LoadAsync(string id)
    {
        if (_disposed) return;
        var version = ++_version;
        _request?.Cancel(); _request?.Dispose();
        _request = new();
        var token = _request.Token;
        Calculation = null; Payments = null; Review = null;
        Error = null; PaymentError = null; ReviewError = null;
        Loading = true; Reviewing = false;
        try
        {
            var calculationCall = client.GetRoyaltyAsync(id, token);
            if (await SupersededAsync(calculationCall, token)) return;
            var calculation = await calculationCall;
            if (!Current(version)) return;
            Calculation = calculation;
            try
            {
                var paymentsCall = client.GetRoyaltyPaymentsAsync(id, token);
                if (await SupersededAsync(paymentsCall, token)) return;
                var payments = await paymentsCall;
                if (Current(version)) Payments = payments;
            }
            // The page renders a sentence for every failure of this part, beside the details it did load; the store keeps
            // the exception (a superseded load's too, though nobody is shown it).
            catch (Exception ex)
            {
                var sentence = Explain(ex, "loading a royalty's payment history", "Payment history");
                if (Current(version)) PaymentError = sentence;
            }
        }
        // The page renders a sentence for every failure of this load, in place of the details; the store keeps the
        // exception (a superseded load's too, though nobody is shown it).
        catch (Exception ex)
        {
            var sentence = Explain(ex, "loading a royalty calculation", "Royalty details");
            if (Current(version)) Error = sentence;
        }
        finally { if (Current(version)) Loading = false; }
    }

    public async Task LoadReviewAsync()
    {
        if (_disposed || Loading || Reviewing || Calculation is null) return;
        var version = _version;
        var token = _request!.Token;
        Reviewing = true; Review = null; ReviewError = null;
        try
        {
            var reviewCall = client.GetRoyaltyPostingReviewAsync(Calculation.ROYALTY_CALCULATION_ID, token);
            if (await SupersededAsync(reviewCall, token)) return;
            var review = await reviewCall;
            if (Current(version)) Review = review;
        }
        // The page renders a sentence for every failure of the review, beside the details; the store keeps the exception.
        catch (Exception ex)
        {
            var sentence = Explain(ex, "loading a royalty's posting review", "Posting review");
            if (Current(version)) ReviewError = sentence;
        }
        finally { if (Current(version)) Reviewing = false; }
    }

    private bool Current(int version) => !_disposed && version == _version;

    /// <summary>
    /// Whether <paramref name="call"/> ended because this state cancelled it — a newer royalty was asked for, or the page
    /// left. Asked of the finished call rather than caught: nobody is waiting for its answer, so there is nothing to tell.
    /// A call that timed out was not cancelled by this state and is awaited, to fail like any other.
    /// </summary>
    private static async Task<bool> SupersededAsync(Task call, CancellationToken token)
    {
        await Task.WhenAny(call);
        return call.IsCanceled && token.IsCancellationRequested;
    }

    /// <summary>Reports <paramref name="ex"/> and returns the sentence for the part of the page that did not load.</summary>
    [TheTechIdeaWeb.Diagnostics.ReportsFailure]
    private string Explain(Exception ex, string operation, string subject) => ex switch
    {
        OilGasApiException { StatusCode: HttpStatusCode.Unauthorized } =>
            failures.Told(ex, operation, "Your session has expired. Sign in again to continue."),
        OilGasApiException { StatusCode: HttpStatusCode.Forbidden } =>
            failures.Told(ex, operation, $"You do not have access to {subject.ToLowerInvariant()}. Contact your app administrator if you need access."),
        OilGasApiException { StatusCode: HttpStatusCode.NotFound } =>
            failures.Told(ex, operation, $"{subject} could not be found. Return to the royalty list and refresh it."),
        OilGasApiException { StatusCode: HttpStatusCode.Conflict } =>
            failures.Told(ex, operation, $"{subject} needs reconciliation. Refresh the records or contact your accounting administrator."),
        _ => failures.Explain(ex, operation, $"{subject} could not be loaded"),
    };

    public void Dispose()
    {
        _disposed = true; ++_version;
        _request?.Cancel(); _request?.Dispose();
        Calculation = null; Payments = null; Review = null;
    }
}

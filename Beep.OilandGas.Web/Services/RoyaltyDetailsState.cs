using System.Net;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Web.Services;

/// <summary>Page state with independent payment/review errors and stale-response protection.</summary>
public sealed class RoyaltyDetailsState(IAccountingServiceClient client) : IDisposable
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
            var calculation = await client.GetRoyaltyAsync(id, token);
            if (!Current(version)) return;
            Calculation = calculation;
            try
            {
                var payments = await client.GetRoyaltyPaymentsAsync(id, token);
                if (Current(version)) Payments = payments;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { if (Current(version)) PaymentError = Message(ex, "Payment history"); }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex) { if (Current(version)) Error = Message(ex, "Royalty details"); }
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
            var review = await client.GetRoyaltyPostingReviewAsync(Calculation.ROYALTY_CALCULATION_ID, token);
            if (Current(version)) Review = review;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception ex) { if (Current(version)) ReviewError = Message(ex, "Posting review"); }
        finally { if (Current(version)) Reviewing = false; }
    }

    private bool Current(int version) => !_disposed && version == _version;
    private static string Message(Exception ex, string subject) => ex is HttpRequestException http ? http.StatusCode switch {
        HttpStatusCode.Unauthorized => "Your session has expired. Sign in again to continue.",
        HttpStatusCode.Forbidden => $"You do not have access to {subject.ToLowerInvariant()}. Contact your app administrator if you need access.",
        HttpStatusCode.NotFound => $"{subject} could not be found. Return to the royalty list and refresh it.",
        HttpStatusCode.Conflict => $"{subject} needs reconciliation. Refresh the records or contact your accounting administrator.",
        _ => $"{subject} could not be loaded. Please retry."
    } : $"{subject} could not be loaded. Please retry.";

    public void Dispose()
    {
        _disposed = true; ++_version;
        _request?.Cancel(); _request?.Dispose();
        Calculation = null; Payments = null; Review = null;
    }
}

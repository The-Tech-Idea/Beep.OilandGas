using System.Net;
using System.Net.Http;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>
/// Resolves and reports the access mode over the settings the host supplies.
/// </summary>
/// <remarks>
/// <para>
/// The health probe is deliberately small: for <see cref="OgResolvedAccess.Local"/> it asks whether the
/// local database is still there, and for <see cref="OgResolvedAccess.Remote"/> it calls
/// <c>{base}/health</c> and tells "no answer" apart from "refused this caller" (FND-02 AC-6). Those are the
/// three states a person can act on; anything finer belongs to the module that owns the read.
/// </para>
/// <para>
/// A health check that cannot reach the host is an ordinary answer, not a crash — but it is still reported
/// through <see cref="IFailureReporter"/> so the evidence exists if the same failure later stops a read.
/// </para>
/// </remarks>
public sealed class OgServiceAccess : IOgServiceAccess
{
    private readonly IOgAuditService _audit;
    private readonly IFailureReporter _failures;
    private readonly Func<string, bool> _localDatabaseExists;
    private readonly Func<HttpMessageHandler>? _handlerFactory;
    private OgServiceAccessSettings _settings;

    public OgServiceAccess(
        OgServiceAccessSettings settings,
        IOgAuditService audit,
        IFailureReporter failures,
        Func<string, bool> localDatabasePresent,
        Func<HttpMessageHandler>? handlerFactory = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _localDatabaseExists = localDatabasePresent ?? throw new ArgumentNullException(nameof(localDatabasePresent));
        _handlerFactory = handlerFactory;
    }

    public OgServiceAccessSettings Settings => _settings;

    public OgResolvedAccess Mode => OgAccessResolver.Resolve(_settings).Mode;

    public string Reason => OgAccessResolver.Resolve(_settings).Reason;

    public Task<OgResult> ConfigureAsync(
        OgServiceAccessSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        var previous = OgAccessResolver.Resolve(_settings);
        var next = OgAccessResolver.Resolve(settings);

        _settings = settings;

        _audit.Add(
            "access.switch",
            "access-mode",
            next.Mode.ToString(),
            "ok",
            $"previous={previous.Mode}; preference={settings.Preference}; {next.Reason}");

        return Task.FromResult(OgResult.Ok(next.Reason));
    }

    public async Task<OgResult<OgHealthSnapshot>> CheckHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var resolution = OgAccessResolver.Resolve(_settings);

        try
        {
            return resolution.Mode switch
            {
                OgResolvedAccess.Local => CheckLocal(resolution),
                OgResolvedAccess.Remote => await CheckRemoteAsync(resolution, cancellationToken).ConfigureAwait(false),
                OgResolvedAccess.NotConfigured => OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
                    OgResolvedAccess.NotConfigured,
                    OgAccessHealth.NotConfigured,
                    resolution.Reason)),
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"checking {resolution.Mode} access health",
                "the health of the data path is unknown",
                FailureSeverity.Degraded);

            // The check itself succeeded — it produced an answer: the path is not reachable. Returning a
            // failed result here would make the caller show "the check broke" instead of "the data path
            // is down", which is the distinction FND-02 AC-6 exists for. Only the reference is quoted.
            var state = resolution.Mode == OgResolvedAccess.Remote
                ? OgAccessHealth.Unreachable
                : OgAccessHealth.DatabaseMissing;

            return OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
                resolution.Mode,
                state,
                $"The data path could not be checked. Reference: {reference}"));
        }
    }

    private OgResult<OgHealthSnapshot> CheckLocal(OgServiceAccessResolution resolution)
    {
        var connectionName = _settings.LocalConnectionName ?? string.Empty;
        var present = _localDatabaseExists(connectionName);

        return OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
            resolution.Mode,
            present ? OgAccessHealth.Ok : OgAccessHealth.DatabaseMissing,
            present
                ? $"The local database '{connectionName}' is present."
                : $"The local database '{connectionName}' is missing; repair through the first-run window."));
    }

    private async Task<OgResult<OgHealthSnapshot>> CheckRemoteAsync(
        OgServiceAccessResolution resolution,
        CancellationToken cancellationToken)
    {
        var baseUrl = _settings.CompanyApiBaseUrl!;
        var healthUri = new Uri(new Uri(baseUrl, UriKind.Absolute), "health");

        using var handler = _handlerFactory?.Invoke();
        using var client = handler is null ? new HttpClient() : new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, healthUri);

        // The token itself is never held here: FND-07 supplies it to the API client, and this probe only
        // needs to know whether one exists so the mode can be resolved. No credential is fabricated.
        using var response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
                resolution.Mode,
                OgAccessHealth.Unauthorized,
                $"The API refused this caller ({ (int)response.StatusCode }). Sign in again; the application will not retry silently."));
        }

        return response.IsSuccessStatusCode
            ? OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
                resolution.Mode,
                OgAccessHealth.Ok,
                $"The API answered { (int)response.StatusCode }."))
            : OgResult<OgHealthSnapshot>.Ok(new OgHealthSnapshot(
                resolution.Mode,
                OgAccessHealth.Unreachable,
                $"The API answered { (int)response.StatusCode }, which is not a health response."));
    }
}

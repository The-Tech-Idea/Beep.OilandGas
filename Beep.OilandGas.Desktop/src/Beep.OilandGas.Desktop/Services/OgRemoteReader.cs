using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>How a remote business read ended (FND-02 AC-3).</summary>
public enum OgRemoteReadOutcome
{
    /// <summary>The API answered with a readable payload.</summary>
    Ok,

    /// <summary>No base URL or usable token: nothing was sent.</summary>
    NotConfigured,

    /// <summary>The API refused this caller (401/403): sign in again, do not retry blindly.</summary>
    Unauthorized,

    /// <summary>The API answered with an error status or an unreadable payload.</summary>
    ServerError,

    /// <summary>No answer at all: DNS, refused connection, timeout.</summary>
    Unreachable,

    /// <summary>The API answered 2xx but the body was missing or not the expected JSON.</summary>
    InvalidPayload,
}

/// <summary>
/// One remote read's outcome. A failed read is a <b>failure</b>, never an empty success: <see cref="Outcome"/>
/// says which failure, and <see cref="Items"/> is only meaningful when it is <see cref="OgRemoteReadOutcome.Ok"/>.
/// </summary>
public sealed record OgRemoteReadResult<T>(
    OgRemoteReadOutcome Outcome,
    IReadOnlyList<T> Items,
    string Message,
    string? Reference = null)
{
    public bool Success => Outcome == OgRemoteReadOutcome.Ok;

    /// <summary>One line for the window, the audit row or the smoke record.</summary>
    public string Describe() => $"{Outcome}: {Message}" + (Reference is null ? string.Empty : $" ({Reference})");
}

/// <summary>
/// FND-02 AC-3: the desktop's first business read against the company API, with the failure states a person can
/// act on instead of an empty list.
/// </summary>
/// <remarks>
/// <para>
/// The distinction matters: "the well list is empty" and "the API refused the caller" look identical if a failure
/// is flattened to <c>[]</c>. This reader returns an outcome, and a caller must check it before showing rows.
/// </para>
/// <para>
/// A network failure is reported through <see cref="IFailureReporter"/> and the result quotes only the reference;
/// an authentication failure is not reported as a crash, because it is an ordinary answer that needs a sign-in.
/// </para>
/// </remarks>
public sealed class OgRemoteReader
{
    private readonly Func<OgServiceAccessSettings> _settings;
    private readonly IFailureReporter _failures;
    private readonly string? _accessToken;
    private readonly Func<HttpMessageHandler>? _handlerFactory;

    public OgRemoteReader(
        OgServiceAccessSettings settings,
        IFailureReporter failures,
        string? accessToken = null,
        Func<HttpMessageHandler>? handlerFactory = null)
        : this(() => settings, failures, accessToken, handlerFactory)
    {
        ArgumentNullException.ThrowIfNull(settings);
    }

    /// <summary>
    /// Reads the settings on every call, so switching the access mode does not leave a reader pointing at the
    /// base URL it was created with.
    /// </summary>
    public OgRemoteReader(
        Func<OgServiceAccessSettings> settings,
        IFailureReporter failures,
        string? accessToken = null,
        Func<HttpMessageHandler>? handlerFactory = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _accessToken = accessToken;
        _handlerFactory = handlerFactory;
    }

    /// <summary>Reads a JSON array from <paramref name="relativeUri"/> under the configured API base URL.</summary>
    public async Task<OgRemoteReadResult<T>> ReadAsync<T>(
        string relativeUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            throw new ArgumentException("A read needs a relative URI.", nameof(relativeUri));
        }

        var settings = _settings();

        if (!settings.HasApi || string.IsNullOrWhiteSpace(settings.CompanyApiBaseUrl))
        {
            return new OgRemoteReadResult<T>(
                OgRemoteReadOutcome.NotConfigured,
                [],
                "No company API is configured, so nothing was read. Switch the access mode or configure the API.");
        }

        var baseUri = new Uri(settings.CompanyApiBaseUrl!, UriKind.Absolute);
        var uri = new Uri(baseUri, relativeUri);

        using var handler = _handlerFactory?.Invoke();
        using var client = handler is null ? new HttpClient() : new HttpClient(handler);

        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        HttpResponseMessage response;

        try
        {
            response = await client
                .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading {uri.AbsolutePath} from the company API",
                "the read failed and no rows were shown",
                FailureSeverity.Degraded);

            return new OgRemoteReadResult<T>(
                OgRemoteReadOutcome.Unreachable,
                [],
                "The company API did not answer. The data path is down; nothing was shown rather than an empty list.",
                reference);
        }

        using (response)
        {
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new OgRemoteReadResult<T>(
                    OgRemoteReadOutcome.Unauthorized,
                    [],
                    $"The company API refused this caller ({(int)response.StatusCode}). Sign in again; the read was not retried.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return new OgRemoteReadResult<T>(
                    OgRemoteReadOutcome.ServerError,
                    [],
                    $"The company API answered {(int)response.StatusCode}; no rows were read.");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(body))
            {
                return new OgRemoteReadResult<T>(
                    OgRemoteReadOutcome.InvalidPayload,
                    [],
                    "The company API answered but sent no body; a missing body is a failure, not an empty result.");
            }

            try
            {
                var items = JsonSerializer.Deserialize<List<T>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (items is null)
                {
                    return new OgRemoteReadResult<T>(
                        OgRemoteReadOutcome.InvalidPayload,
                        [],
                        "The company API sent JSON that is not a list of the expected records.");
                }

                return new OgRemoteReadResult<T>(
                    OgRemoteReadOutcome.Ok,
                    items,
                    items.Count == 0
                        ? "The company API answered: no records match."
                        : $"The company API answered: {items.Count} record(s).");
            }
            catch (JsonException ex)
            {
                var reference = _failures.ReportHandled(
                    ex,
                    $"parsing {uri.AbsolutePath} from the company API",
                    "the read failed and no rows were shown",
                    FailureSeverity.Degraded);

                return new OgRemoteReadResult<T>(
                    OgRemoteReadOutcome.InvalidPayload,
                    [],
                    "The company API sent a payload this build could not read; no rows were shown.",
                    reference);
            }
        }
    }
}

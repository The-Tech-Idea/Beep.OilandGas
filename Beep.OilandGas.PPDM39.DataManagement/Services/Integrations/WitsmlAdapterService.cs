using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Integrations;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Integrations;

/// <summary>
/// Pulls well, casing and log data from a WITSML 1.4.1 SOAP server
/// and upserts records into PPDM WELL, CASING_PROGRAM and LOG tables.
/// Credentials are read from config; never hard-coded.
/// </summary>
/// <remarks>
/// OILGAS-CATCH-01. A sync answers with its result: a paused connection (the circuit breaker open) is said in this
/// adapter's words, and any other failure counts against the circuit breaker, is reported, and is answered with the
/// reference — the result, the sync history and the log carried the exception's own text. Listing the server's wells has
/// no result to carry a failure, so it lets the failure reach its caller (after counting it), where it used to answer an
/// empty list that read as "the server has no wells".
/// </remarks>
public class WitsmlAdapterService : IWitsmlAdapter
{
    private const string AdapterName = "WITSML";

    private readonly IIntegrationHealthService  _health;
    private readonly IDMEEditor                 _editor;
    private readonly ICommonColumnHandler       _cch;
    private readonly IPPDM39DefaultsRepository  _defaults;
    private readonly IPPDMMetadataRepository    _metadata;
    private readonly string                     _connectionName;
    private readonly ILogger<WitsmlAdapterService> _logger;
    private readonly IFailureReporter           _failures;

    // Config (passed from DI via factory)
    private readonly string  _serverUrl;
    private readonly string  _username;
    private readonly string  _password;
    private readonly int     _timeoutSeconds;

    public WitsmlAdapterService(
        IIntegrationHealthService    health,
        IDMEEditor                   editor,
        ICommonColumnHandler         cch,
        IPPDM39DefaultsRepository    defaults,
        IPPDMMetadataRepository      metadata,
        string                       connectionName,
        string                       serverUrl,
        string                       username,
        string                       password,
        int                          timeoutSeconds,
        ILogger<WitsmlAdapterService> logger,
        IFailureReporter             failures)
    {
        _health         = health;
        _editor         = editor;
        _cch            = cch;
        _defaults       = defaults;
        _metadata       = metadata;
        _connectionName = connectionName;
        _serverUrl      = serverUrl;
        _username       = username;
        _password       = password;
        _timeoutSeconds = timeoutSeconds;
        _logger         = logger;
        _failures       = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    // ── IWitsmlAdapter ────────────────────────────────────────────────────────

    public async Task<List<WitsmlWellSummary>> GetAvailableWellsAsync(string serverUrl)
    {
        var url = string.IsNullOrWhiteSpace(serverUrl) ? _serverUrl : serverUrl;
        CheckCircuitBreaker();
        try
        {
            var xml     = BuildGetFromStoreRequest("well", "<well/>");
            var rawXml  = await PostSoapAsync(url, "WMLS_GetFromStore", xml);
            var results = ParseWellsSummary(rawXml);
            _health.RecordSuccess(AdapterName);
            return results;
        }
        // Broad: every failure of the call — the HTTP client, the server's answer, its XML — counts against the circuit
        // breaker. It is then rethrown, for the caller's handler to report.
        catch (Exception ex) when (ex is not RefusalException)
        {
            _health.RecordFailure(AdapterName, ex);
            throw;
        }
    }

    public async Task<WitsmlSyncResult> SyncWellAsync(
        string witsmlWellUid, string fieldId, string userId)
    {
        var paused = Paused();
        if (paused != null)
            return new WitsmlSyncResult(false, "WELL", witsmlWellUid, 0, paused);

        try
        {
            var queryXml = $"<wells><well uid=\"{witsmlWellUid}\"/></wells>";
            var rawXml   = await PostSoapAsync(_serverUrl, "WMLS_GetFromStore",
                               BuildGetFromStoreRequest("well", queryXml));

            var wellDoc = XDocument.Parse(rawXml);
            var wellEl  = wellDoc.Descendants("well").FirstOrDefault();
            if (wellEl is null)
                return new WitsmlSyncResult(false, "WELL", witsmlWellUid, 0, "Well element not found in response");

            var mapped = MapWellElement(wellEl, fieldId);

            var repo = await MakeRepoAsync("WELL");
            var existing = (await repo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "WITSML_UID", Operator = "=", FilterValue = witsmlWellUid }
            })).FirstOrDefault();

            string targetId;
            if (existing is not null)
            {
                SetProperty(existing, "WELL_NAME",        GetProp(mapped, "WELL_NAME"));
                SetProperty(existing, "SPUD_DATE",        GetProp(mapped, "SPUD_DATE"));
                SetProperty(existing, "SURFACE_COUNTRY",  GetProp(mapped, "SURFACE_COUNTRY"));
                await repo.UpdateAsync(existing, userId);
                targetId = GetStrProp(existing, "UWI");
            }
            else
            {
                await repo.InsertAsync(mapped, userId);
                targetId = GetStrProp(mapped, "UWI");
            }

            _health.RecordSuccess(AdapterName);
            _health.AppendHistory(new IntegrationSyncHistoryEntry(
                AdapterName, DateTime.UtcNow, true, 1, null));

            return new WitsmlSyncResult(true, "WELL", targetId, 1, null);
        }
        // Broad: a sync answers every failure of the call and of the write in its result (see the class remarks).
        catch (Exception ex) when (ex is not RefusalException)
        {
            var sentence = Failed(ex, $"syncing WITSML well {witsmlWellUid}", $"WITSML well {witsmlWellUid} was not synced.");
            return new WitsmlSyncResult(false, "WELL", witsmlWellUid, 0, sentence);
        }
    }

    public async Task<WitsmlSyncResult> SyncCasingAsync(
        string witsmlWellUid, string witsmlWellboreUid, string userId)
    {
        var paused = Paused();
        if (paused != null)
            return new WitsmlSyncResult(false, "CASING_PROGRAM", witsmlWellUid, 0, paused);

        try
        {
            var queryXml = $"<casing><well uid=\"{witsmlWellUid}\"><wellbore uid=\"{witsmlWellboreUid}\"/></well></casing>";
            var rawXml   = await PostSoapAsync(_serverUrl, "WMLS_GetFromStore",
                               BuildGetFromStoreRequest("casingSchematic", queryXml));

            var doc   = XDocument.Parse(rawXml);
            var cases = doc.Descendants("casingSchematic").ToList();

            var repo  = await MakeRepoAsync("CASING_PROGRAM");
            int count = 0;
            foreach (var el in cases)
            {
                var mapped = MapCasingElement(el, witsmlWellUid, witsmlWellboreUid);
                await repo.InsertAsync(mapped, userId);
                count++;
            }

            _health.RecordSuccess(AdapterName);
            _health.AppendHistory(new IntegrationSyncHistoryEntry(
                AdapterName, DateTime.UtcNow, true, count, null));
            return new WitsmlSyncResult(true, "CASING_PROGRAM", witsmlWellUid, count, null);
        }
        // Broad: as for a well.
        catch (Exception ex) when (ex is not RefusalException)
        {
            var sentence = Failed(ex, $"syncing WITSML casing for well {witsmlWellUid}, wellbore {witsmlWellboreUid}",
                $"The casing of WITSML well {witsmlWellUid} was not synced.");
            return new WitsmlSyncResult(false, "CASING_PROGRAM", witsmlWellUid, 0, sentence);
        }
    }

    public async Task<WitsmlSyncResult> SyncLogAsync(
        string witsmlWellUid, string witsmlWellboreUid, string logUid, string userId)
    {
        var paused = Paused();
        if (paused != null)
            return new WitsmlSyncResult(false, "LOG", logUid, 0, paused);

        try
        {
            var queryXml = $"<logs><well uid=\"{witsmlWellUid}\"><wellbore uid=\"{witsmlWellboreUid}\"><log uid=\"{logUid}\"/></wellbore></well></logs>";
            var rawXml   = await PostSoapAsync(_serverUrl, "WMLS_GetFromStore",
                               BuildGetFromStoreRequest("log", queryXml));

            var doc    = XDocument.Parse(rawXml);
            var logEl  = doc.Descendants("log").FirstOrDefault();
            if (logEl is null)
                return new WitsmlSyncResult(false, "LOG", logUid, 0, "Log element not found");

            var repo   = await MakeRepoAsync("LOG");
            var mapped = MapLogElement(logEl, witsmlWellUid, witsmlWellboreUid);
            await repo.InsertAsync(mapped, userId);

            _health.RecordSuccess(AdapterName);
            _health.AppendHistory(new IntegrationSyncHistoryEntry(
                AdapterName, DateTime.UtcNow, true, 1, null));
            return new WitsmlSyncResult(true, "LOG", logUid, 1, null);
        }
        // Broad: as for a well.
        catch (Exception ex) when (ex is not RefusalException)
        {
            var sentence = Failed(ex, $"syncing WITSML log {logUid}", $"WITSML log {logUid} was not synced.");
            return new WitsmlSyncResult(false, "LOG", logUid, 0, sentence);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Counts a failed call against the circuit breaker, records it in the sync history, reports it, and answers the
    /// sentence the result carries.
    /// </summary>
    [ReportsFailure]
    private string Failed(Exception exception, string operation, string whatDidNotHappen)
    {
        var sentence = ReportedFailure.Sentence(_failures, exception, operation, whatDidNotHappen);
        _health.RecordFailure(AdapterName, exception);
        _health.AppendHistory(new IntegrationSyncHistoryEntry(AdapterName, DateTime.UtcNow, false, 0, sentence));
        return sentence;
    }

    /// <summary>The sentence for a paused connection (the circuit breaker open); null when calls may go through.</summary>
    private string? Paused() =>
        GetStatusSync().State == CircuitBreakerState.Open
            ? $"The {AdapterName} connection is paused after repeated failures; try again in a few minutes."
            : null;

    private void CheckCircuitBreaker()
    {
        var paused = Paused();
        if (paused != null)
            throw new IntegrationUnavailableException(paused);
    }

    private AdapterHealthStatus GetStatusSync()
        => _health.GetStatusAsync(AdapterName).GetAwaiter().GetResult();

    private async Task<string> PostSoapAsync(string url, string action, string bodyXml)
    {
        using var client  = new HttpClient { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
        var credentials   = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_username}:{_password}"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var soapEnvelope =
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>{bodyXml}</soap:Body>
            </soap:Envelope>
            """;

        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"\"http://www.witsml.org/action/141/{action}\"");

        var response = await client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static string BuildGetFromStoreRequest(string wmlTypeIn, string queryIn)
        => $"""
            <WMLS_GetFromStore xmlns="http://www.witsml.org/message/141">
              <WMLtypeIn>{wmlTypeIn}</WMLtypeIn>
              <QueryIn>{System.Security.SecurityElement.Escape(queryIn)}</QueryIn>
            </WMLS_GetFromStore>
            """;

    private static List<WitsmlWellSummary> ParseWellsSummary(string xml)
    {
        var doc = XDocument.Parse(xml);
        return doc.Descendants("well").Select(e => new WitsmlWellSummary(
            Uid:      e.Attribute("uid")?.Value    ?? string.Empty,
            Name:     e.Element("name")?.Value     ?? string.Empty,
            Country:  e.Element("country")?.Value  ?? string.Empty,
            Operator: e.Element("operator")?.Value ?? string.Empty,
            SpudDate: DateTime.TryParse(e.Element("spudDate")?.Value, out var dt) ? dt : null
        )).ToList();
    }

    private static object MapWellElement(XElement el, string fieldId)
    {
        // Returns a dynamic object; PPDMGenericRepository will map properties to columns
        dynamic w = new System.Dynamic.ExpandoObject();
        var d = (IDictionary<string, object?>)w;
        d["WITSML_UID"]      = el.Attribute("uid")?.Value ?? string.Empty;
        d["WELL_NAME"]       = el.Element("name")?.Value ?? string.Empty;
        d["ASSIGNED_FIELD"]  = fieldId;
        d["SURFACE_COUNTRY"] = el.Element("country")?.Value ?? string.Empty;
        d["SURFACE_STATE"]   = el.Element("state")?.Value ?? string.Empty;
        if (DateTime.TryParse(el.Element("spudDate")?.Value, out var spud))
            d["SPUD_DATE"] = spud;
        return w;
    }

    private static object MapCasingElement(XElement el, string wellUid, string wellboreUid)
    {
        dynamic c = new System.Dynamic.ExpandoObject();
        var d = (IDictionary<string, object?>)c;
        d["CASING_ID"]        = el.Attribute("uid")?.Value ?? string.Empty;
        d["WELL_ID"]          = wellUid;
        d["WELLBORE_ID"]      = wellboreUid;
        d["CASING_DESCR"]     = el.Element("description")?.Value ?? string.Empty;
        if (double.TryParse(el.Element("mdBottom")?.Element("value")?.Value, out var md))
            d["BASE_DEPTH"] = md;
        if (double.TryParse(el.Element("odSection")?.Element("value")?.Value, out var od))
            d["OUTSIDE_DIAMETER"] = od;
        d["MATERIAL_GRADE"]   = el.Element("grade")?.Value ?? string.Empty;
        return c;
    }

    private static object MapLogElement(XElement el, string wellUid, string wellboreUid)
    {
        dynamic l = new System.Dynamic.ExpandoObject();
        var d = (IDictionary<string, object?>)l;
        d["LOG_ID"]       = el.Attribute("uid")?.Value ?? string.Empty;
        d["LOG_NAME"]     = el.Element("name")?.Value ?? string.Empty;
        d["WELL_ID"]      = wellUid;
        d["WELLBORE_ID"]  = wellboreUid;
        if (double.TryParse(el.Element("startIndex")?.Value, out var top))
            d["TOP_DEPTH"] = top;
        if (double.TryParse(el.Element("endIndex")?.Value, out var bot))
            d["BASE_DEPTH"] = bot;
        return l;
    }

    private async Task<PPDMGenericRepository> MakeRepoAsync(string table)
    {
        var meta       = await _metadata.GetTableMetadataAsync(table);
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}") ?? typeof(object);
        return new PPDMGenericRepository(
            _editor, _cch, _defaults, _metadata, entityType, _connectionName, table);
    }

    private static void SetProperty(object obj, string name, object? val)
        => obj.GetType().GetProperty(name,
               System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
               System.Reflection.BindingFlags.IgnoreCase)
           ?.SetValue(obj, val);

    private static object? GetProp(object obj, string name)
        => obj.GetType().GetProperty(name,
               System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
               System.Reflection.BindingFlags.IgnoreCase)
           ?.GetValue(obj);

    private static string GetStrProp(object obj, string name)
        => GetProp(obj, name)?.ToString() ?? string.Empty;
}

/// <summary>
/// An integration's connection is paused (its circuit breaker is open): the request is refused as the integration
/// stands, in this server's words (a refusal, OILGAS-CATCH-01), not reported as a failure — the failures that opened the
/// circuit were reported when they happened.
/// </summary>
public sealed class IntegrationUnavailableException : RefusalException
{
    public IntegrationUnavailableException(string message) : base(RefusalKind.Conflict, message) { }
}

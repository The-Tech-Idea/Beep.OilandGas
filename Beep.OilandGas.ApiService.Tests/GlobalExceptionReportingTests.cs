using System.Net;
using System.Text.Json;
using Beep.OilandGas.ApiService.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// DIAG-01. The API's app-wide handler reports every exception it takes into the failure store, and a 500 carries the
/// reference — never the exception's own text, in any environment.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> the reporter is a recording double, so what is proved is what the handler reports and answers; that the
/// store keeps it is <see cref="OilGasDiagnosticsTests"/>'. The 400 and 409 answers still carry the refusal's message — that
/// contract with the Web is OILGAS-CATCH-01's to change, and is pinned here only as reported.
/// </remarks>
public class GlobalExceptionReportingTests
{
    private const string Secret = "connection string Password=hunter2";

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task An_unhandled_failure_is_reported_and_answered_with_its_reference_not_its_text(string environment)
    {
        var failures = new RecordingReporter();
        await using var app = await StartAsync(environment, failures);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        using var response = await http.GetAsync("/boom");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(Secret, body, StringComparison.Ordinal);

        var reported = Assert.Single(failures.Reports);
        Assert.Equal(FailureSeverity.Error, reported.Severity);
        Assert.Equal("handling GET /boom", reported.Operation);
        Assert.Contains(Secret, reported.Exception.Message, StringComparison.Ordinal);

        using var json = JsonDocument.Parse(body);
        Assert.Equal(RecordingReporter.Reference, json.RootElement.GetProperty("reference").GetString());
        Assert.Contains(RecordingReporter.Reference, json.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/argument", HttpStatusCode.BadRequest)]
    [InlineData("/conflict", HttpStatusCode.Conflict)]
    [InlineData("/denied", HttpStatusCode.Forbidden)]
    public async Task A_refusal_by_exception_is_reported_too(string path, HttpStatusCode expected)
    {
        var failures = new RecordingReporter();
        await using var app = await StartAsync("Production", failures);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        using var response = await http.GetAsync(path);

        Assert.Equal(expected, response.StatusCode);
        var reported = Assert.Single(failures.Reports);
        Assert.Equal(FailureSeverity.Degraded, reported.Severity);
        Assert.Equal($"handling GET {path}", reported.Operation);
    }

    private static async Task<WebApplication> StartAsync(string environment, RecordingReporter failures)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton<IFailureReporter>(failures);

        var app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/boom", string () => throw new InvalidCastException(Secret));
        app.MapGet("/argument", string () => throw new ArgumentException("Well name is required."));
        app.MapGet("/conflict", string () => throw new InvalidOperationException("The well is already closed."));
        app.MapGet("/denied", string () => throw new UnauthorizedAccessException(Secret));

        await app.StartAsync();
        return app;
    }

    private sealed class RecordingReporter : IFailureReporter
    {
        public const string Reference = "a1b2c3d4e5f6";

        public List<(Exception Exception, string Operation, FailureSeverity Severity)> Reports { get; } = [];

        public string ReportHandled(Exception exception, string operation, string consequence, FailureSeverity severity = FailureSeverity.Error)
        {
            lock (Reports)
            {
                Reports.Add((exception, operation, severity));
            }

            return Reference;
        }
    }
}

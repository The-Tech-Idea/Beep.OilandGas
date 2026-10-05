using System.Net;
using System.Text.Json;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// OILGAS-CATCH-01 (was DIAG-01's middleware test). What the API answers for an exception a request did not catch: the
/// application's refusal as problem details carrying its sentence, unreported; anything else reported and answered 500 with
/// its reference — never the exception's own text, in any environment.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> the reporter is a recording double, so what is proved is what is reported and answered; that the store
/// keeps it is <see cref="OilGasDiagnosticsTests"/>'. The host is built from the API's two registrations, not its whole
/// <c>Program.cs</c>; that the API calls them and <c>UseExceptionHandler</c> first is read by nothing here.
/// </remarks>
public class ExceptionAnswerTests
{
    private const string Secret = "connection string Password=hunter2";

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task An_unhandled_failure_is_reported_and_answered_with_its_reference_not_its_text(string environment)
    {
        var failures = new RecordingReporter();
        await using var app = await StartAsync(environment, failures, refusalsFirst: true);
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
    }

    /// <summary>
    /// The defect this replaces: the framework's own types were answered as refusals in the framework's words. An
    /// <see cref="ArgumentException"/> from a collection or an <see cref="InvalidOperationException"/> from EF is a fault.
    /// </summary>
    [Theory]
    [InlineData("/framework-argument")]
    [InlineData("/framework-state")]
    public async Task A_framework_exception_is_a_failure_not_a_refusal(string path)
    {
        var failures = new RecordingReporter();
        await using var app = await StartAsync("Production", failures, refusalsFirst: true);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        using var response = await http.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(Secret, body, StringComparison.Ordinal);
        Assert.Single(failures.Reports);
    }

    [Theory]
    [InlineData("/invalid", HttpStatusCode.BadRequest, "Well name is required.", true)]
    [InlineData("/missing", HttpStatusCode.NotFound, "Well W-1 was not found.", true)]
    [InlineData("/conflict", HttpStatusCode.Conflict, "The well is already closed.", true)]
    [InlineData("/forbidden", HttpStatusCode.Forbidden, "A local Administrator is required.", true)]
    [InlineData("/invalid", HttpStatusCode.BadRequest, "Well name is required.", false)]
    [InlineData("/conflict", HttpStatusCode.Conflict, "The well is already closed.", false)]
    public async Task A_refusal_is_answered_with_its_sentence_and_not_reported(
        string path, HttpStatusCode expected, string sentence, bool refusalsFirst)
    {
        var failures = new RecordingReporter();
        await using var app = await StartAsync("Production", failures, refusalsFirst);
        using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

        using var response = await http.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(sentence, json.RootElement.GetProperty("detail").GetString());
        Assert.Equal((int)expected, json.RootElement.GetProperty("status").GetInt32());
        Assert.Empty(failures.Reports);
    }

    private static async Task<WebApplication> StartAsync(string environment, RecordingReporter failures, bool refusalsFirst)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = environment });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton<IFailureReporter>(failures);

        // The API registers the failure service first and the refusals after it; either order must answer a refusal
        // before the failure service's handler reports it.
        if (refusalsFirst)
        {
            builder.Services.AddRefusalAnswers();
            builder.Services.AddFailureHandling();
        }
        else
        {
            builder.Services.AddFailureHandling();
            builder.Services.AddRefusalAnswers();
        }

        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/boom", string () => throw new InvalidCastException(Secret));
        app.MapGet("/framework-argument", string () => throw new ArgumentException(Secret));
        app.MapGet("/framework-state", string () => throw new InvalidOperationException(Secret));
        app.MapGet("/invalid", string () => throw RefusalException.Invalid("Well name is required."));
        app.MapGet("/missing", string () => throw RefusalException.NotFound("Well W-1 was not found."));
        app.MapGet("/conflict", string () => throw RefusalException.Conflict("The well is already closed."));
        app.MapGet("/forbidden", string () => throw RefusalException.Forbidden("A local Administrator is required."));

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

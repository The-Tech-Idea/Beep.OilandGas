using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The API takes the acting user only from the authenticated principal — the OilGas account this application issued as
/// <c>party_id</c> — never from the request and never from the identity server's <c>sub</c>.
/// </summary>
/// <remarks>
/// <para>
/// Blind spot of the source guard (<see cref="ControllersNeverTakeTheActorFromTheRequestOrTheIdentityServer"/>): it is a
/// text scan of <c>Beep.OilandGas.ApiService/Controllers</c> for known spellings of the defect — a <c>[FromQuery]</c>
/// string parameter named <c>userId</c>, a <c>?? "system"</c> fallback, a <c>FindFirst("sub")</c> /
/// <c>FindFirstValue("sub")</c> read, or a <c>ClaimTypes.NameIdentifier</c> read. It cannot see an actor taken from a
/// parameter with another name (<c>[FromQuery] string actor</c>), from a request-body field (<c>request.UserId</c>),
/// from <c>Identity.Name</c>, from a fallback to some other literal or constant, or from code outside the Controllers
/// folder (services, attributes, hubs, middleware); nor does it prove that a controller calls
/// <see cref="ActingUser.ActingUserId"/>. Comments are stripped before matching; string literals are kept, so a defect
/// spelled inside a string (a log message quoting it) is reported too.
/// </para>
/// </remarks>
public sealed class ActingUserFromPrincipalTests
{
    private const string PartyId = "party_id";

    // ── (a) Source guard ────────────────────────────────────────────────────────

    private static readonly (string Name, Regex Pattern)[] Defects =
    [
        ("a [FromQuery] userId parameter", new Regex(@"\[FromQuery[^\]]*\]\s*string\??\s+userId\b", RegexOptions.Compiled)),
        ("a ?? \"system\" actor fallback", new Regex(@"\?\?\s*""system""", RegexOptions.Compiled | RegexOptions.IgnoreCase)),
        ("an actor read from the identity server's sub", new Regex(@"FindFirst(Value)?\s*\(\s*""sub""\s*\)", RegexOptions.Compiled)),
        ("an actor read from ClaimTypes.NameIdentifier", new Regex(@"ClaimTypes\s*\.\s*NameIdentifier\b", RegexOptions.Compiled)),
    ];

    [Fact]
    public void ControllersNeverTakeTheActorFromTheRequestOrTheIdentityServer()
    {
        var controllers = Path.Combine(SolutionRoot(), "Beep.OilandGas.ApiService", "Controllers");
        var files = Directory.EnumerateFiles(controllers, "*.cs", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(files);

        var violations = files
            .SelectMany(file => Violations(File.ReadAllText(file))
                .Select(violation => $"{Path.GetRelativePath(controllers, file)}: {violation}"))
            .ToList();

        Assert.True(violations.Count == 0,
            "Controllers must take the acting user from User.ActingUserId(), never from the request or sub:" +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Theory]
    [InlineData("public IActionResult Save([FromQuery] string? userId = null) => Ok();")]
    [InlineData("public IActionResult Save([FromQuery] string userId) => Ok();")]
    [InlineData("public IActionResult Save([FromQuery(Name = \"u\")] string? userId) => Ok();")]
    [InlineData("var actor = userId ?? \"system\";")]
    [InlineData("var actor = request.UserId ?? \"SYSTEM\";")]
    [InlineData("var actor = User.FindFirst(\"sub\")?.Value;")]
    [InlineData("var actor = User.FindFirstValue(\"sub\");")]
    [InlineData("var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);")]
    public void GuardRecognisesEachSpellingOfTheDefect(string source) =>
        Assert.NotEmpty(Violations(source));

    [Theory]
    [InlineData("// was: var actor = userId ?? \"system\";\nvar actor = User.ActingUserId();")]
    [InlineData("/* [FromQuery] string? userId and FindFirst(\"sub\") are gone */ var actor = User.ActingUserId();")]
    [InlineData("var url = \"https://example.test/\"; var actor = User.ActingUserId();")]
    public void GuardIgnoresCommentsAndCleanCode(string source) =>
        Assert.Empty(Violations(source));

    private static IEnumerable<string> Violations(string source)
    {
        var code = StripComments(source);
        return Defects.Where(defect => defect.Pattern.IsMatch(code)).Select(defect => defect.Name);
    }

    /// <summary>Removes // and /* */ comments while keeping string and character literals intact.</summary>
    private static string StripComments(string source)
    {
        var output = new StringBuilder(source.Length);
        var index = 0;
        while (index < source.Length)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            if (current == '/' && next == '/')
            {
                while (index < source.Length && source[index] != '\n') index++;
                continue;
            }
            if (current == '/' && next == '*')
            {
                var end = source.IndexOf("*/", index + 2, StringComparison.Ordinal);
                index = end < 0 ? source.Length : end + 2;
                output.Append(' ');
                continue;
            }
            if (current == '"' || current == '\'')
            {
                var verbatim = current == '"' && index > 0 && source[index - 1] == '@'
                    || current == '"' && index > 1 && source[index - 1] == '$' && source[index - 2] == '@';
                output.Append(current);
                index++;
                while (index < source.Length)
                {
                    var character = source[index];
                    output.Append(character);
                    index++;
                    if (!verbatim && character == '\\' && index < source.Length)
                    {
                        output.Append(source[index]);
                        index++;
                        continue;
                    }
                    if (character != current) continue;
                    if (verbatim && index < source.Length && source[index] == '"')
                    {
                        output.Append('"');
                        index++;
                        continue;
                    }
                    break;
                }
                continue;
            }
            output.Append(current);
            index++;
        }
        return output.ToString();
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "Beep.OilandGas.sln")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        return directory!.FullName;
    }

    // ── (b) The accessor ────────────────────────────────────────────────────────

    [Fact]
    public void ActingUserIdIsThePartyIdThisApplicationIssued()
    {
        var principal = Principal(
            new Claim(PartyIdClaimsTransformation<string>.ClaimType, "account-1"),
            new Claim(ClaimTypes.NameIdentifier, "someone-else"),
            new Claim("sub", "identity-server-subject"));

        Assert.Equal("account-1", principal.ActingUserId());
        Assert.Equal("account-1", principal.FindActingUserId());
    }

    [Fact]
    public void PrincipalWithoutAnAccountIsRefusedNotAttributed()
    {
        var principal = Principal(
            new Claim(ClaimTypes.NameIdentifier, "someone-else"),
            new Claim("sub", "identity-server-subject"),
            new Claim(ClaimTypes.Name, "display-name"));

        Refusals.Forbidden(() => principal.ActingUserId());
        Refusals.Forbidden(() => ((ClaimsPrincipal?)null).ActingUserId());
        Refusals.Forbidden(() => new ClaimsPrincipal(new ClaimsIdentity()).ActingUserId());
        Assert.Null(principal.FindActingUserId());
    }

    [Fact]
    public void PartyIdFromAnotherIssuerIsNeverRead()
    {
        var principal = Principal(new Claim("party_id", "x", ClaimValueTypes.String, "https://idp.example.test/"));

        Refusals.Forbidden(() => principal.ActingUserId());
        Assert.Null(principal.FindActingUserId());
    }

    // ── (c) Controllers attribute to the principal, whatever the request names ──

    [Fact]
    public async Task InsertIsRecordedAgainstThePrincipalEvenWhenTheQueryNamesSomeoneElse()
    {
        var data = new Mock<IPPDM39DataService>(MockBehavior.Strict);
        data.Setup(x => x.InsertEntityAsync("WELL", It.IsAny<Dictionary<string, object>>(), "real-actor", "PPDM39"))
            .ReturnsAsync(new GenericEntityResponse { Success = true });
        var controller = new PPDM39DataController(data.Object, NullLogger<PPDM39DataController>.Instance,
            Mock.Of<IProgressTrackingService>(), new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter())
        {
            ControllerContext = SignedIn("real-actor", "?userId=someone-else")
        };

        using var body = JsonDocument.Parse("{\"UWI\":\"100\"}");
        var response = await controller.InsertEntity("WELL", body.RootElement);

        Assert.IsType<OkObjectResult>(response.Result);
        data.Verify(x => x.InsertEntityAsync("WELL", It.IsAny<Dictionary<string, object>>(), "real-actor", "PPDM39"), Times.Once);
        data.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AccessScopeIsReadForThePrincipalNotForAQueriedUser()
    {
        var security = new Mock<IRowLevelSecurityService>(MockBehavior.Strict);
        security.Setup(x => x.GetUserAccessibleFieldsAsync("real-actor")).ReturnsAsync(new[] { "FIELD-1" });
        var controller = new RowLevelSecurityController(security.Object, NullLogger<RowLevelSecurityController>.Instance)
        {
            ControllerContext = SignedIn("real-actor", "?userId=someone-else")
        };

        var response = await controller.GetAccessibleFields();

        Assert.Equal(new[] { "FIELD-1" }, Assert.IsType<string[]>(Assert.IsType<OkObjectResult>(response).Value));
        security.Verify(x => x.GetUserAccessibleFieldsAsync("real-actor"), Times.Once);
        security.VerifyNoOtherCalls();
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    private static ControllerContext SignedIn(string account, string queryString)
    {
        var context = new DefaultHttpContext
        {
            User = Principal(
                new Claim(PartyId, account),
                new Claim(ClaimTypes.NameIdentifier, "someone-else"),
                new Claim("sub", "someone-else"))
        };
        context.Request.QueryString = new QueryString(queryString);
        return new ControllerContext { HttpContext = context };
    }
}

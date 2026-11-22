using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Setup;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-07, restated for the database model: the gate decides from the installation's database, sign-in authenticates
/// against it, every attempt is audited, and the session the shell runs under is built from the company record the
/// database supplied.
/// </summary>
public sealed class SetupAndSignInTests
{
    private const string Strong = "Probe#2026!Desk";

    [Theory]
    // connection configured, company recorded, accounts exist, signed in -> action
    [InlineData(false, false, false, false, OgStartupAction.RunSetup)] // nothing on this machine
    [InlineData(false, true, true, true, OgStartupAction.RunSetup)]    // claims but no connection
    [InlineData(true, false, false, false, OgStartupAction.RunSetup)]  // database empty: never finished
    [InlineData(true, true, false, false, OgStartupAction.RunSetup)]   // company but no account: rollback leftover
    [InlineData(true, false, true, false, OgStartupAction.RunSetup)]   // accounts but no company record
    [InlineData(true, true, true, false, OgStartupAction.SignIn)]      // ready to sign in
    [InlineData(true, true, true, true, OgStartupAction.Ready)]        // signed in
    public void The_startup_gate_decides_from_the_database_and_the_session(
        bool connectionConfigured,
        bool companyRecorded,
        bool accountsExist,
        bool signedIn,
        OgStartupAction expected)
    {
        Assert.Equal(
            expected,
            OgStartupGate.Decide(connectionConfigured, companyRecorded, accountsExist, signedIn));
    }

    [Fact]
    public void A_database_with_a_company_but_no_account_sends_the_operator_back_through_setup()
    {
        // Setup adopts the company that is already there and creates the missing account; nothing is duplicated.
        Assert.Equal(
            OgStartupAction.RunSetup,
            OgStartupGate.Decide(
                connectionConfigured: true,
                companyRecorded: true,
                accountsExist: false,
                signedIn: false));
    }

    [Fact]
    public async Task A_refused_sign_in_writes_one_audit_row_and_creates_no_session()
    {
        var audit = new RecordingAuditService();
        var service = NewService(audit);

        var wrongPassword = await service.SignInAsync("admin", "Wrong#Password2026!", new StubIdentity(refuse: true));

        Assert.False(wrongPassword.Success);
        Assert.Equal(OgDatabaseIdentityCodes.Refused, wrongPassword.Reference);

        var row = Assert.Single(audit.Rows);
        Assert.Equal("sign-in", row.Verb);
        Assert.Equal("refused", row.Outcome);
    }

    [Fact]
    public async Task An_accepted_sign_in_writes_one_audit_row_and_returns_the_roles_it_granted()
    {
        var audit = new RecordingAuditService();
        var context = new OgRuntimeContext();
        var service = new OgSignInService(audit, context);

        var result = await service.SignInAsync(
            "admin",
            Strong,
            new StubIdentity(roles: ["Administrator"], displayName: "Administrator"));

        Assert.True(result.Success, result.Message);
        Assert.Equal("admin", result.Value!.Session.UserId);
        Assert.Equal("Administrator", result.Value.Session.UserDisplayName);
        Assert.Equal(["Administrator"], result.Value.Roles);

        var row = Assert.Single(audit.Rows);
        Assert.Equal("ok", row.Outcome);
        Assert.Contains("Administrator", row.Detail ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void The_session_follows_the_company_record_the_database_supplied()
    {
        var state = new OgSetupState(
            Completed: true,
            CompanyName: "Probe Energy",
            DataRoot: OgPaths.DataRoot,
            AdministratorUserName: "admin",
            CompletedAtUtc: DateTimeOffset.UtcNow,
            CompanyCode: "PROBE",
            CountryCode: "AE",
            CultureName: "ar-SA",
            TimeZoneId: "Asia/Dubai",
            BaseCurrencyCode: "AED",
            DefaultFieldName: "North Field",
            Deployment: OgDeploymentMode.CompanyDatabase,
            DataConnectionName: "CompanyDatabase",
            SharedSettingsLoadedAtUtc: DateTimeOffset.UtcNow);

        var session = OgSignInService.BuildSession(new OgSignedInUser("admin", "Administrator"), state);

        Assert.Equal("Probe Energy", session.CompanyName);
        Assert.Equal("North Field", session.FieldName);
        Assert.Equal("AED", session.BaseCurrencyCode);
        Assert.Equal("ar-SA", session.CultureName);
        Assert.Equal(OgDeploymentMode.CompanyDatabase, session.Mode);
        Assert.Equal("CompanyDatabase", session.ConnectionName);
    }

    [Fact]
    public void A_session_without_a_company_record_falls_back_to_the_install_default()
    {
        var session = OgSignInService.BuildSession(
            new OgSignedInUser("admin", "Administrator"),
            setup: null);

        Assert.Equal(OgPaths.DefaultCompanyName, session.CompanyName);
        Assert.Equal("admin", session.UserId);
    }

    [Fact]
    public void Adopting_a_settings_change_keeps_the_signed_in_person()
    {
        var before = OgSignInService.BuildSession(new OgSignedInUser("admin", "Administrator"), setup: null);

        var after = OgSignInService.Adopt(
            before,
            new OgSetupState(
                Completed: true,
                CompanyName: "Adopted Energy",
                DataRoot: OgPaths.DataRoot,
                AdministratorUserName: "admin",
                CompletedAtUtc: DateTimeOffset.UtcNow,
                BaseCurrencyCode: "SAR",
                DefaultFieldName: "South Field",
                Deployment: OgDeploymentMode.CompanyDatabase,
                DataConnectionName: "CompanyDatabase"));

        Assert.Equal("admin", after.UserId);
        Assert.Equal("Administrator", after.UserDisplayName);
        Assert.Equal("Adopted Energy", after.CompanyName);
        Assert.Equal("South Field", after.FieldName);
        Assert.Equal("SAR", after.BaseCurrencyCode);
    }

    private static OgSignInService NewService(RecordingAuditService audit) =>
        new(audit, new OgRuntimeContext());

    /// <summary>The account directory as a stub, so the sign-in service can be tested without a database.</summary>
    private sealed class StubIdentity : IOgDatabaseIdentity
    {
        private readonly bool _refuse;
        private readonly IReadOnlyList<string> _roles;
        private readonly string _displayName;

        public StubIdentity(bool refuse = false, IReadOnlyList<string>? roles = null, string displayName = "Administrator")
        {
            _refuse = refuse;
            _roles = roles ?? [];
            _displayName = displayName;
        }

        public Task<OgResult<IReadOnlyList<string>>> ListUserNamesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<string>>.Ok([]));

        public Task<OgResult<bool>> HasAccountsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<bool>.Ok(true));

        public Task<OgResult<OgDatabaseUser>> SignInAsync(
            string userName,
            string password,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_refuse
                ? OgResult<OgDatabaseUser>.Failed(
                    "The user name or the password was not accepted.",
                    OgDatabaseIdentityCodes.Refused)
                : OgResult<OgDatabaseUser>.Ok(new OgDatabaseUser(
                    "USER-1", userName, _displayName, "TENANT", "BA", _roles)));

        public Task<OgResult<OgDatabaseUser>> CreateSystemAdministratorAsync(
            string userName,
            string displayName,
            string password,
            string actor,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A sign-in test does not create accounts.");

        public Task<OgResult> DeleteAccountAsync(string userId, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A sign-in test does not delete accounts.");

        public Task<OgResult> ChangePasswordAsync(
            string userName,
            string newPassword,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A sign-in test does not change passwords.");

        public Task<OgResult> MarkPasswordChangeRequiredAsync(
            string userName,
            string actor,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("A sign-in test does not mark accounts.");
    }

    private sealed class RecordingAuditService : IOgAuditService
    {
        public List<OgAuditRow> Rows { get; } = [];

        public void Add(
            string verb,
            string subjectKind,
            string subjectId,
            string outcome,
            string? detail = null,
            string? fieldId = null) =>
            Rows.Add(new OgAuditRow(verb, subjectKind, subjectId, outcome, detail, fieldId));
    }

    private sealed record OgAuditRow(
        string Verb,
        string SubjectKind,
        string SubjectId,
        string Outcome,
        string? Detail,
        string? FieldId);
}

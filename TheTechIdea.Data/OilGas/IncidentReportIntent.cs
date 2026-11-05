using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace TheTechIdea.Data.OilGas;

/// <summary>The exact company/FIELD report to retain and reconcile before any retry.</summary>
public sealed record IncidentReportIntent(Guid IntentId, FieldSelectionReview Context, IncidentReportOption Option,
    string OptionsRevision, DateTime IncidentUtc, string Location, string Description, string Jurisdiction)
{
    public void Require()
    {
        if (Context is null || Option is null) throw new InvalidOperationException("The original FIELD and incident option are required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (IntentId == Guid.Empty || !LeaseSummaryValidator.Identifier(Context.FieldId) ||
            !LeaseSummaryValidator.Identifier(Option.IncidentSetId) || !LeaseSummaryValidator.Identifier(Option.IncidentTypeId) ||
            Option.SeverityId is not ("TIER_1" or "TIER_2" or "TIER_3" or "TIER_4") ||
            OptionsRevision is not { Length: 64 } || OptionsRevision.Any(c => !char.IsAsciiHexDigit(c) || char.IsAsciiLetterLower(c)) ||
            IncidentUtc == default || IncidentUtc.Kind != DateTimeKind.Utc || !Text(Location, 255) || !Text(Description, 2000, multiline: true) || !Text(Jurisdiction, 128))
            throw new InvalidOperationException("The incident report requires its original identity, reviewed values, UTC time and bounded text.");
    }

    private static bool Text(string value, int maximum, bool multiline = false) => !string.IsNullOrWhiteSpace(value) && value == value.Trim() &&
        value.Length <= maximum && !value.Any(c => char.IsControl(c) && !(multiline && c is '\r' or '\n'));
    public string IncidentId() { Require(); return "HSE-" + IntentId.ToString("N").ToUpperInvariant(); }
}

/// <summary>Confirmed incident records for the exact retained report; workflow creation is separate.</summary>
public sealed record IncidentReportReceipt(CompanyScope Company, string ActorUserId, string FieldId, Guid IntentId, string IncidentId)
{
    public void Require(IncidentReportIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        if (Company != original.Context.Company || ActorUserId != original.Context.ActorUserId || FieldId != original.Context.FieldId ||
            IntentId != original.IntentId || IncidentId != original.IncidentId())
            throw new InvalidOperationException("The recorded incident does not match the original report.");
    }
}

/// <summary>Private original report evidence, written with its incident records by the reviewed command.</summary>
public sealed class HSE_INCIDENT_REPORT_INTENT : ModelEntityBase
{
    [Key, Required, MaxLength(32)] public string HSE_INCIDENT_REPORT_INTENT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(36)] public string INCIDENT_ID { get; set; } = "";
    [Required, MaxLength(16384)] public string COMMAND_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string COMMAND_HASH { get; set; } = "";

    public static HSE_INCIDENT_REPORT_INTENT Create(IncidentReportIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent); intent.Require();
        var json = JsonSerializer.Serialize(intent);
        if (json.Length > 16384) throw new InvalidOperationException("The incident report is too large to retain.");
        return new() { HSE_INCIDENT_REPORT_INTENT_ID = intent.IntentId.ToString("N"), OWNER_TENANT_ID = intent.Context.Company.TenantId,
            OWNER_COMPANY_BA_ID = intent.Context.Company.BusinessAssociateId, FIELD_ID = intent.Context.FieldId!, INCIDENT_ID = intent.IncidentId(),
            COMMAND_JSON = json, COMMAND_HASH = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json))), ACTIVE_IND = "Y" };
    }

    public IncidentReportIntent Restore(CompanyScope company, string actor)
    {
        if (company is null || !LeaseSummaryValidator.Identifier(actor) || OWNER_TENANT_ID != company.TenantId || OWNER_COMPANY_BA_ID != company.BusinessAssociateId ||
            ACTIVE_IND != "Y" || ROW_CREATED_BY != actor || ROW_CHANGED_BY != actor ||
            ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created ||
            string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length > 16384 ||
            COMMAND_HASH != Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(COMMAND_JSON))))
            throw new InvalidOperationException("The retained incident report ownership or payload is unconfirmed.");
        var intent = JsonSerializer.Deserialize<IncidentReportIntent>(COMMAND_JSON) ?? throw new InvalidOperationException("The retained incident report is unavailable.");
        intent.Require(); var expected = Create(intent);
        if (intent.Context.Company != company || intent.Context.ActorUserId != actor || HSE_INCIDENT_REPORT_INTENT_ID != expected.HSE_INCIDENT_REPORT_INTENT_ID ||
            FIELD_ID != expected.FIELD_ID || INCIDENT_ID != expected.INCIDENT_ID)
            throw new InvalidOperationException("The retained incident report differs from its original identity or FIELD.");
        return intent;
    }
}

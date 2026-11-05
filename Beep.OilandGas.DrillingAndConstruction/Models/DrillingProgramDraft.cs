using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

public sealed record DrillingProgramWellFacts([property:JsonRequired]string Uwi,[property:JsonRequired]string FieldId,
    [property:JsonRequired]string? Name,[property:JsonRequired]string Revision)
{
    public static DrillingProgramWellFacts FromRecord(WELL well,bool activeRequired=true)
    {
        if(well is null || string.IsNullOrWhiteSpace(well.UWI) || well.UWI.Length>128 || well.UWI.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(well.ASSIGNED_FIELD) || well.ACTIVE_IND is not ("Y" or "N") || activeRequired && well.ACTIVE_IND!="Y")throw new InvalidOperationException("One exact active recorded well and its FIELD are required.");
        return new(well.UWI,well.ASSIGNED_FIELD,well.WELL_NAME,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(well)));
    }
}

public sealed record DrillingProgramBasis([property:JsonRequired]DevelopmentInvestmentHandoffStatus Funding,
    [property:JsonRequired]DrillingProgramWellFacts Well,[property:JsonRequired]AssetDatabaseScope ExecutionDatabase,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public DevelopmentAssignedReviewScope Scope()=>Funding.Source.Current.Source.Review.Scope;
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Funding=Funding.Revision,Well,ExecutionDatabase}));
    public void Require()
    {
        if(Funding?.Source?.Current?.Source?.Review?.Scope is not {} scope || Well is null || ExecutionDatabase is null)throw new InvalidOperationException("The recorded well and approved original investment source are required.");
        Funding.Require(scope.Company,scope.ActorUserId,Funding.Source.Current.Source.Review.InstanceId,Funding.Source.AfeId);
        if(Funding.Progress?.State!="APPROVED" || Funding.Source.Recorded?.Decision!="APPROVE" || Well.FieldId!=scope.FieldId ||
            string.IsNullOrWhiteSpace(Well.Uwi) || Well.Uwi.Length>128 || Well.Uwi.Any(char.IsControl) || !ProcessDefinitionRevision.IsValid(Well.Revision) ||
            string.IsNullOrWhiteSpace(ExecutionDatabase.ConnectionName) || string.IsNullOrWhiteSpace(ExecutionDatabase.Fingerprint) || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("A drilling program requires the exact active well/FIELD and fully reconciled final investment approval, not a pending or financial-only decision.");
    }
}

public sealed record CreateDrillingProgramDraft([property:JsonRequired]DrillingProgramBasis Basis,[property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Name,[property:JsonRequired]string Objective,[property:JsonRequired]string WellType,
    [property:JsonRequired]string Profile,[property:JsonRequired]decimal MeasuredDepth,[property:JsonRequired]decimal VerticalDepth,
    [property:JsonRequired]string DepthUnit,[property:JsonRequired]int DurationDays,[property:JsonRequired]DateOnly PlannedSpudDate,
    [property:JsonRequired]DateOnly PlannedCompletionDate)
{
    public const int MaximumBodyLength=4*1024*1024;
    public void Require()
    {
        if(Basis is null)throw new InvalidOperationException("Inspect the current approved well/program basis before saving.");Basis.Require();
        if(IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Name) || Name!=Name.Trim() || Name.Length>256 || Name.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(Objective) || Objective!=Objective.Trim() || Objective.Length>4096 || Objective.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) ||
            WellType is not ("WILDCAT" or "APPRAISAL" or "DEVELOPMENT" or "SIDETRACK" or "INFILL") || Profile is not ("VERTICAL" or "J_SHAPE" or "S_SHAPE" or "HORIZONTAL") ||
            MeasuredDepth<=0 || VerticalDepth<=0 || VerticalDepth>MeasuredDepth || DepthUnit is not ("m" or "ft") || DurationDays<=0 ||
            PlannedSpudDate.Year<1900 || PlannedCompletionDate<PlannedSpudDate || PlannedSpudDate<Basis.Funding.Source.Afe().EffectiveDate || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("Enter the explicit program name/objective, well type/profile, positive MD/TVD and m/ft unit, duration and valid planned calendar dates at or after the AFE effective date.");
    }
    public string ProgramId(){Require();return "drilling-program-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Basis.Scope().Company,IntentId}));}
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public DRILLING_PROGRAM ToRecord(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc==default || utc<Basis.CheckedAtUtc.UtcDateTime)throw new InvalidOperationException("Actual current program producer UTC is required.");var scope=Basis.Scope();var afe=Basis.Funding.Source.Afe();
        var row=new DRILLING_PROGRAM{PROGRAM_ID=ProgramId(),OWNER_TENANT_ID=scope.Company.TenantId,OWNER_COMPANY_BA_ID=scope.Company.BusinessAssociateId,
            WELL_ID=Basis.Well.Uwi,FIELD_ID=scope.FieldId,PROGRAM_NAME=Name,DESCRIPTION_OBJECTIVE=Objective,PROGRAM_STATUS="DRAFT",PROGRAM_VERSION=1,
            WELL_TYPE=WellType,WELL_PROFILE=Profile,PLANNED_MEASURED_DEPTH=MeasuredDepth,PLANNED_TRUE_VERTICAL_DEPTH=VerticalDepth,DEPTH_OUOM=DepthUnit,PLANNED_DURATION_DAYS=DurationDays,
            PLANNED_SPUD_DATE=PlannedSpudDate.ToDateTime(TimeOnly.MinValue),PLANNED_COMPLETION_DATE=PlannedCompletionDate.ToDateTime(TimeOnly.MinValue),
            AFE_ID=afe.AfeId,AFE_NUMBER=afe.Number,AFE_COST=afe.RequestedAmount,AFE_CURRENCY=afe.CurrencyCode,FDP_BASELINE_ID=afe.BaselineId,
            INVESTMENT_HANDOFF_ID=Basis.Funding.HandoffId!,INVESTMENT_OUTCOME_REVISION=Basis.Funding.Source.Recorded!.RecordedRevision,
            SOURCE="COMPANY_DRILLING_PROGRAM",ACTIVE_IND="Y",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=scope.ActorUserId,ROW_CHANGED_BY=scope.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,
            PROGRAM_DRAFT_JSON=JsonSerializer.Serialize(this)};
        row.PROGRAM_DRAFT_REVISION=Digest(row);return row;
    }
    public static string Digest(DRILLING_PROGRAM row){var copy=CompanyInventorySourceReview.Copy(row);copy.PROGRAM_DRAFT_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CreateDrillingProgramDraft FromRecord(DRILLING_PROGRAM row)
    {
        if(row?.PROGRAM_DRAFT_JSON is not {Length:>0 and <=MaximumBodyLength} json)throw new InvalidOperationException("The bounded original drilling program proof is unavailable.");
        var original=JsonSerializer.Deserialize<CreateDrillingProgramDraft>(json)??throw new InvalidOperationException("The original drilling program command is invalid.");original.Require();var actual=CompanyInventorySourceReview.Copy(row);
        if(actual.ROW_CREATED_DATE is null || actual.ROW_CHANGED_DATE is null || actual.ROW_CREATED_DATE.Value.Kind==DateTimeKind.Local || actual.ROW_CHANGED_DATE.Value.Kind==DateTimeKind.Local ||
            actual.PLANNED_SPUD_DATE is null || actual.PLANNED_COMPLETION_DATE is null || actual.PLANNED_SPUD_DATE.Value.Kind==DateTimeKind.Local || actual.PLANNED_COMPLETION_DATE.Value.Kind==DateTimeKind.Local || actual.PLANNED_SPUD_DATE.Value.TimeOfDay!=TimeSpan.Zero || actual.PLANNED_COMPLETION_DATE.Value.TimeOfDay!=TimeSpan.Zero)
            throw new InvalidOperationException("The original program UTC audit and exact planned calendar dates are required.");
        actual.ROW_CREATED_DATE=DateTime.SpecifyKind(actual.ROW_CREATED_DATE.Value,DateTimeKind.Utc);actual.ROW_CHANGED_DATE=DateTime.SpecifyKind(actual.ROW_CHANGED_DATE.Value,DateTimeKind.Utc);
        actual.PLANNED_SPUD_DATE=DateTime.SpecifyKind(actual.PLANNED_SPUD_DATE.Value,DateTimeKind.Unspecified);actual.PLANNED_COMPLETION_DATE=DateTime.SpecifyKind(actual.PLANNED_COMPLETION_DATE.Value,DateTimeKind.Unspecified);
        if(actual.ROW_CHANGED_DATE<actual.ROW_CREATED_DATE)throw new InvalidOperationException("The native program change audit precedes its creation.");
        var expected=original.ToRecord(actual.ROW_CREATED_DATE.Value);expected.ROW_CHANGED_DATE=actual.ROW_CHANGED_DATE;expected.PROGRAM_DRAFT_REVISION=Digest(expected);actual.PROGRAM_DRAFT_REVISION=Digest(actual);
        if(row.PROGRAM_DRAFT_REVISION!=expected.PROGRAM_DRAFT_REVISION || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The native program differs from its exact original company/well/funding/inputs/audit proof.");return original;
    }
}

public sealed record DrillingProgramDraftReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string ProgramId,
    [property:JsonRequired]DRILLING_PROGRAM? Recorded,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(CreateDrillingProgramDraft original)
    {
        original.Require();if(IntentRevision!=original.Revision() || ProgramId!=original.ProgramId() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero ||
            Recorded is not null && CreateDrillingProgramDraft.FromRecord(Recorded).Revision()!=original.Revision())throw new InvalidOperationException("The program receipt does not confirm the exact retained original draft.");
    }
}

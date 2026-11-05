using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace TheTechIdea.Data.OilGas;

/// <summary>Shared input/readback checks for the existing FDP draft owner; never grants creation or approval rights.</summary>
public static class DevelopmentPlanDraftReview
{
    public static CreateDevelopmentPlan Capture(CreateDevelopmentPlan request, FieldSelectionReview current)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(current);
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(current.Company.TenantId) || !LeaseSummaryValidator.Identifier(current.Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(current.ActorUserId)) throw new InvalidOperationException("An explicit company and original actor are required for the draft.");
        var frozen=request;
        if(frozen.PlanningDatabase is null || !LeaseSummaryValidator.Identifier(frozen.PlanningDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(frozen.PlanningDatabase.Fingerprint))
            throw new InvalidOperationException("Review the original development-planning database binding before saving.");
        if(current.FieldId is null || frozen.SelectionContext!=current || frozen.FieldId!=current.FieldId || frozen.IntentId==Guid.Empty)
            throw new InvalidOperationException("Review the current company/FIELD and retain the original draft intent before saving.");
        RequireValues(frozen.PlanName,frozen.Description,frozen.PlannedFirstProductionDate,frozen.PlannedFieldLifeEndDate,frozen.EstimatedCapexMillions,frozen.CapexCurrency);
        return frozen;
    }

    private static void RequireValues(string name,string? description,DateOnly? firstProduction,DateOnly? lifeEnd,decimal? capex,string? currency)
    {
        if(string.IsNullOrWhiteSpace(name) || name.Length>200 || name!=name.Trim() || name.Any(char.IsControl) ||
            description?.Length>4000 || description?.Any(character=>char.IsControl(character) && character is not ('\r' or '\n' or '\t'))==true)
            throw new InvalidOperationException("Enter a plan name and a bounded development description.");
        if(firstProduction is { Year:<1900 } || lifeEnd is { Year:<1900 })
            throw new InvalidOperationException("Planned dates must be explicit company calendar dates from 1900 onward.");
        if(firstProduction is {} start && lifeEnd is {} end && end<start)
            throw new InvalidOperationException("The planned field life end cannot precede planned first production.");
        if(capex is < 0 || capex is not null &&
            (currency is null || currency.Length!=3 || currency.Any(character=>character is < 'A' or > 'Z')) ||
            capex is null && currency is not null)
            throw new InvalidOperationException("Enter a nonnegative estimate in millions and its explicit currency, or leave both unrecorded.");
    }

    public static void RequirePopulation(IReadOnlyList<FIELD_DEVELOPMENT_PLAN> rows,FieldSelectionReview current,string? originalId=null)
    {
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        RequirePopulation(rows,current.Company,current.FieldId!,originalId);
    }
    public static void RequirePopulation(IReadOnlyList<FIELD_DEVELOPMENT_PLAN> rows,CompanyScope company,string fieldId,string? originalId=null)
    {
        APInvoiceOwnership.RequireScope(company);
        if(!LeaseSummaryValidator.Identifier(fieldId))throw new InvalidOperationException("An exact recorded draft FIELD is required.");
        if(rows is null || rows.Count>200 || rows.Any(row=>row is null) || rows.Select(row=>row.FDP_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("A complete bounded draft population is required.");
        foreach(var row in rows)
        {
            if(row.TENANT_ID!=company.TenantId || row.BUSINESS_ASSOCIATE_ID!=company.BusinessAssociateId || row.FIELD_ID!=fieldId || row.ACTIVE_IND!="Y" ||
                originalId is not null && row.FDP_ID!=originalId || !LeaseSummaryValidator.Identifier(row.ROW_CREATED_BY) || row.ROW_CREATED_DATE is null || row.ROW_CREATED_DATE.Value.Kind==DateTimeKind.Local || row.ROW_CHANGED_DATE is not null || !string.IsNullOrEmpty(row.ROW_CHANGED_BY) ||
                !Guid.TryParseExact(row.DRAFT_INTENT_ID,"D",out var intent) || intent==Guid.Empty || !ProcessDefinitionRevision.IsValid(row.DRAFT_REQUEST_REVISION) ||
                row.SOURCE_DRAFT_FDP_ID is not null || row.SOURCE_REVIEW_PROCESS_ID is not null || row.RELEASE_INTENT_ID is not null || row.RELEASE_REQUEST_REVISION is not null || row.RELEASE_COMMAND_JSON is not null ||
                row.FDP_ID!=PlanId(company,intent) || row.FDP_STATUS!="DRAFT" || row.FDP_VERSION!=1 || row.SUBMISSION_DATE is not null || row.APPROVAL_DATE is not null || !string.IsNullOrEmpty(row.APPROVED_BY))
                throw new InvalidOperationException("The recorded company/FIELD draft identity, ownership or initial state could not be confirmed.");
            DateOnly? Calendar(DateTime? value)
            {
                if(value is null)return null;
                if(value.Value.TimeOfDay!=TimeSpan.Zero)throw new InvalidOperationException("Planned development dates must retain their original calendar date.");
                return DateOnly.FromDateTime(value.Value);
            }
            RequireValues(row.FDP_NAME,row.DEVELOPMENT_CONCEPT_DESC,Calendar(row.FIRST_PRODUCTION_DATE),Calendar(row.FIELD_LIFE_END),row.TOTAL_CAPEX_MM,
                row.TOTAL_CAPEX_MM is null && string.IsNullOrEmpty(row.CAPEX_CURRENCY)?null:row.CAPEX_CURRENCY);
        }
    }

    private static string PlanId(CompanyScope company,Guid intentId)=>"fdp-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId=intentId}));

    public static string RequestRevision(CreateDevelopmentPlan request)
    {
        var frozen=Capture(request,request.SelectionContext ?? throw new InvalidOperationException("The draft FIELD review is missing."));
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
            frozen.SelectionContext,frozen.PlanningDatabase,frozen.IntentId,frozen.PlanName,frozen.FieldId,frozen.Description,
            frozen.PlannedFirstProductionDate,frozen.PlannedFieldLifeEndDate,frozen.EstimatedCapexMillions,frozen.CapexCurrency
        }));
    }

    public static string PlanIdFor(CreateDevelopmentPlan request)
    {
        var frozen=Capture(request,request.SelectionContext ?? throw new InvalidOperationException("The draft FIELD review is missing."));
        return PlanId(frozen.SelectionContext.Company,frozen.IntentId);
    }

    public static void RequireRecorded(CreateDevelopmentPlan original, FIELD_DEVELOPMENT_PLAN row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var frozen=Capture(original,original.SelectionContext ?? throw new InvalidOperationException("The original draft review is missing."));
        var context=frozen.SelectionContext!;
        RequirePopulation([row],context,PlanIdFor(frozen));
        if(row.FDP_ID!=PlanIdFor(frozen) || row.TENANT_ID!=context.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=context.Company.BusinessAssociateId ||
            row.FIELD_ID!=context.FieldId || row.DRAFT_INTENT_ID!=frozen.IntentId.ToString("D") || row.DRAFT_REQUEST_REVISION!=RequestRevision(frozen) ||
            row.ROW_CREATED_BY!=context.ActorUserId || row.ROW_CREATED_DATE is null || row.ACTIVE_IND!="Y" ||
            row.FDP_STATUS!="DRAFT" || row.FDP_VERSION!=1 || row.APPROVAL_DATE is not null || !string.IsNullOrEmpty(row.APPROVED_BY) || row.SUBMISSION_DATE is not null ||
            row.FDP_NAME!=frozen.PlanName || row.DEVELOPMENT_CONCEPT_DESC!=(frozen.Description ?? "") ||
            row.FIRST_PRODUCTION_DATE!=frozen.PlannedFirstProductionDate?.ToDateTime(TimeOnly.MinValue) || row.FIELD_LIFE_END!=frozen.PlannedFieldLifeEndDate?.ToDateTime(TimeOnly.MinValue) ||
            row.TOTAL_CAPEX_MM!=frozen.EstimatedCapexMillions || row.CAPEX_CURRENCY!=(frozen.CapexCurrency ?? ""))
            throw new InvalidOperationException("The recorded original company draft does not match its maker, intent and reviewed inputs.");
    }
}

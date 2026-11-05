using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Complete native preparation captured under the existing company/FIELD owner.</summary>
public sealed record ProductionInterventionNativeReview(FieldSelectionReview Context, WELL Well, WELL_ACTIVITY[] History, string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public ProductionInterventionWorkOrderReview? WorkOrder { get; init; }
    public static ProductionInterventionNativeReview Create(FieldSelectionReview context, WELL well, IEnumerable<WELL_ACTIVITY> history, ProductionInterventionWorkOrderReview? workOrder=null)
    {
        var value = new ProductionInterventionNativeReview(context, Copy(well), history.OrderBy(row => row.ACTIVITY_OBS_NO).Select(Copy).ToArray(), string.Empty){WorkOrder=workOrder is null?null:Copy(workOrder)};
        value = value with { Revision = Digest(value) };
        value.Require();
        return value;
    }

    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Context);
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        PurchaseOrderOwnership.RequireScope(Context.Company);
        ArgumentNullException.ThrowIfNull(Well);
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(Well.UWI) ||
            Well.ASSIGNED_FIELD != Context.FieldId || Well.OPERATOR != Context.Company.BusinessAssociateId || Well.ACTIVE_IND != "Y" ||
            History is not { Length: > 0 and <= 512 } || History.Any(row => row is null || row.ACTIVE_IND != "Y") || Revision != Digest(this))
            throw new InvalidOperationException("The complete original company/FIELD/WELL/activity review is required.");
        _ = ProductionInterventionActivityChecks.Revision(History, Context.Company, Context.ActorUserId, Context.FieldId!, Well.UWI);
        foreach (var row in History)
            if (!LeaseSummaryValidator.Identifier(row.SOURCE) || row.ACTIVITY_OBS_NO < 0 || decimal.Truncate(row.ACTIVITY_OBS_NO) != row.ACTIVITY_OBS_NO)
                throw new InvalidOperationException("The original activity history has incomplete native keys.");
        if(WorkOrder is not null)WorkOrder.Require(WorkOrder.Header.WORK_ORDER_ID,Context.FieldId!,Well.UWI);
    }

    public void RequireCurrent(ProductionInterventionNativeReview current)
    {
        Require(); current.Require();
        if (JsonSerializer.Serialize(this) != JsonSerializer.Serialize(current))
            throw new InvalidOperationException("The complete original native preparation changed.");
    }

    internal static T Copy<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    internal static string Hash(object value) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value));
    private static string Digest(ProductionInterventionNativeReview value) => Hash(value with { Revision = string.Empty });
}

public sealed record ProductionInterventionNativeCommand(Guid IntentId, ProductionInterventionNativeReview Review,
    ProductionInterventionDecisionRequest Decision, decimal ObservationNumber)
{
    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Review); ArgumentNullException.ThrowIfNull(Decision); Review.Require();
        if(ProductionInterventionActivityChecks.IsDecisionActivity(Review.History.MaxBy(row=>row.ACTIVITY_OBS_NO)!.ACTIVITY_TYPE_ID))
            throw new InvalidOperationException("The original preparation already has a recorded disposition. A new source preparation or controlled correction is required.");
        if (IntentId == Guid.Empty || Decision.IntentId != IntentId || Decision.Context != Review.Context ||
            Decision.Decision != ProductionInterventionDecisionChecks.Require(Decision, Review.Context.FieldId!))
            throw new InvalidOperationException("An exact original native decision identity and FIELD review are required.");
        ProductionInterventionActivityChecks.RequireDecision(Review.History, Decision, Review.Context.Company,
            Review.Context.ActorUserId, Review.Context.FieldId!, Review.Well.UWI);
        var source = ProductionInterventionActivityChecks.OriginalSource(Review.History, Review.Well.UWI);
        ProductionInterventionDecisionChecks.RequireIndependentApprover(source.ROW_CREATED_BY, Review.Context.ActorUserId);
        var maximum = Review.History.Max(row => row.ACTIVITY_OBS_NO);
        if (maximum == decimal.MaxValue || ObservationNumber != maximum + 1)
            throw new InvalidOperationException("The stable native observation must follow the entire reviewed history.");
        if (Decision.CreateWorkOrder || Decision.LinkAfe || !string.IsNullOrWhiteSpace(Decision.AfeId))
            throw new InvalidOperationException("Order creation and financial linking require their separate reviewed command owners.");
        if(string.IsNullOrWhiteSpace(Decision.WorkOrderId))
        {if(Review.WorkOrder is not null)throw new InvalidOperationException("An unrequested order cannot be attached to this decision.");}
        else
        {
            if(Review.WorkOrder is null || Decision.Decision!="APPROVED")throw new InvalidOperationException("Only an approved decision with complete recorded order/component evidence can link maintenance.");
            Review.WorkOrder.Require(Decision.WorkOrderId,Review.Context.FieldId!,Review.Well.UWI);
        }
        if (JsonSerializer.Serialize(this).Length > WELL_INTERVENTION_INTENT.MaximumPayloadLength)
            throw new InvalidOperationException("The original native decision exceeds its declared journal capacity.");
    }

    public WELL_ACTIVITY NewActivity()
    {
        Require();
        return new()
        {
            UWI = Review.Well.UWI,
            SOURCE = ProductionInterventionActivityChecks.OriginalSource(Review.History, Review.Well.UWI).SOURCE,
            ACTIVITY_OBS_NO = ObservationNumber,
            ACTIVITY_TYPE_ID = "WORKOVER_" + Decision.Decision,
            REMARK = ProductionInterventionDecisionChecks.RecordedRemark(Decision.Note, Decision.WorkOrderId, null, null),
            ACTIVE_IND = "Y", PPDM_GUID = IntentId.ToString().ToUpperInvariant()
        };
    }

    public void RequireActivity(WELL_ACTIVITY activity)
    {
        var expected = NewActivity();
        if (activity.ROW_CREATED_BY != Review.Context.ActorUserId || activity.ROW_CHANGED_BY != Review.Context.ActorUserId ||
            activity.ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || created == default ||
            activity.ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created)
            throw new InvalidOperationException("The native decision audit does not confirm its original actor and UTC creation.");
        expected.ROW_CREATED_BY = activity.ROW_CREATED_BY; expected.ROW_CREATED_DATE = created;
        expected.ROW_CHANGED_BY = activity.ROW_CHANGED_BY; expected.ROW_CHANGED_DATE = changed;
        if (JsonSerializer.Serialize(expected) != JsonSerializer.Serialize(activity))
            throw new InvalidOperationException("The entire native decision differs from its original retained command.");
    }
}

public sealed class WELL_INTERVENTION_INTENT : ModelEntityBase
{
    public const int MaximumPayloadLength = 1048576;
    [Key, Required, MaxLength(32)] public string INTENT_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string ACTOR_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = string.Empty;
    [Required, MaxLength(128)] public string UWI { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string SOURCE_REVISION { get; set; } = string.Empty;
    [Required, MaxLength(MaximumPayloadLength)] public string COMMAND_JSON { get; set; } = string.Empty;

    public static WELL_INTERVENTION_INTENT Create(ProductionInterventionNativeCommand command)
    {
        command.Require();
        return new() { INTENT_ID = command.IntentId.ToString("N"), OWNER_TENANT_ID = command.Review.Context.Company.TenantId,
            OWNER_COMPANY_BA_ID = command.Review.Context.Company.BusinessAssociateId, ACTOR_ID = command.Review.Context.ActorUserId,
            FIELD_ID = command.Review.Context.FieldId!, UWI = command.Review.Well.UWI, SOURCE_REVISION = command.Review.Revision,
            COMMAND_JSON = JsonSerializer.Serialize(command), PPDM_GUID = command.IntentId.ToString().ToUpperInvariant(), SOURCE = "Beep.OilandGas.Production" };
    }

    public ProductionInterventionNativeCommand Restore()
    {
        if (string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length > MaximumPayloadLength)
            throw new InvalidOperationException("The original native decision journal is unavailable.");
        var command = JsonSerializer.Deserialize<ProductionInterventionNativeCommand>(COMMAND_JSON) ?? throw new InvalidOperationException("The original native command is malformed.");
        var expected = Create(command); RequireAudit(this, command.Review.Context.ActorUserId);
        CopyAudit(this, expected);
        if (JsonSerializer.Serialize(this) != JsonSerializer.Serialize(expected))
            throw new InvalidOperationException("The entire original native intent or ownership changed.");
        return command;
    }

    internal static void RequireAudit(ModelEntityBase row, string actor)
    {
        CompanyGLAccountReview.RequireAudit(row);
        if (row.ROW_CREATED_BY != actor || row.ROW_CHANGED_BY != actor || row.ROW_CHANGED_DATE is null)
            throw new InvalidOperationException("The native journal audit requires its original actor.");
    }
    internal static void CopyAudit(ModelEntityBase source, ModelEntityBase target)
    {
        target.ROW_CREATED_BY = source.ROW_CREATED_BY; target.ROW_CREATED_DATE = source.ROW_CREATED_DATE;
        target.ROW_CHANGED_BY = source.ROW_CHANGED_BY; target.ROW_CHANGED_DATE = source.ROW_CHANGED_DATE;
    }
}

public sealed class WELL_INTERVENTION_RESULT : ModelEntityBase
{
    [Key, Required, MaxLength(32)] public string INTENT_ID { get; set; } = string.Empty;
    [Required, MaxLength(WELL_INTERVENTION_INTENT.MaximumPayloadLength)] public string INTENT_JSON { get; set; } = string.Empty;
    [Required, MaxLength(WELL_INTERVENTION_INTENT.MaximumPayloadLength)] public string ACTIVITY_JSON { get; set; } = string.Empty;
    [Required, MaxLength(16384)] public string RESULT_JSON { get; set; } = string.Empty;

    public static WELL_INTERVENTION_RESULT Create(WELL_INTERVENTION_INTENT intent, WELL_ACTIVITY activity)
    {
        var command = intent.Restore(); command.RequireActivity(activity);
        if (activity.ROW_CREATED_DATE < intent.ROW_CHANGED_DATE)
            throw new InvalidOperationException("The decision activity precedes its retained native intent.");
        var result = new ProductionInterventionDecisionResult { IntentId = command.IntentId, NativeReceiptRevision = ProductionInterventionNativeReview.Hash(new { Intent = intent, Activity = activity }),
            DecisionSource = activity.SOURCE, DecisionObservationNumber = activity.ACTIVITY_OBS_NO,
            ReviewedSourceRevision = command.Decision.ExpectedSourceRevision, Success = true, FieldId = command.Decision.FieldId,
            WellId = command.Review.Well.UWI, Decision = command.Decision.Decision!, WorkOrderId=string.IsNullOrWhiteSpace(command.Decision.WorkOrderId)?null:command.Decision.WorkOrderId,
            Message = "Original native intervention decision recorded." };
        return new() { INTENT_ID = intent.INTENT_ID, INTENT_JSON = JsonSerializer.Serialize(intent), ACTIVITY_JSON = JsonSerializer.Serialize(activity),
            RESULT_JSON = JsonSerializer.Serialize(result), PPDM_GUID = command.IntentId.ToString().ToUpperInvariant(), SOURCE = "Beep.OilandGas.Production" };
    }

    public ProductionInterventionDecisionResult Restore(WELL_INTERVENTION_INTENT intent)
    {
        if (string.IsNullOrWhiteSpace(INTENT_JSON) || INTENT_JSON.Length > WELL_INTERVENTION_INTENT.MaximumPayloadLength ||
            string.IsNullOrWhiteSpace(ACTIVITY_JSON) || ACTIVITY_JSON.Length > WELL_INTERVENTION_INTENT.MaximumPayloadLength ||
            string.IsNullOrWhiteSpace(RESULT_JSON) || RESULT_JSON.Length > 16384 || INTENT_JSON != JsonSerializer.Serialize(intent))
            throw new InvalidOperationException("The original native result has incomplete or different retained evidence.");
        var activity = JsonSerializer.Deserialize<WELL_ACTIVITY>(ACTIVITY_JSON) ?? throw new InvalidOperationException("The native result activity is malformed.");
        var expected = Create(intent, activity); WELL_INTERVENTION_INTENT.RequireAudit(this, intent.ACTOR_ID);
        if (ROW_CREATED_DATE < activity.ROW_CHANGED_DATE) throw new InvalidOperationException("The result audit precedes the decision activity.");
        WELL_INTERVENTION_INTENT.CopyAudit(this, expected);
        if (JsonSerializer.Serialize(this) != JsonSerializer.Serialize(expected)) throw new InvalidOperationException("The complete native result changed.");
        return JsonSerializer.Deserialize<ProductionInterventionDecisionResult>(RESULT_JSON)!;
    }
}

public sealed record ProductionInterventionNativeRecovery(CompanyScope Company, string Actor, Guid IntentId,
    WELL_INTERVENTION_INTENT? Intent, WELL_INTERVENTION_RESULT? Outcome)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        if (!LeaseSummaryValidator.Identifier(Actor) || IntentId == Guid.Empty) throw new InvalidOperationException("Original private recovery identity is required.");
        if (Intent is null) { if (Outcome is not null) throw new InvalidOperationException("A native result requires its original journal."); return; }
        var command = Intent.Restore();
        if (command.IntentId != IntentId || command.Review.Context.Company != Company || command.Review.Context.ActorUserId != Actor)
            throw new UnauthorizedAccessException("Only the original company/account can recover this private decision.");
        if (Outcome is not null) _ = Outcome.Restore(Intent);
    }
}

/// <summary>Extension tables migrated beside WELL_ACTIVITY by the existing PPDM_CORE owner.</summary>
public static class ProductionInterventionNativeSchema
{
    public static IReadOnlyList<Type> EntityTypes { get; } = Array.AsReadOnly(new[] { typeof(WELL_INTERVENTION_INTENT), typeof(WELL_INTERVENTION_RESULT) });
}

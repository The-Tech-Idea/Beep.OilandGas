using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IIncidentReportIntentStore
{
    Task<IncidentReportIntent?> ReadAsync(FieldSelectionReview context);
    Task RetainAsync(IncidentReportIntent original);
    Task ClearAsync(IncidentReportIntent original);
}

public interface ICompanyHseWorkflowIntentStore
{
    Task<CompanyHseWorkflowStart?> ReadAsync(FieldSelectionReview context, string incidentId);
    Task RetainAsync(CompanyHseWorkflowStart original);
    Task ClearAsync(CompanyHseWorkflowStart original);
}

public interface IIncidentRcaIntentStore
{
    Task<IncidentRcaIntent?> ReadRcaAsync(FieldSelectionReview context, string incidentId);
    Task RetainAsync(IncidentRcaIntent original);
    Task ClearAsync(IncidentRcaIntent original);
}

public interface IIncidentRcaHandoffIntentStore
{
    Task<CompanyHseRcaHandoffCommand?> ReadHandoffAsync(FieldSelectionReview context, string incidentId);
    Task RetainAsync(CompanyHseRcaHandoffCommand original);
    Task ClearAsync(CompanyHseRcaHandoffCommand original);
}

/// <summary>Retains the original report in the protected browser session for reconciliation.</summary>
public sealed class IncidentReportIntentStore(ProtectedSessionStorage storage) : IIncidentReportIntentStore, ICompanyHseWorkflowIntentStore, IIncidentRcaIntentStore, IIncidentRcaHandoffIntentStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private static string Key(FieldSelectionReview context) => "oilgas.hse.report." + Convert.ToHexString(
        SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId })));
    private static string Revision(IncidentReportIntent original) => HSE_INCIDENT_REPORT_INTENT.Create(original).COMMAND_HASH;
    private static readonly SemaphoreSlim WorkflowGate = new(1, 1);
    private static string WorkflowKey(FieldSelectionReview context, string incidentId) => "oilgas.hse.workflow." + Convert.ToHexString(
        SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId, incidentId })));
    private static string WorkflowRevision(CompanyHseWorkflowStart original) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(original));
    private static readonly SemaphoreSlim RcaGate = new(1, 1);
    private static string RcaKey(FieldSelectionReview context, string incidentId) => "oilgas.hse.rca." + Convert.ToHexString(
        SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId, incidentId })));
    private static string RcaRevision(IncidentRcaIntent original) => HSE_INCIDENT_RCA_INTENT.Create(original).COMMAND_HASH;
    private static readonly SemaphoreSlim RcaHandoffGate = new(1, 1);
    private static string RcaHandoffKey(FieldSelectionReview context, string incidentId) => "oilgas.hse.rca.handoff." + Convert.ToHexString(
        SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId, incidentId })));
    private static string RcaHandoffRevision(CompanyHseRcaHandoffCommand original) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(original));

    public async Task<CompanyHseRcaHandoffCommand?> ReadHandoffAsync(FieldSelectionReview context, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(context); FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact FIELD and incident are required.");
        var saved = await storage.GetAsync<CompanyHseRcaHandoffCommand>(RcaHandoffKey(context, incidentId)); if (!saved.Success) return null;
        var original = saved.Value ?? throw new InvalidOperationException("The retained RCA handoff is unavailable."); original.Require();
        var owner = original.Review.Context;
        if (owner.Company != context.Company || owner.ActorUserId != context.ActorUserId || owner.Database != context.Database || owner.FieldId != context.FieldId || original.Review.Cause.IncidentId != incidentId)
            throw new InvalidOperationException("The retained RCA handoff belongs to another company, account, FIELD or incident.");
        return original;
    }
    public async Task RetainAsync(CompanyHseRcaHandoffCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await RcaHandoffGate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Cause.IncidentId; var previous = await ReadHandoffAsync(context, incident);
            if (previous is not null && RcaHandoffRevision(previous) != RcaHandoffRevision(original)) throw new InvalidOperationException("Recover the original RCA handoff before preparing another.");
            await storage.SetAsync(RcaHandoffKey(context, incident), original);
            if ((await ReadHandoffAsync(context, incident)) is not { } saved || RcaHandoffRevision(saved) != RcaHandoffRevision(original)) throw new InvalidOperationException("The RCA handoff was not confirmed retained.");
        }
        finally { RcaHandoffGate.Release(); }
    }
    public async Task ClearAsync(CompanyHseRcaHandoffCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await RcaHandoffGate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Cause.IncidentId; var previous = await ReadHandoffAsync(context, incident);
            if (previous is not null && RcaHandoffRevision(previous) != RcaHandoffRevision(original)) throw new InvalidOperationException("Another retained RCA handoff was preserved.");
            await storage.DeleteAsync(RcaHandoffKey(context, incident));
        }
        finally { RcaHandoffGate.Release(); }
    }

    public async Task<IncidentRcaIntent?> ReadRcaAsync(FieldSelectionReview context, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(context);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact FIELD and incident are required.");
        var saved = await storage.GetAsync<IncidentRcaIntent>(RcaKey(context, incidentId));
        if (!saved.Success) return null;
        var original = saved.Value ?? throw new InvalidOperationException("The retained cause request is unavailable.");
        original.Require(); var source = original.Source;
        if (source.Context.Company != context.Company || source.Context.ActorUserId != context.ActorUserId || source.Context.Database != context.Database ||
            source.Context.FieldId != context.FieldId || source.IncidentId != incidentId)
            throw new InvalidOperationException("The retained cause belongs to another company, account, FIELD or incident.");
        return original;
    }

    public async Task RetainAsync(IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await RcaGate.WaitAsync();
        try
        {
            var context = original.Source.Context; var id = original.Source.IncidentId;
            var previous = await ReadRcaAsync(context, id);
            if (previous is not null && RcaRevision(previous) != RcaRevision(original)) throw new InvalidOperationException("Recover the original cause before preparing another.");
            await storage.SetAsync(RcaKey(context, id), original);
            if ((await ReadRcaAsync(context, id)) is not { } saved || RcaRevision(saved) != RcaRevision(original)) throw new InvalidOperationException("The original cause was not confirmed retained.");
        }
        finally { RcaGate.Release(); }
    }

    public async Task ClearAsync(IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await RcaGate.WaitAsync();
        try
        {
            var context = original.Source.Context; var id = original.Source.IncidentId;
            var previous = await ReadRcaAsync(context, id);
            if (previous is not null && RcaRevision(previous) != RcaRevision(original)) throw new InvalidOperationException("Another retained cause was preserved.");
            await storage.DeleteAsync(RcaKey(context, id));
        }
        finally { RcaGate.Release(); }
    }

    public async Task<CompanyHseWorkflowStart?> ReadAsync(FieldSelectionReview context, string incidentId)
    {
        ArgumentNullException.ThrowIfNull(context);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId))
            throw new InvalidOperationException("An exact FIELD and incident are required.");
        var saved = await storage.GetAsync<CompanyHseWorkflowStart>(WorkflowKey(context, incidentId));
        if (!saved.Success) return null;
        var original = saved.Value ?? throw new InvalidOperationException("The retained workflow request is unavailable.");
        original.Require();
        if (original.Context.Company != context.Company || original.Context.ActorUserId != context.ActorUserId ||
            original.Context.Database != context.Database || original.Context.FieldId != context.FieldId || original.IncidentId != incidentId)
            throw new InvalidOperationException("The retained workflow request belongs to another company, account, FIELD or incident.");
        return original;
    }

    public async Task RetainAsync(CompanyHseWorkflowStart original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await WorkflowGate.WaitAsync();
        try
        {
            var previous = await ReadAsync(original.Context, original.IncidentId);
            if (previous is not null && WorkflowRevision(previous) != WorkflowRevision(original))
                throw new InvalidOperationException("Recover the original workflow request before preparing another.");
            await storage.SetAsync(WorkflowKey(original.Context, original.IncidentId), original);
            if ((await ReadAsync(original.Context, original.IncidentId)) is not { } saved || WorkflowRevision(saved) != WorkflowRevision(original))
                throw new InvalidOperationException("The original workflow request was not confirmed retained.");
        }
        finally { WorkflowGate.Release(); }
    }

    public async Task ClearAsync(CompanyHseWorkflowStart original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await WorkflowGate.WaitAsync();
        try
        {
            var previous = await ReadAsync(original.Context, original.IncidentId);
            if (previous is not null && WorkflowRevision(previous) != WorkflowRevision(original))
                throw new InvalidOperationException("Another retained workflow request was preserved.");
            await storage.DeleteAsync(WorkflowKey(original.Context, original.IncidentId));
        }
        finally { WorkflowGate.Release(); }
    }

    public async Task<IncidentReportIntent?> ReadAsync(FieldSelectionReview context)
    {
        ArgumentNullException.ThrowIfNull(context);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId)) throw new InvalidOperationException("An exact FIELD is required.");
        var saved = await storage.GetAsync<IncidentReportIntent>(Key(context));
        if (!saved.Success) return null;
        var original = saved.Value ?? throw new InvalidOperationException("The retained report is unavailable.");
        original.Require();
        if (original.Context.Company != context.Company || original.Context.ActorUserId != context.ActorUserId ||
            original.Context.Database != context.Database || original.Context.FieldId != context.FieldId)
            throw new InvalidOperationException("The retained report belongs to another company, account or FIELD.");
        return original;
    }

    public async Task RetainAsync(IncidentReportIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await gate.WaitAsync();
        try
        {
            var previous = await ReadAsync(original.Context);
            if (previous is not null && Revision(previous) != Revision(original))
                throw new InvalidOperationException("Recover the original incident report before preparing another.");
            await storage.SetAsync(Key(original.Context), original);
            if ((await ReadAsync(original.Context)) is not { } saved || Revision(saved) != Revision(original))
                throw new InvalidOperationException("The original incident report was not confirmed retained.");
        }
        finally { gate.Release(); }
    }

    public async Task ClearAsync(IncidentReportIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); await gate.WaitAsync();
        try
        {
            var previous = await ReadAsync(original.Context);
            if (previous is not null && Revision(previous) != Revision(original))
                throw new InvalidOperationException("Another retained incident report was preserved.");
            await storage.DeleteAsync(Key(original.Context));
        }
        finally { gate.Release(); }
    }
}

using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Beep.OilandGas.Web.Services;

public interface IDevelopmentDraftIntentStore
{
    Task<DevelopmentEquipmentInstallation?> ReadEquipmentInstallationAsync(CompanyScope company,string actor);
    Task RetainEquipmentInstallationAsync(DevelopmentEquipmentInstallation original);
    Task ClearEquipmentInstallationAsync(DevelopmentEquipmentInstallation original);
    Task<DevelopmentFacilityCreation?> ReadFacilityCreationAsync(CompanyScope company,string actor);
    Task RetainFacilityCreationAsync(DevelopmentFacilityCreation original);
    Task ClearFacilityCreationAsync(DevelopmentFacilityCreation original);
    Task<DevelopmentFacilityEdit?> ReadFacilityEditAsync(CompanyScope company,string actor);
    Task RetainFacilityEditAsync(DevelopmentFacilityEdit original);
    Task ClearFacilityEditAsync(DevelopmentFacilityEdit original);
    Task<DrillingReportingReassignment?> ReadReportingReassignmentAsync(CompanyScope company,string actor);
    Task RetainReportingReassignmentAsync(DrillingReportingReassignment original);
    Task ClearReportingReassignmentAsync(DrillingReportingReassignment original);
    Task<DrillingReportingClosure?> ReadReportingClosureAsync(CompanyScope company,string actor);
    Task RetainReportingClosureAsync(DrillingReportingClosure original);
    Task ClearReportingClosureAsync(DrillingReportingClosure original);
    Task<DrillingCompletionReviewDecision?> ReadCompletionDecisionAsync(CompanyScope company,string actor);
    Task RetainCompletionDecisionAsync(DrillingCompletionReviewDecision original);
    Task ClearCompletionDecisionAsync(DrillingCompletionReviewDecision original);
    Task<DrillingCompletionReviewStart?> ReadCompletionReviewAsync(CompanyScope company,string actor);
    Task RetainCompletionReviewAsync(DrillingCompletionReviewStart original);
    Task ClearCompletionReviewAsync(DrillingCompletionReviewStart original);
    Task<DrillingExecutionAssignment?> ReadExecutionAssignmentAsync(CompanyScope company,string actor);
    Task RetainExecutionAssignmentAsync(DrillingExecutionAssignment original);
    Task ClearExecutionAssignmentAsync(DrillingExecutionAssignment original);
    Task<DrillingDailyReportInput?> ReadDailyReportAsync(CompanyScope company,string actor);
    Task RetainDailyReportAsync(DrillingDailyReportInput original);
    Task ClearDailyReportAsync(DrillingDailyReportInput original);
    Task<DrillingExecutionStart?> ReadExecutionAsync(CompanyScope company,string actor);
    Task RetainExecutionAsync(DrillingExecutionStart original);
    Task ClearExecutionAsync(DrillingExecutionStart original);
    Task<DrillingProgramReviewDecision?> ReadProgramDecisionAsync(CompanyScope company,string actor);
    Task RetainProgramDecisionAsync(DrillingProgramReviewDecision original);
    Task ClearProgramDecisionAsync(DrillingProgramReviewDecision original);
    Task<DrillingProgramReviewStart?> ReadProgramReviewAsync(CompanyScope company,string actor);
    Task RetainProgramReviewAsync(DrillingProgramReviewStart original);
    Task ClearProgramReviewAsync(DrillingProgramReviewStart original);
    Task<CreateDrillingProgramDraft?> ReadProgramAsync(CompanyScope company,string actor);
    Task RetainProgramAsync(CreateDrillingProgramDraft original);
    Task ClearProgramAsync(CreateDrillingProgramDraft original);

    Task<DevelopmentInvestmentStageDecision?> ReadInvestmentStageAsync(CompanyScope company,string actor);
    Task RetainInvestmentStageAsync(DevelopmentInvestmentStageDecision original);
    Task ClearInvestmentStageAsync(DevelopmentInvestmentStageDecision original);
    Task<DevelopmentInvestmentHandoff?> ReadInvestmentHandoffAsync(CompanyScope company,string actor);
    Task RetainInvestmentHandoffAsync(DevelopmentInvestmentHandoff original);
    Task ClearInvestmentHandoffAsync(DevelopmentInvestmentHandoff original);
    Task<DevelopmentAfeCommand?> ReadAfeAsync(CompanyScope company,string actor);
    Task RetainAfeAsync(DevelopmentAfeCommand original);
    Task ClearAfeAsync(DevelopmentAfeCommand original);
    Task<CreateDevelopmentPlan?> ReadAsync(CompanyScope company,string actor);
    Task RetainAsync(CreateDevelopmentPlan original);
    Task ClearAsync(CreateDevelopmentPlan original);
    Task<DevelopmentReviewStart?> ReadReviewAsync(CompanyScope company,string actor);
    Task RetainReviewAsync(DevelopmentReviewStart original);
    Task ClearReviewAsync(DevelopmentReviewStart original);
    Task<DevelopmentReviewDecision?> ReadDecisionAsync(CompanyScope company,string actor);
    Task RetainDecisionAsync(DevelopmentReviewDecision original);
    Task ClearDecisionAsync(DevelopmentReviewDecision original);
    Task<DevelopmentPlanRelease?> ReadReleaseAsync(CompanyScope company,string actor);
    Task RetainReleaseAsync(DevelopmentPlanRelease original);
    Task ClearReleaseAsync(DevelopmentPlanRelease original);
}

/// <summary>Encrypted tab-session recovery input. This is never an authorization or business-record store.</summary>
public sealed partial class DevelopmentDraftIntentStore(ProtectedSessionStorage storage) : IDevelopmentDraftIntentStore
{
    private readonly SemaphoreSlim gate=new(1,1);
    private static string Key(CompanyScope company,string actor)=>"oilgas.fdp.intent."+
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new{company,actor})));
    public async Task<DevelopmentInvestmentHandoff?> ReadInvestmentHandoffAsync(CompanyScope company,string actor)
    {
        var value=await storage.GetAsync<DevelopmentInvestmentHandoff>(Key(company,actor)+".investment-handoff");if(!value.Success)return null;
        var original=value.Value??throw new InvalidOperationException("The retained original investment handoff is unavailable.");original.Require();
        if(original.Scope().Company!=company || original.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained investment handoff belongs to another original company or sender.");return original;
    }
    public async Task RetainInvestmentHandoffAsync(DevelopmentInvestmentHandoff original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadInvestmentHandoffAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original investment handoff before submitting another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".investment-handoff",original);
            if((await ReadInvestmentHandoffAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original investment handoff recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearInvestmentHandoffAsync(DevelopmentInvestmentHandoff original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadInvestmentHandoffAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained investment handoff changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".investment-handoff");
        }
        finally{gate.Release();}
    }
    public async Task<DevelopmentAfeCommand?> ReadAfeAsync(CompanyScope company,string actor)
    {
        var value=await storage.GetAsync<DevelopmentAfeCommand>(Key(company,actor)+".afe");if(!value.Success)return null;
        var original=value.Value??throw new InvalidOperationException("The retained original development AFE request is unavailable.");original.Require();
        if(original.Review.Source.Review.Scope.Company!=company || original.Review.Source.Review.Scope.ActorUserId!=actor)throw new InvalidOperationException("The retained AFE request belongs to another original company or account.");return original;
    }
    public async Task RetainAfeAsync(DevelopmentAfeCommand original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Source.Review.Scope;var previous=await ReadAfeAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original development AFE request before submitting another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".afe",original);
            if((await ReadAfeAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original AFE request recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearAfeAsync(DevelopmentAfeCommand original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Source.Review.Scope;var previous=await ReadAfeAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained original AFE request changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".afe");
        }
        finally{gate.Release();}
    }
    public async Task<DevelopmentPlanRelease?> ReadReleaseAsync(CompanyScope company,string actor)
    {
        var value=await storage.GetAsync<DevelopmentPlanRelease>(Key(company,actor)+".baseline");if(!value.Success)return null;
        var original=value.Value??throw new InvalidOperationException("The retained original FDP baseline approval is unavailable.");original.Require();
        if(original.Review.Scope.Company!=company || original.Review.Scope.ActorUserId!=actor)throw new InvalidOperationException("The retained baseline belongs to another original company or account.");return original;
    }
    public async Task RetainReleaseAsync(DevelopmentPlanRelease original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Scope;var previous=await ReadReleaseAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original FDP baseline approval before submitting another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".baseline",original);
            if((await ReadReleaseAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original baseline recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearReleaseAsync(DevelopmentPlanRelease original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Scope;var previous=await ReadReleaseAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained original baseline changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".baseline");
        }
        finally{gate.Release();}
    }
    public async Task<DevelopmentReviewDecision?> ReadDecisionAsync(CompanyScope company,string actor)
    {
        var value=await storage.GetAsync<DevelopmentReviewDecision>(Key(company,actor)+".decision");if(!value.Success)return null;
        var original=value.Value??throw new InvalidOperationException("The retained original development decision could not be read.");original.Require();
        if(original.Review.Scope.Company!=company || original.Review.Scope.ActorUserId!=actor)throw new InvalidOperationException("The retained decision belongs to another company or account.");
        return original;
    }
    public async Task RetainDecisionAsync(DevelopmentReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Scope;var previous=await ReadDecisionAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original decision before recording another stage decision.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".decision",original);
            if((await ReadDecisionAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original decision recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearDecisionAsync(DevelopmentReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Review.Scope;var previous=await ReadDecisionAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original retained decision changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".decision");
        }
        finally{gate.Release();}
    }
    public async Task<DevelopmentReviewStart?> ReadReviewAsync(CompanyScope company,string actor)
    {
        var value=await storage.GetAsync<DevelopmentReviewStart>(Key(company,actor)+".review");if(!value.Success)return null;
        var original=value.Value??throw new InvalidOperationException("The retained original development review could not be read.");original.Require();
        if(original.Source.Context.Company!=company || original.Source.Context.ActorUserId!=actor)throw new InvalidOperationException("The original review belongs to another company or account.");
        return original;
    }
    public async Task RetainReviewAsync(DevelopmentReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var context=original.Source.Context;var retained=await ReadReviewAsync(context.Company,context.ActorUserId);
            if(retained is not null && retained.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original development review before requesting another.");
            await storage.SetAsync(Key(context.Company,context.ActorUserId)+".review",original);
            if((await ReadReviewAsync(context.Company,context.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original review recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearReviewAsync(DevelopmentReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var context=original.Source.Context;var retained=await ReadReviewAsync(context.Company,context.ActorUserId);
            if(retained is not null && retained.Revision()!=original.Revision())throw new InvalidOperationException("The retained original review changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(context.Company,context.ActorUserId)+".review");
        }
        finally{gate.Release();}
    }
    public async Task<CreateDevelopmentPlan?> ReadAsync(CompanyScope company,string actor)
    {
        var result=await storage.GetAsync<CreateDevelopmentPlan>(Key(company,actor));
        if(!result.Success)return null;
        var original=result.Value ?? throw new InvalidOperationException("The retained development draft could not be read.");
        DevelopmentPlanDraftReview.Capture(original,original.SelectionContext);
        if(original.SelectionContext.Company!=company || original.SelectionContext.ActorUserId!=actor)
            throw new InvalidOperationException("The retained draft belongs to another account or company.");
        return original;
    }
    public async Task RetainAsync(CreateDevelopmentPlan original)
    {
        DevelopmentPlanDraftReview.Capture(original,original.SelectionContext);
        await gate.WaitAsync();
        try
        {
            var previous=await ReadAsync(original.SelectionContext.Company,original.SelectionContext.ActorUserId);
            if(previous is not null && DevelopmentPlanDraftReview.RequestRevision(previous)!=DevelopmentPlanDraftReview.RequestRevision(original))
                throw new InvalidOperationException("Recover the retained original draft before saving another draft.");
            await storage.SetAsync(Key(original.SelectionContext.Company,original.SelectionContext.ActorUserId),original);
            var recorded=await ReadAsync(original.SelectionContext.Company,original.SelectionContext.ActorUserId);
            if(recorded is null || DevelopmentPlanDraftReview.RequestRevision(recorded)!=DevelopmentPlanDraftReview.RequestRevision(original))
                throw new InvalidOperationException("The original draft recovery input was not retained. Saving is unavailable.");
        }
        finally{gate.Release();}
    }
    public async Task ClearAsync(CreateDevelopmentPlan original)
    {
        await gate.WaitAsync();
        try
        {
            var recorded=await ReadAsync(original.SelectionContext.Company,original.SelectionContext.ActorUserId);
            if(recorded is not null && DevelopmentPlanDraftReview.RequestRevision(recorded)!=DevelopmentPlanDraftReview.RequestRevision(original))
                throw new InvalidOperationException("The retained original draft changed; its recovery input was preserved.");
            await storage.DeleteAsync(Key(original.SelectionContext.Company,original.SelectionContext.ActorUserId));
        }
        finally{gate.Release();}
    }
}

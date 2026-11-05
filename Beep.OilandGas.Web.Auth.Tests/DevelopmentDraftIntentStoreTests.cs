using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentDraftIntentStoreTests
{
#if WORKFLOW_INBOX_FOCUSED
    [Fact] public async Task ProgramReviewUsesEncryptedScopedStorageAndCannotReplaceAnotherOriginalOrCommandKind()
    {
        using var f=new Beep.OilandGas.ApiService.Tests.DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare();var original=f.Original;var scope=original.Scope();var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainProgramReviewAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Source.Program.PROGRAM_NAME,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);Assert.Equal(original.Revision(),(await Store().ReadProgramReviewAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadProgramReviewAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadProgramReviewAsync(scope.Company,"other"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainProgramReviewAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearProgramReviewAsync(original with{IntentId=Guid.NewGuid()}));await Store().RetainProgramAsync(f.Program.Command);Assert.Equal(2,tab.Values.Count);await Store().ClearProgramReviewAsync(original);Assert.Single(tab.Values);Assert.NotNull(await Store().ReadProgramAsync(scope.Company,scope.ActorUserId));
        await Store().RetainProgramReviewAsync(original);tab.Values[tab.Values.Keys.Single(key=>key.EndsWith(".program-review",StringComparison.Ordinal))]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadProgramReviewAsync(scope.Company,scope.ActorUserId));
    }

    [Fact] public async Task ProgramUsesEncryptedOriginalAndCannotCrossScopeReplaceOrClearAnotherIntent()
    {
        using var f=new Beep.OilandGas.ApiService.Tests.DrillingProgramDraftNativeTests.Fixture();await f.Prepare();var original=f.Command;var scope=original.Basis.Scope();
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainProgramAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Objective,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original.Revision(),(await Store().ReadProgramAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadProgramAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadProgramAsync(scope.Company,"another"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainProgramAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearProgramAsync(original with{Objective="Changed original"}));
        await Store().ClearProgramAsync(original);Assert.Empty(tab.Values);await Store().RetainProgramAsync(original);tab.Values[Assert.Single(tab.Values).Key]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadProgramAsync(scope.Company,scope.ActorUserId));
    }
    [Fact] public async Task InvestmentStageUsesEncryptedOriginalScopedToCompanyActorAndExactIntent()
    {
        using var f=new DevelopmentInvestmentAssignedStateTests.Fixture();var original=new DevelopmentInvestmentStageDecision(f.Review,Guid.NewGuid(),"REJECT","Private original investment stage / قرار مستقل");var scope=original.Scope();
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainInvestmentStageAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Rationale,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original.Revision(),(await Store().ReadInvestmentStageAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadInvestmentStageAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadInvestmentStageAsync(scope.Company,"another"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainInvestmentStageAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearInvestmentStageAsync(original with{Rationale="Changed original"}));
        await Store().ClearInvestmentStageAsync(original);Assert.Empty(tab.Values);await Store().RetainInvestmentStageAsync(original);tab.Values[Assert.Single(tab.Values).Key]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadInvestmentStageAsync(scope.Company,scope.ActorUserId));
    }
    [Fact] public async Task InvestmentHandoffUsesActualProtectedTabStorageAndCannotReplaceAnotherOriginalOrCommandKind()
    {
        using var f=new Beep.OilandGas.ApiService.Tests.DevelopmentInvestmentHandoffNativeTests.Fixture();await f.Prepare();var original=f.Original;var scope=original.Scope();
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainInvestmentHandoffAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Workflow.ProcessName,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original.Revision(),(await Store().ReadInvestmentHandoffAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadInvestmentHandoffAsync(new("tenant","foreign"),scope.ActorUserId));Assert.Null(await Store().ReadInvestmentHandoffAsync(scope.Company,"another"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainInvestmentHandoffAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearInvestmentHandoffAsync(original with{IntentId=Guid.NewGuid()}));
        var draft=Original();await Store().RetainAsync(draft);Assert.Equal(2,tab.Values.Count);
        await Store().ClearInvestmentHandoffAsync(original);Assert.Single(tab.Values);Assert.Equal(draft,await Store().ReadAsync(draft.SelectionContext.Company,draft.SelectionContext.ActorUserId));
        await Store().RetainInvestmentHandoffAsync(original);tab.Values[tab.Values.Keys.Single(key=>key.EndsWith(".investment-handoff",StringComparison.Ordinal))]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadInvestmentHandoffAsync(scope.Company,scope.ActorUserId));
    }
#endif
    internal sealed class TabStorage : IJSRuntime
    {
        internal Dictionary<string,string> Values=[];
        internal bool Refuse;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier,object?[]? args)=>InvokeAsync<TValue>(identifier,default,args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier,CancellationToken cancellationToken,object?[]? args)
        {
            if(Refuse)throw new JSException("Tab storage unavailable");
            var key=(string)args![0]!;
            if(identifier=="sessionStorage.setItem")Values[key]=(string)args[1]!;
            else if(identifier=="sessionStorage.removeItem")Values.Remove(key);
            else if(identifier=="sessionStorage.getItem")return ValueTask.FromResult(Values.TryGetValue(key,out var value)?JsonSerializer.Deserialize<TValue>(JsonSerializer.Serialize(value))!:default!);
            else throw new InvalidOperationException(identifier);
            return ValueTask.FromResult(default(TValue)!);
        }
    }
    private static CreateDevelopmentPlan Original()=>new(){SelectionContext=new(new("tenant","company"),new("core","core-binding"),"planner","preference","field"),
        PlanningDatabase=new("planning","planning-binding"),IntentId=Guid.NewGuid(),FieldId="field",PlanName="Private concept / تطوير"};
    [Fact] public async Task ActualProtectedStorageRetainsEncryptedOriginalAcrossNewOwnerAndSeparatesAccountsAndCompanies()
    {
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();var original=Original();
        DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.PlanName,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original,await Store().ReadAsync(original.SelectionContext.Company,"planner"));
        Assert.Null(await Store().ReadAsync(new("tenant","other"),"planner"));Assert.Null(await Store().ReadAsync(original.SelectionContext.Company,"other"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearAsync(original with{PlanName="Different input"}));
        Assert.Equal(original,await Store().ReadAsync(original.SelectionContext.Company,"planner"));await Store().ClearAsync(original);Assert.Empty(tab.Values);
    }
    [Fact] public async Task UnavailableOrCorruptedStorageCannotBeTreatedAsNoPendingWork()
    {
        var tab=new TabStorage();var original=Original();var store=new DevelopmentDraftIntentStore(new ProtectedSessionStorage(tab,new EphemeralDataProtectionProvider()));
        await store.RetainAsync(original);var key=Assert.Single(tab.Values).Key;tab.Values[key]="corrupted";
        await Assert.ThrowsAnyAsync<Exception>(()=>store.ReadAsync(original.SelectionContext.Company,"planner"));
        tab.Refuse=true;await Assert.ThrowsAsync<JSException>(()=>store.RetainAsync(original));Assert.Single(tab.Values);
    }
    [Fact] public async Task ReviewRecoveryUsesActualEncryptedStorageAndCannotReplaceAnotherOriginalRequest()
    {
        var input=Original();var row=DevelopmentDraftWorkspaceTests.Recorded(input);var source=new DevelopmentPlanSourceReview(input.SelectionContext,input.PlanningDatabase,row.FDP_ID,row,
            DevelopmentPlanSourceReview.RevisionFor(input.SelectionContext,input.PlanningDatabase,row.FDP_ID,row),DateTimeOffset.UtcNow);
        var workflow=new DevelopmentReviewWorkflowChoice("review","Private technical review","1.0",new('A',64),[new("technical","Technical review",1,"TechnicalReviewer",["RESERVOIR_ENGINEER"],null)]);
        var original=new DevelopmentReviewStart(source,new("lifecycle","lifecycle-binding"),workflow,[new("technical","TechnicalReviewer",[new("reviewer","RESERVOIR_ENGINEER")],new('B',64))],Guid.NewGuid());
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainAsync(input);await Store().RetainReviewAsync(original);Assert.Equal(2,tab.Values.Count);
        Assert.All(tab.Values.Values,cipher=>{Assert.DoesNotContain(input.PlanName,cipher);Assert.DoesNotContain(original.IntentId.ToString(),cipher);});
        Assert.Equal(original.Revision(),(await Store().ReadReviewAsync(input.SelectionContext.Company,"planner"))!.Revision());
        Assert.Null(await Store().ReadReviewAsync(new("tenant","other"),"planner"));Assert.Null(await Store().ReadReviewAsync(input.SelectionContext.Company,"other"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainReviewAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearReviewAsync(original with{IntentId=Guid.NewGuid()}));
        await Store().ClearReviewAsync(original);Assert.Single(tab.Values);Assert.Equal(input,await Store().ReadAsync(input.SelectionContext.Company,"planner"));
    }
    [Fact] public async Task DecisionRecoveryRetainsActualEncryptedOriginalAcrossOwnersAndCannotReplaceOrClearDifferentIntent()
    {
        using var f=new DevelopmentAssignedReviewStateTests.Fixture();var original=new DevelopmentReviewDecision(f.Review,Guid.NewGuid(),"REJECT","Private independent rationale / مراجعة");
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainDecisionAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Notes,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original.Revision(),(await Store().ReadDecisionAsync(original.Review.Scope.Company,"reviewer"))!.Revision());
        Assert.Null(await Store().ReadDecisionAsync(new("tenant","other"),"reviewer"));Assert.Null(await Store().ReadDecisionAsync(original.Review.Scope.Company,"other"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainDecisionAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearDecisionAsync(original with{Notes="Changed rationale"}));
        Assert.Equal(original.Revision(),(await Store().ReadDecisionAsync(original.Review.Scope.Company,"reviewer"))!.Revision());await Store().ClearDecisionAsync(original);Assert.Empty(tab.Values);
    }
    [Fact] public async Task ActualEncryptedBaselineJournalRetainsOriginalAndSeparatesOtherCommandKindsAndScopes()
    {
        using var f=new DevelopmentPlanReleaseStateTests.Fixture();var original=new DevelopmentPlanRelease(f.Review,Guid.NewGuid(),"Private FDP baseline rationale / موافقة");
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        var decision=new DevelopmentReviewDecision(f.Reader.Review,Guid.NewGuid(),"APPROVE","Original stage decision");await Store().RetainDecisionAsync(decision);await Store().RetainReleaseAsync(original);Assert.Equal(2,tab.Values.Count);
        Assert.All(tab.Values.Values,cipher=>{Assert.DoesNotContain(original.Rationale,cipher);Assert.DoesNotContain(original.IntentId.ToString(),cipher);});
        Assert.Equal(original.Revision(),(await Store().ReadReleaseAsync(original.Review.Scope.Company,"reviewer"))!.Revision());
        Assert.Null(await Store().ReadReleaseAsync(new("tenant","foreign"),"reviewer"));Assert.Null(await Store().ReadReleaseAsync(original.Review.Scope.Company,"other"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainReleaseAsync(original with{IntentId=Guid.NewGuid()}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearReleaseAsync(original with{Rationale="Changed original"}));
        await Store().ClearReleaseAsync(original);Assert.Single(tab.Values);Assert.Equal(decision.Revision(),(await Store().ReadDecisionAsync(decision.Review.Scope.Company,"reviewer"))!.Revision());
    }
    [Fact] public async Task AfeRecoveryUsesExistingProtectedTabOwnerAndCannotReplaceOrClearAnotherOriginal()
    {
        using var f=new DevelopmentAfeStateTests.Fixture();await f.Prepare();var original=f.State.Prepared!;var scope=original.Review.Source.Review.Scope;
        var tab=new TabStorage();var protection=new EphemeralDataProtectionProvider();DevelopmentDraftIntentStore Store()=>new(new ProtectedSessionStorage(tab,protection));
        await Store().RetainAfeAsync(original);var encrypted=Assert.Single(tab.Values).Value;Assert.DoesNotContain(original.Rationale,encrypted);Assert.DoesNotContain(original.IntentId.ToString(),encrypted);
        Assert.Equal(original.Revision(),(await Store().ReadAfeAsync(scope.Company,scope.ActorUserId))!.Revision());Assert.Null(await Store().ReadAfeAsync(new("tenant","other"),scope.ActorUserId));Assert.Null(await Store().ReadAfeAsync(scope.Company,"someone-else"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().RetainAfeAsync(original with{IntentId=Guid.NewGuid()}));await Assert.ThrowsAsync<InvalidOperationException>(()=>Store().ClearAfeAsync(original with{Amount=1.125m}));
        Assert.Equal(original.Revision(),(await Store().ReadAfeAsync(scope.Company,scope.ActorUserId))!.Revision());await Store().ClearAfeAsync(original);Assert.Empty(tab.Values);
        await Store().RetainAfeAsync(original);var key=Assert.Single(tab.Values).Key;tab.Values[key]="corrupted";await Assert.ThrowsAnyAsync<Exception>(()=>Store().ReadAfeAsync(scope.Company,scope.ActorUserId));
    }

}

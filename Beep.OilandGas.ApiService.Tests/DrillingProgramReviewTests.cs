#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramReviewTests
{
    internal static DrillingProgramReviewReadiness Review(DrillingProgramDraftNativeTests.Fixture f,string mode="ready")
    {
        var source=new DrillingProgramReviewSource(f.Current,f.Command.ToRecord(DateTime.UtcNow),"",DateTimeOffset.UtcNow);source=source with{Revision=source.ContentRevision()};
        var recipients=mode=="blocked"?Array.Empty<RepositoryCompanyRoleRecipient>():new[]{new RepositoryCompanyRoleRecipient("program-reviewer","DRILLING_REVIEW")};
        var stages=new[]{new DrillingProgramReviewStage(new("technical","Independent program review",1,"ConfiguredProgramReviewer",recipients.Select(row=>row.PersonaCode).ToArray(),24),new("technical","ConfiguredProgramReviewer",recipients,new('B',64)))};
        var value=new DrillingProgramReviewReadiness(source,mode=="empty"?[]:[new("program-review","Recorded program review","1.0",new('A',64),stages)],"",DateTimeOffset.UtcNow);value=value with{Revision=value.ContentRevision()};var scope=source.Basis.Scope();value.Require(scope.Company,scope.ActorUserId,source.Program.PROGRAM_ID,source.Program.PROGRAM_DRAFT_REVISION);return value;
    }
    [Theory][InlineData("ready")][InlineData("blocked")][InlineData("empty")]
    public async Task ExactSavedVersionKeepsReadinessSeparateFromDraftAndInvestmentApproval(string mode)
    {
        using var f=new DrillingProgramDraftNativeTests.Fixture();await f.Prepare();var review=Review(f,mode);Assert.Equal("DRAFT",review.Source.Program.PROGRAM_STATUS);Assert.Equal("APPROVED",review.Source.Basis.Funding.Progress!.State);if(mode=="empty")Assert.Empty(review.Options);else Assert.Equal(mode=="ready",Assert.Single(review.Options).Ready);Assert.Equal(0,f.Store.Native.Writes);
        var copy=JsonSerializer.Deserialize<DrillingProgramReviewReadiness>(JsonSerializer.Serialize(review))!;Assert.Equal(review.Revision,copy.ContentRevision());
    }
    [Theory][InlineData("maker")][InlineData("sender")][InlineData("duplicate")][InlineData("workspace")][InlineData("role")][InlineData("program")][InlineData("company")][InlineData("revision")][InlineData("stage-order")]
    public async Task IncompletePrivateSourceOrRecipientProofCannotBecomeRoutingReadiness(string fault)
    {
        using var f=new DrillingProgramDraftNativeTests.Fixture();await f.Prepare();var review=Review(f);var option=review.Options[0];var stage=option.Stages[0];
        if(fault is "maker" or "sender" or "duplicate" or "workspace" or "role")
        {
            var receiver=stage.Authority.Recipients[0];var rows=fault switch{"maker"=>new[]{receiver with{UserId=review.Source.Program.ROW_CREATED_BY}},"sender"=>new[]{receiver with{UserId=review.Source.Basis.Scope().ActorUserId}},"duplicate"=>new[]{receiver,receiver},"workspace"=>new[]{receiver with{PersonaCode="FOREIGN"}},_=>new[]{receiver}};
            stage=stage with{Authority=stage.Authority with{Recipients=rows,AssignedRole=fault=="role"?"ForeignRole":stage.Authority.AssignedRole}};option=option with{Stages=[stage]};review=review with{Options=[option]};
        }
        if(fault=="program")review.Source.Program.PROGRAM_NAME="Unrecorded source edit";
        if(fault=="company")review.Source.Program.OWNER_COMPANY_BA_ID="foreign";
        if(fault=="stage-order"){var later=stage with{Definition=stage.Definition with{StepId="later",Sequence=2},Authority=stage.Authority with{StepId="later"}};review=review with{Options=[option with{Stages=[later,stage]}]};}
        review=review with{Revision=review.ContentRevision()};var scope=review.Source.Basis.Scope();Assert.ThrowsAny<Exception>(()=>review.Require(scope.Company,scope.ActorUserId,review.Source.Program.PROGRAM_ID,fault=="revision"?new('F',64):review.Source.Program.PROGRAM_DRAFT_REVISION));
    }
    [Theory][InlineData("valid")][InlineData("entity")][InlineData("type")][InlineData("optional")][InlineData("next")][InlineData("conditional")][InlineData("config")][InlineData("name")][InlineData("role")]
    public void CanonicalDefinitionAdmissionRequiresExplicitMandatoryOrderedProgramApproval(string fault)
    {
        var step=new ProcessStepDefinition{StepId="technical",StepName="Independent technical review",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["ConfiguredProgramReviewer"]};
        var definition=new ProcessDefinition{ProcessId="program-review",ProcessName="Recorded program review",Version="1.0",ProcessType=fault=="type"?"ACCOUNTING":"DRILLING",EntityType=fault=="entity"?"AFE":"DRILLING_PROGRAM",IsActive=true,Steps=[step]};
        if(fault=="optional")step.IsRequired=false;if(fault=="next")step.NextStepId="guessed";if(fault=="conditional")step.ConditionalNextSteps=["branch"];if(fault=="config")step.StepConfiguration=new(){["AutoApprove"]=true};if(fault=="name")step.StepName="";if(fault=="role")step.RequiredRoles.Add("OtherRole");
        if(fault=="valid")Assert.Single(DevelopmentReviewDefinition.RequireProgram(definition));else Assert.Throws<InvalidOperationException>(()=>DevelopmentReviewDefinition.RequireProgram(definition));
    }
}
#endif

using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyDrillingProgramService
{
    private async Task<DrillingProgramReviewSource> ReadProgramReviewSourceAsync(Access access,DrillingProgramBasis basis,string programId,string programRevision)
    {
        var rows=await native.ReadProgramDraftsAsync(basis,access.Check);var row=rows.Drafts.SingleOrDefault(row=>row.PROGRAM_ID==programId)??throw new InvalidOperationException("The exact saved program is not recorded for this well/funding source.");if(row.PROGRAM_DRAFT_REVISION!=programRevision)throw new InvalidOperationException("The saved program version changed. Reload its actual evidence before reviewing routing.");
        var result=new DrillingProgramReviewSource(basis,row,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require();return result;
    }
    public async Task<DrillingProgramReviewReadiness> ReviewRoutingAsync(ClaimsPrincipal user,string process,string afe,string well,string programId,string programRevision)
    {
        if(!LeaseSummaryValidator.Identifier(programId) || !ProcessDefinitionRevision.IsValid(programRevision))throw new InvalidOperationException("Reload the exact saved program before reviewing independent routing.");
        var access=await Capture(user,process,afe,well,false,true);var selected=await company.RefreshCompanyAsync();var canPropose=selected.Permissions.Contains("Drilling.Create",StringComparer.Ordinal) && selected.Permissions.Contains("Workflow.Start",StringComparer.Ordinal);
        async Task<string> Sender()
        {
            await access.Check();var current=await company.RefreshCompanyAsync();
            if(canPropose && (!current.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) || !current.Permissions.Contains("Drilling.Create",StringComparer.Ordinal)))throw new UnauthorizedAccessException("Current drilling-create and workflow-start authority is required to inspect private proposed program assignments.");
            return JsonSerializer.Serialize(new{current.Scope,current.UserId,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal)});
        }
        var sender=await Sender();
        async Task<DrillingProgramReviewSource> ReadSource()
        {
            if(await Sender()!=sender)throw new UnauthorizedAccessException("The original program routing authority changed.");var basis=await access.Read();
            return await ReadProgramReviewSourceAsync(access,basis,programId,programRevision);
        }
        var initial=await ReadSource();var scope=initial.Basis.Scope();var binding=scope.LifecycleDatabase;
        var lifecycle=editor.GetDataSource(binding.ConnectionName)??throw new InvalidOperationException("The original program-review lifecycle source is unavailable.");
        async Task Guard(bool idle=true)
        {
            if(await Sender()!=sender || await modules.ResolveAsync("LIFECYCLE")!=binding.ConnectionName || await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],binding.ConnectionName)!=binding.Fingerprint || !ReferenceEquals(lifecycle,editor.GetDataSource(binding.ConnectionName)))
                throw new UnauthorizedAccessException("The original program routing actor or lifecycle binding changed.");
            MigrationConnectionTarget.Validate(editor,lifecycle,binding.ConnectionName);if(idle)PPDMDataSourceTransaction.RequireIdle(lifecycle);
        }
        if(processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(binding.ConnectionName) is not PPDMProcessService workflow)throw new InvalidOperationException("The canonical bound lifecycle owner is required for program review.");
        async Task<DrillingProgramReviewReadiness> Observe()
        {
            await Guard();var existing=await workflow.FindRecordedProgramReviewAsync(scope.Company,scope.FieldId,programId,()=>Guard(false));var definitions=existing is null && canPropose?CompanyInventorySourceReview.Copy(await workflow.GetProcessDefinitionsByTypeAsync("DRILLING")??throw new InvalidOperationException("The complete program review catalog is unavailable.")):new List<Beep.OilandGas.Models.Processes.ProcessDefinition>();
            if(definitions.Count>128 || definitions.Any(row=>row is null || !row.IsActive || row.ProcessType!="DRILLING" || !LeaseSummaryValidator.Identifier(row.ProcessId)) || definitions.Select(row=>row.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)throw new InvalidOperationException("The complete program review catalog is ambiguous or exceeds its bound.");
            var options=new List<DrillingProgramReviewOption>();var roles=new Dictionary<string,DevelopmentReviewRecipientStep>(StringComparer.Ordinal);var stageCount=0;
            foreach(var definition in definitions.Where(row=>existing is null && row.EntityType=="DRILLING_PROGRAM").OrderBy(row=>row.ProcessId,StringComparer.Ordinal))
            {
                var configured=DevelopmentReviewDefinition.RequireProgram(definition);stageCount+=configured.Count;if(stageCount>256)throw new InvalidOperationException("The complete program review catalog exceeds 256 configured stages.");var stages=new List<DrillingProgramReviewStage>();
                foreach(var step in configured)
                {
                    await Guard();var role=step.RequiredRoles[0];if(!roles.TryGetValue(role,out var authority)){authority=await recipients.ReadProgramAsync(initial,step.StepId,role);roles.Add(role,authority);}var current=authority with{StepId=step.StepId};
                    stages.Add(new(new(step.StepId,step.StepName,step.SequenceNumber,role,current.Recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),step.SlaHours),current));
                }
                var option=new DrillingProgramReviewOption(definition.ProcessId,definition.ProcessName,definition.Version,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),stages);option.Require(initial);options.Add(option);
            }
            await Guard();var result=new DrillingProgramReviewReadiness(initial,options,"",DateTimeOffset.UtcNow){ExistingReviewInstanceId=existing};result=result with{Revision=result.ContentRevision()};result.Require(scope.Company,scope.ActorUserId,programId,programRevision);return result;
        }
        var first=await Observe();var closing=await Observe();if(first.Revision!=closing.Revision)throw new InvalidOperationException("The configured program review stages or independent recipients changed during review.");
        var after=await ReadSource();if(after.Revision!=initial.Revision)throw new InvalidOperationException("The original saved program, well or approved funding changed during routing review.");await access.Confirm();await Guard();return CompanyInventorySourceReview.Copy(closing);
    }
}

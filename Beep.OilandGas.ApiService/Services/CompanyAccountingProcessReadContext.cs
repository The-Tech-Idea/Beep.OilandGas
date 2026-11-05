using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reuses existing company/module/process owners and captures fixed PRODUCTION plus LIFECYCLE evidence.</summary>
public sealed class CompanyAccountingProcessReadContext(CompanyModuleConnection company,ModuleConnectionResolver modules,IProcessService processes)
    : ICompanyAccountingProcessReadContext
{
    public async Task<(RepositoryCompanyAccess Access,string Binding,string Actor,IProcessService Process)> CaptureProcessAsync(ClaimsPrincipal user)
    {
        var production=await company.CaptureAsync(user);
        var lifecycle=await modules.ResolveAsync("LIFECYCLE");
        var lifecycleRevision=await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],lifecycle);
        if(processes is not BoundProcessService bound)throw new InvalidOperationException("A fixed-target company LIFECYCLE process owner is required.");
        var fixedProcess=await bound.CreateBoundServiceAsync(lifecycle);
        var closingProduction=await company.CaptureAsync(user);
        var closingLifecycle=await modules.ResolveAsync("LIFECYCLE");
        if(production.Access.Scope!=closingProduction.Access.Scope || production.Actor!=closingProduction.Actor || production.Binding!=closingProduction.Binding ||
            lifecycle!=closingLifecycle || lifecycleRevision!=await modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],closingLifecycle))
            throw new UnauthorizedAccessException("Original company accounting/process database bindings changed.");
        var binding=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Production=production.Binding,Lifecycle=lifecycleRevision}));
        return(closingProduction.Access,binding,closingProduction.Actor,fixedProcess);
    }
}

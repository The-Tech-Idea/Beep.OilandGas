using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.ProductionOperations.Models;
public sealed record CompanyWorkOrderMaterialReleaseReferenceQuery(CompanyWorkOrderMaterialAllocationReferenceQuery Allocation,Guid IntentId)
{
    public void Require(){if(Allocation is null || IntentId==Guid.Empty)throw new ArgumentException("The original allocation and release intent references are required.");Allocation.Require();}
}
public sealed record CompanyWorkOrderMaterialReleaseRecovery(CompanyWorkOrderMaterialReleaseCommand Command,WORK_ORDER_MATERIAL_RELEASE Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialReleaseReferenceQuery query)
    {
        query.Require();if(Command is null || Recorded is null || Command.IntentId!=query.IntentId)throw new InvalidOperationException("Complete original release reference is unavailable.");Command.Require();Command.Original.Require(company,actor,query.Allocation);Command.RequireRecorded(Recorded);
    }
}
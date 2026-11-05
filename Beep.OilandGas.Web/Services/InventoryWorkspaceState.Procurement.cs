using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed partial class InventoryWorkspaceState
{
    private PurchaseOrderWorkspaceState? procurement;
    public bool ProcurementWorkBlocked=>procurement?.HasCompetingProcurementWork==true;
    public string? ProcurementRecoveryRoute=>procurement?.ReceiverRecoveryRoute ?? procurement?.LinkCycleRecoveryRoute ?? procurement?.LinkDecisionRecoveryRoute ?? procurement?.LinkPreparationRecoveryRoute;
    public event Action? ProcurementChanged;
    internal void AttachProcurement(PurchaseOrderWorkspaceState owner,CompanyClientContext context)
    {
        if(!ReferenceEquals(company,context) || procurement is not null && !ReferenceEquals(procurement,owner))
            throw new InvalidOperationException("Warehouse coordination requires the same scoped company and procurement owner.");
        if(procurement is not null)return;procurement=owner;
        owner.ReceiverChanged+=ProcurementWorkChanged;owner.InventoryLinkChanged+=ProcurementWorkChanged;
        owner.LinkPreparationChanged+=ProcurementWorkChanged;owner.LinkDecisionChanged+=ProcurementWorkChanged;owner.LinkCycleChanged+=ProcurementWorkChanged;
    }
    internal void DetachProcurement(PurchaseOrderWorkspaceState owner)
    {
        if(!ReferenceEquals(procurement,owner))return;
        owner.ReceiverChanged-=ProcurementWorkChanged;owner.InventoryLinkChanged-=ProcurementWorkChanged;
        owner.LinkPreparationChanged-=ProcurementWorkChanged;owner.LinkDecisionChanged-=ProcurementWorkChanged;owner.LinkCycleChanged-=ProcurementWorkChanged;procurement=null;
    }
    private void ProcurementWorkChanged(){ProcurementChanged?.Invoke();}
}

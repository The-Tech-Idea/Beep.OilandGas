using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;
/// <summary>One retained cycle preimage; references predecessors by digest rather than embedding history.</summary>
public sealed class PO_INVENTORY_LINK_HISTORY:ModelEntityBase
{
    [Key,MaxLength(32)] public string PO_INVENTORY_LINK_HISTORY_ID {get;set;}="";
    [MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [MaxLength(128)] public string PURCHASE_ORDER_ID {get;set;}="";
    [MaxLength(128)] public string PO_LINE_ITEM_ID {get;set;}="";
    public int SEQUENCE_NUMBER {get;set;}
    [MaxLength(32)] public string? PREVIOUS_ENTRY_ID {get;set;}
    [MaxLength(64)] public string? PREVIOUS_REVISION {get;set;}
    [MaxLength(32)] public string CYCLE_INTENT_ID {get;set;}="";
    [MaxLength(64)] public string SOURCE_REVISION {get;set;}="";
    [MaxLength(512)] public string BINDING_REVISION {get;set;}="";
    [MaxLength(512)] public string REASON {get;set;}="";
    [MaxLength(4194304)] public string ARCHIVED_LINE_JSON {get;set;}="";
    [MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}

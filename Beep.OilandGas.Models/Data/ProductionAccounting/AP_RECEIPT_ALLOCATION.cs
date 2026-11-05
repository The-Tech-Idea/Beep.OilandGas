using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;

/// <summary>Company-owned recorded receipt quantity assigned to one exact owned AP invoice line.</summary>
public sealed class AP_RECEIPT_ALLOCATION : ModelEntityBase
{
    private string? reviewCommand;
    /// <summary>Original bounded allocation intent; historical rows may have no command proof.</summary>
    [MaxLength(4000)]
    public string? REVIEW_COMMAND_JSON {get=>reviewCommand;set=>SetProperty(ref reviewCommand,value);}
    private string? ownerTenant;
    [MaxLength(128)]
    public string? OWNER_TENANT_ID {get=>ownerTenant;set=>SetProperty(ref ownerTenant,value);}
    private string? ownerCompany;
    [MaxLength(128)]
    public string? OWNER_COMPANY_BA_ID {get=>ownerCompany;set=>SetProperty(ref ownerCompany,value);}
    private string AP_RECEIPT_ALLOCATION_IDValue = "";
    [Key]
    [MaxLength(128)]
    public string AP_RECEIPT_ALLOCATION_ID {get=>AP_RECEIPT_ALLOCATION_IDValue;set=>SetProperty(ref AP_RECEIPT_ALLOCATION_IDValue,value);}
    private string AP_INVOICE_IDValue = "";
    [MaxLength(128)]
    public string AP_INVOICE_ID {get=>AP_INVOICE_IDValue;set=>SetProperty(ref AP_INVOICE_IDValue,value);}
    private string AP_INVOICE_LINE_IDValue = "";
    [MaxLength(128)]
    public string AP_INVOICE_LINE_ID {get=>AP_INVOICE_LINE_IDValue;set=>SetProperty(ref AP_INVOICE_LINE_IDValue,value);}
    private string PURCHASE_ORDER_IDValue = "";
    [MaxLength(128)]
    public string PURCHASE_ORDER_ID {get=>PURCHASE_ORDER_IDValue;set=>SetProperty(ref PURCHASE_ORDER_IDValue,value);}
    private string PO_LINE_ITEM_IDValue = "";
    [MaxLength(128)]
    public string PO_LINE_ITEM_ID {get=>PO_LINE_ITEM_IDValue;set=>SetProperty(ref PO_LINE_ITEM_IDValue,value);}
    private string PO_RECEIPT_IDValue = "";
    [MaxLength(128)]
    public string PO_RECEIPT_ID {get=>PO_RECEIPT_IDValue;set=>SetProperty(ref PO_RECEIPT_IDValue,value);}
    private decimal? ALLOCATED_QUANTITYValue;
    public decimal? ALLOCATED_QUANTITY {get=>ALLOCATED_QUANTITYValue;set=>SetProperty(ref ALLOCATED_QUANTITYValue,value);}
    private string? QUANTITY_UOMValue;
    [MaxLength(64)]
    public string? QUANTITY_UOM {get=>QUANTITY_UOMValue;set=>SetProperty(ref QUANTITY_UOMValue,value);}
}

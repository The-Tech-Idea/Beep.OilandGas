using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting;

/// <summary>Recorded vendor invoice line; ownership is inherited from its exact company-owned AP header.</summary>
public sealed class AP_INVOICE_LINE : ModelEntityBase
{
    private string AP_INVOICE_LINE_IDValue = "";
    [Key]
    [MaxLength(128)]
    public string AP_INVOICE_LINE_ID {get=>AP_INVOICE_LINE_IDValue;set=>SetProperty(ref AP_INVOICE_LINE_IDValue,value);}
    private string AP_INVOICE_IDValue = "";
    [MaxLength(128)]
    public string AP_INVOICE_ID {get=>AP_INVOICE_IDValue;set=>SetProperty(ref AP_INVOICE_IDValue,value);}
    private int? LINE_NUMBERValue;
    public int? LINE_NUMBER {get=>LINE_NUMBERValue;set=>SetProperty(ref LINE_NUMBERValue,value);}
    private string? ITEM_DESCRIPTIONValue;
    [MaxLength(1024)]
    public string? ITEM_DESCRIPTION {get=>ITEM_DESCRIPTIONValue;set=>SetProperty(ref ITEM_DESCRIPTIONValue,value);}
    private decimal? QUANTITYValue;
    public decimal? QUANTITY {get=>QUANTITYValue;set=>SetProperty(ref QUANTITYValue,value);}
    private string? QUANTITY_UOMValue;
    [MaxLength(64)]
    public string? QUANTITY_UOM {get=>QUANTITY_UOMValue;set=>SetProperty(ref QUANTITY_UOMValue,value);}
    private decimal? UNIT_PRICEValue;
    public decimal? UNIT_PRICE {get=>UNIT_PRICEValue;set=>SetProperty(ref UNIT_PRICEValue,value);}
    private decimal? LINE_TOTALValue;
    public decimal? LINE_TOTAL {get=>LINE_TOTALValue;set=>SetProperty(ref LINE_TOTALValue,value);}
    private string? CURRENCY_CODEValue;
    [MaxLength(32)]
    public string? CURRENCY_CODE {get=>CURRENCY_CODEValue;set=>SetProperty(ref CURRENCY_CODEValue,value);}
    private string? PURCHASE_ORDER_IDValue;
    [MaxLength(128)]
    public string? PURCHASE_ORDER_ID {get=>PURCHASE_ORDER_IDValue;set=>SetProperty(ref PURCHASE_ORDER_IDValue,value);}
    private string? PO_LINE_ITEM_IDValue;
    [MaxLength(128)]
    public string? PO_LINE_ITEM_ID {get=>PO_LINE_ITEM_IDValue;set=>SetProperty(ref PO_LINE_ITEM_IDValue,value);}
}

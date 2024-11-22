using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class INSTRUMENT_DETAIL: Entity

{

private  System.String INSTRUMENT_IDValue; 
 public System.String INSTRUMENT_ID
        {  
            get  
            {  
                return this.INSTRUMENT_IDValue;  
            }  

          set { SetProperty(ref  INSTRUMENT_IDValue, value); }
        } 
private  System.String DETAIL_IDValue; 
 public System.String DETAIL_ID
        {  
            get  
            {  
                return this.DETAIL_IDValue;  
            }  

          set { SetProperty(ref  DETAIL_IDValue, value); }
        } 
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.Decimal AVERAGE_VALUEValue; 
 public System.Decimal AVERAGE_VALUE
        {  
            get  
            {  
                return this.AVERAGE_VALUEValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUEValue, value); }
        } 
private  System.String AVERAGE_VALUE_OUOMValue; 
 public System.String AVERAGE_VALUE_OUOM
        {  
            get  
            {  
                return this.AVERAGE_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUE_OUOMValue, value); }
        } 
private  System.String AVERAGE_VALUE_UOMValue; 
 public System.String AVERAGE_VALUE_UOM
        {  
            get  
            {  
                return this.AVERAGE_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  AVERAGE_VALUE_UOMValue, value); }
        } 
private  System.Decimal COST_VALUEValue; 
 public System.Decimal COST_VALUE
        {  
            get  
            {  
                return this.COST_VALUEValue;  
            }  

          set { SetProperty(ref  COST_VALUEValue, value); }
        } 
private  System.Decimal CURRENCY_CONVERSIONValue; 
 public System.Decimal CURRENCY_CONVERSION
        {  
            get  
            {  
                return this.CURRENCY_CONVERSIONValue;  
            }  

          set { SetProperty(ref  CURRENCY_CONVERSIONValue, value); }
        } 
private  System.String CURRENCY_OUOMValue; 
 public System.String CURRENCY_OUOM
        {  
            get  
            {  
                return this.CURRENCY_OUOMValue;  
            }  

          set { SetProperty(ref  CURRENCY_OUOMValue, value); }
        } 
private  System.String CURRENCY_UOMValue; 
 public System.String CURRENCY_UOM
        {  
            get  
            {  
                return this.CURRENCY_UOMValue;  
            }  

          set { SetProperty(ref  CURRENCY_UOMValue, value); }
        } 
private  System.String DETAIL_DESCRIPTIONValue; 
 public System.String DETAIL_DESCRIPTION
        {  
            get  
            {  
                return this.DETAIL_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DETAIL_DESCRIPTIONValue, value); }
        } 
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.String INSTRUMENT_DETAIL_CODEValue; 
 public System.String INSTRUMENT_DETAIL_CODE
        {  
            get  
            {  
                return this.INSTRUMENT_DETAIL_CODEValue;  
            }  

          set { SetProperty(ref  INSTRUMENT_DETAIL_CODEValue, value); }
        } 
private  System.String INSTRUMENT_DETAIL_TYPEValue; 
 public System.String INSTRUMENT_DETAIL_TYPE
        {  
            get  
            {  
                return this.INSTRUMENT_DETAIL_TYPEValue;  
            }  

          set { SetProperty(ref  INSTRUMENT_DETAIL_TYPEValue, value); }
        } 
private  System.DateTime MAX_DATEValue; 
 public System.DateTime MAX_DATE
        {  
            get  
            {  
                return this.MAX_DATEValue;  
            }  

          set { SetProperty(ref  MAX_DATEValue, value); }
        } 
private  System.Decimal MAX_VALUEValue; 
 public System.Decimal MAX_VALUE
        {  
            get  
            {  
                return this.MAX_VALUEValue;  
            }  

          set { SetProperty(ref  MAX_VALUEValue, value); }
        } 
private  System.String MAX_VALUE_OUOMValue; 
 public System.String MAX_VALUE_OUOM
        {  
            get  
            {  
                return this.MAX_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_OUOMValue, value); }
        } 
private  System.String MAX_VALUE_UOMValue; 
 public System.String MAX_VALUE_UOM
        {  
            get  
            {  
                return this.MAX_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MAX_VALUE_UOMValue, value); }
        } 
private  System.DateTime MIN_DATEValue; 
 public System.DateTime MIN_DATE
        {  
            get  
            {  
                return this.MIN_DATEValue;  
            }  

          set { SetProperty(ref  MIN_DATEValue, value); }
        } 
private  System.Decimal MIN_VALUEValue; 
 public System.Decimal MIN_VALUE
        {  
            get  
            {  
                return this.MIN_VALUEValue;  
            }  

          set { SetProperty(ref  MIN_VALUEValue, value); }
        } 
private  System.String MIN_VALUE_OUOMValue; 
 public System.String MIN_VALUE_OUOM
        {  
            get  
            {  
                return this.MIN_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_OUOMValue, value); }
        } 
private  System.String MIN_VALUE_UOMValue; 
 public System.String MIN_VALUE_UOM
        {  
            get  
            {  
                return this.MIN_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  MIN_VALUE_UOMValue, value); }
        } 
private  System.String PPDM_GUIDValue; 
 public System.String PPDM_GUID
        {  
            get  
            {  
                return this.PPDM_GUIDValue;  
            }  

          set { SetProperty(ref  PPDM_GUIDValue, value); }
        } 
private  System.Decimal REFERENCE_VALUEValue; 
 public System.Decimal REFERENCE_VALUE
        {  
            get  
            {  
                return this.REFERENCE_VALUEValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUEValue, value); }
        } 
private  System.String REFERENCE_VALUE_OUOMValue; 
 public System.String REFERENCE_VALUE_OUOM
        {  
            get  
            {  
                return this.REFERENCE_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_OUOMValue, value); }
        } 
private  System.String REFERENCE_VALUE_TYPEValue; 
 public System.String REFERENCE_VALUE_TYPE
        {  
            get  
            {  
                return this.REFERENCE_VALUE_TYPEValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_TYPEValue, value); }
        } 
private  System.String REFERENCE_VALUE_UOMValue; 
 public System.String REFERENCE_VALUE_UOM
        {  
            get  
            {  
                return this.REFERENCE_VALUE_UOMValue;  
            }  

          set { SetProperty(ref  REFERENCE_VALUE_UOMValue, value); }
        } 
private  System.String REMARKValue; 
 public System.String REMARK
        {  
            get  
            {  
                return this.REMARKValue;  
            }  

          set { SetProperty(ref  REMARKValue, value); }
        } 
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
        } 
private  System.String ROW_CHANGED_BYValue; 
 public System.String ROW_CHANGED_BY
        {  
            get  
            {  
                return this.ROW_CHANGED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_BYValue, value); }
        } 
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
        {  
            get  
            {  
                return this.ROW_CHANGED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CHANGED_DATEValue, value); }
        } 
private  System.String ROW_CREATED_BYValue; 
 public System.String ROW_CREATED_BY
        {  
            get  
            {  
                return this.ROW_CREATED_BYValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_BYValue, value); }
        } 
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
        {  
            get  
            {  
                return this.ROW_EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EXPIRY_DATEValue, value); }
        } 
private  System.String ROW_QUALITYValue; 
 public System.String ROW_QUALITY
        {  
            get  
            {  
                return this.ROW_QUALITYValue;  
            }  

          set { SetProperty(ref  ROW_QUALITYValue, value); }
        } 


    public INSTRUMENT_DETAIL () { }

  }
}


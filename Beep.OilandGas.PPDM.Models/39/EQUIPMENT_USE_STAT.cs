using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class EQUIPMENT_USE_STAT: Entity,IPPDMEntity

{

private  System.String EQUIPMENT_IDValue; 
 public System.String EQUIPMENT_ID
        {  
            get  
            {  
                return this.EQUIPMENT_IDValue;  
            }  

          set { SetProperty(ref  EQUIPMENT_IDValue, value); }
        } 
private  System.String SPEC_IDValue; 
 public System.String SPEC_ID
        {  
            get  
            {  
                return this.SPEC_IDValue;  
            }  

          set { SetProperty(ref  SPEC_IDValue, value); }
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
private  System.Decimal COSTValue; 
 public System.Decimal COST
        {  
            get  
            {  
                return this.COSTValue;  
            }  

          set { SetProperty(ref  COSTValue, value); }
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
private  System.String DATE_FORMAT_DESCValue; 
 public System.String DATE_FORMAT_DESC
        {  
            get  
            {  
                return this.DATE_FORMAT_DESCValue;  
            }  

          set { SetProperty(ref  DATE_FORMAT_DESCValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
        } 
private  System.DateTime? END_TIMEValue; 
 public System.DateTime? END_TIME
        {  
            get  
            {  
                return this.END_TIMEValue;  
            }  

          set { SetProperty(ref  END_TIMEValue, value); }
        } 
private  System.String EQUIP_USE_STAT_TYPEValue; 
 public System.String EQUIP_USE_STAT_TYPE
        {  
            get  
            {  
                return this.EQUIP_USE_STAT_TYPEValue;  
            }  

          set { SetProperty(ref  EQUIP_USE_STAT_TYPEValue, value); }
        } 
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.DateTime? MAX_DATEValue; 
 public System.DateTime? MAX_DATE
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
private  System.DateTime? MIN_DATEValue; 
 public System.DateTime? MIN_DATE
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
private  System.Decimal PERCENT_CAPACITYValue; 
 public System.Decimal PERCENT_CAPACITY
        {  
            get  
            {  
                return this.PERCENT_CAPACITYValue;  
            }  

          set { SetProperty(ref  PERCENT_CAPACITYValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.DateTime? START_TIMEValue; 
 public System.DateTime? START_TIME
        {  
            get  
            {  
                return this.START_TIMEValue;  
            }  

          set { SetProperty(ref  START_TIMEValue, value); }
        } 
private  System.String TIMEZONEValue; 
 public System.String TIMEZONE
        {  
            get  
            {  
                return this.TIMEZONEValue;  
            }  

          set { SetProperty(ref  TIMEZONEValue, value); }
        } 
private  System.String USE_CODEValue; 
 public System.String USE_CODE
        {  
            get  
            {  
                return this.USE_CODEValue;  
            }  

          set { SetProperty(ref  USE_CODEValue, value); }
        } 
private  System.String USE_DESCValue; 
 public System.String USE_DESC
        {  
            get  
            {  
                return this.USE_DESCValue;  
            }  

          set { SetProperty(ref  USE_DESCValue, value); }
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
private  System.DateTime? ROW_CHANGED_DATEValue; 
 public System.DateTime? ROW_CHANGED_DATE
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
private  System.DateTime? ROW_CREATED_DATEValue; 
 public System.DateTime? ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime? ROW_EFFECTIVE_DATEValue; 
 public System.DateTime? ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime? ROW_EXPIRY_DATEValue; 
 public System.DateTime? ROW_EXPIRY_DATE
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


    public EQUIPMENT_USE_STAT () { }

  }
}


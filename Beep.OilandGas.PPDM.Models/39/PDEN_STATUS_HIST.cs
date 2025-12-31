using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PDEN_STATUS_HIST: Entity,IPPDMEntity

{

private  System.String PDEN_SUBTYPEValue; 
 public System.String PDEN_SUBTYPE
        {  
            get  
            {  
                return this.PDEN_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PDEN_SUBTYPEValue, value); }
        } 
private  System.String PDEN_IDValue; 
 public System.String PDEN_ID
        {  
            get  
            {  
                return this.PDEN_IDValue;  
            }  

          set { SetProperty(ref  PDEN_IDValue, value); }
        } 
private  System.String PDEN_SOURCEValue; 
 public System.String PDEN_SOURCE
        {  
            get  
            {  
                return this.PDEN_SOURCEValue;  
            }  

          set { SetProperty(ref  PDEN_SOURCEValue, value); }
        } 
private  System.String STATUS_IDValue; 
 public System.String STATUS_ID
        {  
            get  
            {  
                return this.STATUS_IDValue;  
            }  

          set { SetProperty(ref  STATUS_IDValue, value); }
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
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
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
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.Decimal PERCENT_CAPABILITYValue; 
 public System.Decimal PERCENT_CAPABILITY
        {  
            get  
            {  
                return this.PERCENT_CAPABILITYValue;  
            }  

          set { SetProperty(ref  PERCENT_CAPABILITYValue, value); }
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
private  System.DateTime? START_TIMEValue; 
 public System.DateTime? START_TIME
        {  
            get  
            {  
                return this.START_TIMEValue;  
            }  

          set { SetProperty(ref  START_TIMEValue, value); }
        } 
private  System.String STATUSValue; 
 public System.String STATUS
        {  
            get  
            {  
                return this.STATUSValue;  
            }  

          set { SetProperty(ref  STATUSValue, value); }
        } 
private  System.DateTime? STATUS_DATEValue; 
 public System.DateTime? STATUS_DATE
        {  
            get  
            {  
                return this.STATUS_DATEValue;  
            }  

          set { SetProperty(ref  STATUS_DATEValue, value); }
        } 
private  System.String STATUS_TYPEValue; 
 public System.String STATUS_TYPE
        {  
            get  
            {  
                return this.STATUS_TYPEValue;  
            }  

          set { SetProperty(ref  STATUS_TYPEValue, value); }
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


    public PDEN_STATUS_HIST () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PIPELINE_MAINTENANCE: Entity

{

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String MAINTENANCE_IDValue; 
 public System.String MAINTENANCE_ID
        {  
            get  
            {  
                return this.MAINTENANCE_IDValue;  
            }  

          set { SetProperty(ref  MAINTENANCE_IDValue, value); }
        } 
private  System.String MAINTENANCE_TYPEValue; 
 public System.String MAINTENANCE_TYPE
        {  
            get  
            {  
                return this.MAINTENANCE_TYPEValue;  
            }  

          set { SetProperty(ref  MAINTENANCE_TYPEValue, value); }
        } 
private  System.DateTime SCHEDULE_DATEValue; 
 public System.DateTime SCHEDULE_DATE
        {  
            get  
            {  
                return this.SCHEDULE_DATEValue;  
            }  

          set { SetProperty(ref  SCHEDULE_DATEValue, value); }
        } 
private  System.DateTime COMPLETION_DATEValue; 
 public System.DateTime COMPLETION_DATE
        {  
            get  
            {  
                return this.COMPLETION_DATEValue;  
            }  

          set { SetProperty(ref  COMPLETION_DATEValue, value); }
        } 
private  System.String CREWValue; 
 public System.String CREW
        {  
            get  
            {  
                return this.CREWValue;  
            }  

          set { SetProperty(ref  CREWValue, value); }
        } 
private  System.Decimal MAINTENANCE_COSTValue; 
 public System.Decimal MAINTENANCE_COST
        {  
            get  
            {  
                return this.MAINTENANCE_COSTValue;  
            }  

          set { SetProperty(ref  MAINTENANCE_COSTValue, value); }
        } 
private  System.String MAINTENANCE_COST_OUOMValue; 
 public System.String MAINTENANCE_COST_OUOM
        {  
            get  
            {  
                return this.MAINTENANCE_COST_OUOMValue;  
            }  

          set { SetProperty(ref  MAINTENANCE_COST_OUOMValue, value); }
        } 
private  System.String SEGMENT_IDValue; 
 public System.String SEGMENT_ID
        {  
            get  
            {  
                return this.SEGMENT_IDValue;  
            }  

          set { SetProperty(ref  SEGMENT_IDValue, value); }
        } 
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
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


    public PIPELINE_MAINTENANCE () { }

  }
}


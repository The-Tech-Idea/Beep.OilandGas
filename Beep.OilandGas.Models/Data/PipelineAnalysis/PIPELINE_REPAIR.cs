using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_REPAIR: Entity,IPPDMEntity

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
private  System.String REPAIR_IDValue; 
 public System.String REPAIR_ID
        {  
            get  
            {  
                return this.REPAIR_IDValue;  
            }  

          set { SetProperty(ref  REPAIR_IDValue, value); }
        } 
private  System.String ANOMALY_IDValue; 
 public System.String ANOMALY_ID
        {  
            get  
            {  
                return this.ANOMALY_IDValue;  
            }  

          set { SetProperty(ref  ANOMALY_IDValue, value); }
        } 
private  System.String REPAIR_TYPEValue; 
 public System.String REPAIR_TYPE
        {  
            get  
            {  
                return this.REPAIR_TYPEValue;  
            }  

          set { SetProperty(ref  REPAIR_TYPEValue, value); }
        } 
private  System.DateTime? REPAIR_DATEValue; 
 public System.DateTime? REPAIR_DATE
        {  
            get  
            {  
                return this.REPAIR_DATEValue;  
            }  

          set { SetProperty(ref  REPAIR_DATEValue, value); }
        } 
private  System.String REPAIR_METHODValue; 
 public System.String REPAIR_METHOD
        {  
            get  
            {  
                return this.REPAIR_METHODValue;  
            }  

          set { SetProperty(ref  REPAIR_METHODValue, value); }
        } 
private  System.String CONTRACTORValue; 
 public System.String CONTRACTOR
        {  
            get  
            {  
                return this.CONTRACTORValue;  
            }  

          set { SetProperty(ref  CONTRACTORValue, value); }
        } 
private  System.Decimal REPAIR_COSTValue; 
 public System.Decimal REPAIR_COST
        {  
            get  
            {  
                return this.REPAIR_COSTValue;  
            }  

          set { SetProperty(ref  REPAIR_COSTValue, value); }
        } 
private  System.String REPAIR_COST_OUOMValue; 
 public System.String REPAIR_COST_OUOM
        {  
            get  
            {  
                return this.REPAIR_COST_OUOMValue;  
            }  

          set { SetProperty(ref  REPAIR_COST_OUOMValue, value); }
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
private  System.DateTime? EFFECTIVE_DATEValue; 
 public System.DateTime? EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
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


    public PIPELINE_REPAIR () { }

  }
}


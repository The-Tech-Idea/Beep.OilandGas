using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_REPAIR : ModelEntityBase {

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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PIPELINE_REPAIR () { }

  }
}



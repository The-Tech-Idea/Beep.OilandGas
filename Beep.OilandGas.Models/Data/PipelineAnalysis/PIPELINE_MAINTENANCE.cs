using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_MAINTENANCE : ModelEntityBase {

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
private  System.DateTime? SCHEDULE_DATEValue; 
 public System.DateTime? SCHEDULE_DATE
        {  
            get  
            {  
                return this.SCHEDULE_DATEValue;  
            }  

          set { SetProperty(ref  SCHEDULE_DATEValue, value); }
        } 
private  System.DateTime? COMPLETION_DATEValue; 
 public System.DateTime? COMPLETION_DATE
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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PIPELINE_MAINTENANCE () { }

  }
}



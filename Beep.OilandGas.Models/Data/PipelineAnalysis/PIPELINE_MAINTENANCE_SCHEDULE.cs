using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE_MAINTENANCE_SCHEDULE : ModelEntityBase {

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String SCHEDULE_IDValue; 
 public System.String SCHEDULE_ID
        {  
            get  
            {  
                return this.SCHEDULE_IDValue;  
            }  

          set { SetProperty(ref  SCHEDULE_IDValue, value); }
        } 
private  System.String SCHEDULE_TYPEValue; 
 public System.String SCHEDULE_TYPE
        {  
            get  
            {  
                return this.SCHEDULE_TYPEValue;  
            }  

          set { SetProperty(ref  SCHEDULE_TYPEValue, value); }
        } 
private  System.String FREQUENCYValue; 
 public System.String FREQUENCY
        {  
            get  
            {  
                return this.FREQUENCYValue;  
            }  

          set { SetProperty(ref  FREQUENCYValue, value); }
        } 
private  System.DateTime? NEXT_DUE_DATEValue; 
 public System.DateTime? NEXT_DUE_DATE
        {  
            get  
            {  
                return this.NEXT_DUE_DATEValue;  
            }  

          set { SetProperty(ref  NEXT_DUE_DATEValue, value); }
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

    public PIPELINE_MAINTENANCE_SCHEDULE () { }

  }
}

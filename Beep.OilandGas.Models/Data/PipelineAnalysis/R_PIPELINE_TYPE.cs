using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class R_PIPELINE_TYPE : ModelEntityBase {

private  System.String PIPELINE_TYPEValue; 
 public System.String PIPELINE_TYPE
        {  
            get  
            {  
                return this.PIPELINE_TYPEValue;  
            }  

          set { SetProperty(ref  PIPELINE_TYPEValue, value); }
        } 
private  System.String ABBREVIATIONValue; 
 public System.String ABBREVIATION
        {  
            get  
            {  
                return this.ABBREVIATIONValue;  
            }  

          set { SetProperty(ref  ABBREVIATIONValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String LONG_NAMEValue; 
 public System.String LONG_NAME
        {  
            get  
            {  
                return this.LONG_NAMEValue;  
            }  

          set { SetProperty(ref  LONG_NAMEValue, value); }
        } 
private  System.String REMARKValue; 

private  System.String SHORT_NAMEValue; 
 public System.String SHORT_NAME
        {  
            get  
            {  
                return this.SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  SHORT_NAMEValue, value); }
        } 
private  System.String SOURCEValue; 

    public R_PIPELINE_TYPE () { }

  }
}



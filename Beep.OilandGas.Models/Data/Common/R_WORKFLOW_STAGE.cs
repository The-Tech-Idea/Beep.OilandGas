using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
public partial class R_WORKFLOW_STAGE : ModelEntityBase

{

private  System.String WORKFLOW_STAGEValue; 
 public System.String WORKFLOW_STAGE
        {  
            get  
            {  
                return this.WORKFLOW_STAGEValue;  
            }  

          set { SetProperty(ref  WORKFLOW_STAGEValue, value); }
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

    public R_WORKFLOW_STAGE () { }

  }
}

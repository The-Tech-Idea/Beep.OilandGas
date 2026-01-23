using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_WORKFLOW_STAGE : ModelEntityBase

{

private  System.String PROSPECT_IDValue; 
 public System.String PROSPECT_ID
        {  
            get  
            {  
                return this.PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROSPECT_IDValue, value); }
        } 
private  System.Decimal STAGE_SEQ_NOValue; 
 public System.Decimal STAGE_SEQ_NO
        {  
            get  
            {  
                return this.STAGE_SEQ_NOValue;  
            }  

          set { SetProperty(ref  STAGE_SEQ_NOValue, value); }
        } 
private  System.String WORKFLOW_STAGEValue; 
 public System.String WORKFLOW_STAGE
        {  
            get  
            {  
                return this.WORKFLOW_STAGEValue;  
            }  

          set { SetProperty(ref  WORKFLOW_STAGEValue, value); }
        } 
private  System.DateTime? STAGE_START_DATEValue; 
 public System.DateTime? STAGE_START_DATE
        {  
            get  
            {  
                return this.STAGE_START_DATEValue;  
            }  

          set { SetProperty(ref  STAGE_START_DATEValue, value); }
        } 
private  System.DateTime? STAGE_END_DATEValue; 
 public System.DateTime? STAGE_END_DATE
        {  
            get  
            {  
                return this.STAGE_END_DATEValue;  
            }  

          set { SetProperty(ref  STAGE_END_DATEValue, value); }
        } 
private  System.String STAGE_OUTCOMEValue; 
 public System.String STAGE_OUTCOME
        {  
            get  
            {  
                return this.STAGE_OUTCOMEValue;  
            }  

          set { SetProperty(ref  STAGE_OUTCOMEValue, value); }
        } 
private  System.String NEXT_STAGEValue; 
 public System.String NEXT_STAGE
        {  
            get  
            {  
                return this.NEXT_STAGEValue;  
            }  

          set { SetProperty(ref  NEXT_STAGEValue, value); }
        } 
private  System.String APPROVALS_REQUIREDValue; 
 public System.String APPROVALS_REQUIRED
        {  
            get  
            {  
                return this.APPROVALS_REQUIREDValue;  
            }  

          set { SetProperty(ref  APPROVALS_REQUIREDValue, value); }
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

    public PROSPECT_WORKFLOW_STAGE () { }

  }
}



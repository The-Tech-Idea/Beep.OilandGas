using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_WORKFLOW_STAGE: Entity,IPPDMEntity

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


    public PROSPECT_WORKFLOW_STAGE () { }

  }
}

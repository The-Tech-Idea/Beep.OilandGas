using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_HISTORY : ModelEntityBase

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
private  System.Decimal HISTORY_SEQ_NOValue; 
 public System.Decimal HISTORY_SEQ_NO
        {  
            get  
            {  
                return this.HISTORY_SEQ_NOValue;  
            }  

          set { SetProperty(ref  HISTORY_SEQ_NOValue, value); }
        } 
private  System.String FIELD_NAMEValue; 
 public System.String FIELD_NAME
        {  
            get  
            {  
                return this.FIELD_NAMEValue;  
            }  

          set { SetProperty(ref  FIELD_NAMEValue, value); }
        } 
private  System.String OLD_VALUEValue; 
 public System.String OLD_VALUE
        {  
            get  
            {  
                return this.OLD_VALUEValue;  
            }  

          set { SetProperty(ref  OLD_VALUEValue, value); }
        } 
private  System.String NEW_VALUEValue; 
 public System.String NEW_VALUE
        {  
            get  
            {  
                return this.NEW_VALUEValue;  
            }  

          set { SetProperty(ref  NEW_VALUEValue, value); }
        } 
private  System.String CHANGE_REASONValue; 
 public System.String CHANGE_REASON
        {  
            get  
            {  
                return this.CHANGE_REASONValue;  
            }  

          set { SetProperty(ref  CHANGE_REASONValue, value); }
        } 
private  System.String CHANGED_BYValue; 
 public System.String CHANGED_BY
        {  
            get  
            {  
                return this.CHANGED_BYValue;  
            }  

          set { SetProperty(ref  CHANGED_BYValue, value); }
        } 
private  System.DateTime? CHANGE_DATEValue; 
 public System.DateTime? CHANGE_DATE
        {  
            get  
            {  
                return this.CHANGE_DATEValue;  
            }  

          set { SetProperty(ref  CHANGE_DATEValue, value); }
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

    public PROSPECT_HISTORY () { }

  }
}

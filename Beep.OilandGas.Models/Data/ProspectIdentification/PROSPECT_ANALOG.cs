using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_ANALOG : ModelEntityBase

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
private  System.String ANALOG_IDValue; 
 public System.String ANALOG_ID
        {  
            get  
            {  
                return this.ANALOG_IDValue;  
            }  

          set { SetProperty(ref  ANALOG_IDValue, value); }
        } 
private  System.String ANALOG_TYPEValue; 
 public System.String ANALOG_TYPE
        {  
            get  
            {  
                return this.ANALOG_TYPEValue;  
            }  

          set { SetProperty(ref  ANALOG_TYPEValue, value); }
        } 
private  System.String ANALOG_PROSPECT_IDValue; 
 public System.String ANALOG_PROSPECT_ID
        {  
            get  
            {  
                return this.ANALOG_PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  ANALOG_PROSPECT_IDValue, value); }
        } 
private  System.String ANALOG_FIELD_IDValue; 
 public System.String ANALOG_FIELD_ID
        {  
            get  
            {  
                return this.ANALOG_FIELD_IDValue;  
            }  

          set { SetProperty(ref  ANALOG_FIELD_IDValue, value); }
        } 
private  System.Decimal  SIMILARITY_SCOREValue; 
 public System.Decimal  SIMILARITY_SCORE
        {  
            get  
            {  
                return this.SIMILARITY_SCOREValue;  
            }  

          set { SetProperty(ref  SIMILARITY_SCOREValue, value); }
        } 
private  System.String KEY_SIMILARITIESValue; 
 public System.String KEY_SIMILARITIES
        {  
            get  
            {  
                return this.KEY_SIMILARITIESValue;  
            }  

          set { SetProperty(ref  KEY_SIMILARITIESValue, value); }
        } 
private  System.String USE_CASEValue; 
 public System.String USE_CASE
        {  
            get  
            {  
                return this.USE_CASEValue;  
            }  

          set { SetProperty(ref  USE_CASEValue, value); }
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

    public PROSPECT_ANALOG () { }

  }
}

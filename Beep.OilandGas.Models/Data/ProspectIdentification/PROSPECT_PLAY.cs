using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_PLAY : ModelEntityBase

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
private  System.String PLAY_IDValue; 
 public System.String PLAY_ID
        {  
            get  
            {  
                return this.PLAY_IDValue;  
            }  

          set { SetProperty(ref  PLAY_IDValue, value); }
        } 
private  System.String ASSOCIATION_TYPEValue; 
 public System.String ASSOCIATION_TYPE
        {  
            get  
            {  
                return this.ASSOCIATION_TYPEValue;  
            }  

          set { SetProperty(ref  ASSOCIATION_TYPEValue, value); }
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

    public PROSPECT_PLAY () { }

  }
}

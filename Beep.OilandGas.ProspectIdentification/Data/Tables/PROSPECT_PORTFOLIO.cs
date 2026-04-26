using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_PORTFOLIO : ModelEntityBase

{

private  System.String PORTFOLIO_IDValue; 
 public System.String PORTFOLIO_ID
        {  
            get  
            {  
                return this.PORTFOLIO_IDValue;  
            }  

          set { SetProperty(ref  PORTFOLIO_IDValue, value); }
        } 
private  System.String PORTFOLIO_NAMEValue; 
 public System.String PORTFOLIO_NAME
        {  
            get  
            {  
                return this.PORTFOLIO_NAMEValue;  
            }  

          set { SetProperty(ref  PORTFOLIO_NAMEValue, value); }
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
private  System.String STRATEGIC_OBJECTIVESValue; 
 public System.String STRATEGIC_OBJECTIVES
        {  
            get  
            {  
                return this.STRATEGIC_OBJECTIVESValue;  
            }  

          set { SetProperty(ref  STRATEGIC_OBJECTIVESValue, value); }
        } 
private  System.DateTime? CREATION_DATEValue; 
 public System.DateTime? CREATION_DATE
        {  
            get  
            {  
                return this.CREATION_DATEValue;  
            }  

          set { SetProperty(ref  CREATION_DATEValue, value); }
        } 
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PROSPECT_PORTFOLIO () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_BA : ModelEntityBase

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
private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
        } 
private  System.String ROLE_TYPEValue; 
 public System.String ROLE_TYPE
        {  
            get  
            {  
                return this.ROLE_TYPEValue;  
            }  

          set { SetProperty(ref  ROLE_TYPEValue, value); }
        } 
private  System.Decimal? WORKING_INTERESTValue; 
 public System.Decimal? WORKING_INTEREST
        {  
            get  
            {  
                return this.WORKING_INTERESTValue;  
            }  

          set { SetProperty(ref  WORKING_INTERESTValue, value); }
        } 
private  System.Decimal? CARRIED_INTERESTValue; 
 public System.Decimal? CARRIED_INTEREST
        {  
            get  
            {  
                return this.CARRIED_INTERESTValue;  
            }  

          set { SetProperty(ref  CARRIED_INTERESTValue, value); }
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

    public PROSPECT_BA () { }

  }
}



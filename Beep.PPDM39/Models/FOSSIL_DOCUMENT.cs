using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class FOSSIL_DOCUMENT: Entity

{

private  System.String FOSSIL_IDValue; 
 public System.String FOSSIL_ID
        {  
            get  
            {  
                return this.FOSSIL_IDValue;  
            }  

          set { SetProperty(ref  FOSSIL_IDValue, value); }
        } 
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
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
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
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
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
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
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
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


    public FOSSIL_DOCUMENT () { }

  }
}


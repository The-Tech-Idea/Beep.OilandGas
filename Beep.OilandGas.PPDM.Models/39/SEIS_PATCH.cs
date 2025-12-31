using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_PATCH: Entity,IPPDMEntity

{

private  System.String PATCH_IDValue; 
 public System.String PATCH_ID
        {  
            get  
            {  
                return this.PATCH_IDValue;  
            }  

          set { SetProperty(ref  PATCH_IDValue, value); }
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
private  System.Decimal CHANNEL_COUNTValue; 
 public System.Decimal CHANNEL_COUNT
        {  
            get  
            {  
                return this.CHANNEL_COUNTValue;  
            }  

          set { SetProperty(ref  CHANNEL_COUNTValue, value); }
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
private  System.Decimal GAP_COUNTValue; 
 public System.Decimal GAP_COUNT
        {  
            get  
            {  
                return this.GAP_COUNTValue;  
            }  

          set { SetProperty(ref  GAP_COUNTValue, value); }
        } 
private  System.String PATCH_LAYOUTValue; 
 public System.String PATCH_LAYOUT
        {  
            get  
            {  
                return this.PATCH_LAYOUTValue;  
            }  

          set { SetProperty(ref  PATCH_LAYOUTValue, value); }
        } 
private  System.String PATCH_TYPEValue; 
 public System.String PATCH_TYPE
        {  
            get  
            {  
                return this.PATCH_TYPEValue;  
            }  

          set { SetProperty(ref  PATCH_TYPEValue, value); }
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
private  System.String ROLL_ALONG_METHODValue; 
 public System.String ROLL_ALONG_METHOD
        {  
            get  
            {  
                return this.ROLL_ALONG_METHODValue;  
            }  

          set { SetProperty(ref  ROLL_ALONG_METHODValue, value); }
        } 
private  System.String SHOT_GAP_INDValue; 
 public System.String SHOT_GAP_IND
        {  
            get  
            {  
                return this.SHOT_GAP_INDValue;  
            }  

          set { SetProperty(ref  SHOT_GAP_INDValue, value); }
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
private  System.String SYMMETRIC_INDValue; 
 public System.String SYMMETRIC_IND
        {  
            get  
            {  
                return this.SYMMETRIC_INDValue;  
            }  

          set { SetProperty(ref  SYMMETRIC_INDValue, value); }
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


    public SEIS_PATCH () { }

  }
}


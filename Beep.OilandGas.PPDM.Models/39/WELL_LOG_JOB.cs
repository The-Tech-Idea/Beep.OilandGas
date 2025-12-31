using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_LOG_JOB: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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
private  System.String JOB_IDValue; 
 public System.String JOB_ID
        {  
            get  
            {  
                return this.JOB_IDValue;  
            }  

          set { SetProperty(ref  JOB_IDValue, value); }
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
private  System.String AREA_IDValue; 
 public System.String AREA_ID
        {  
            get  
            {  
                return this.AREA_IDValue;  
            }  

          set { SetProperty(ref  AREA_IDValue, value); }
        } 
private  System.String AREA_TYPEValue; 
 public System.String AREA_TYPE
        {  
            get  
            {  
                return this.AREA_TYPEValue;  
            }  

          set { SetProperty(ref  AREA_TYPEValue, value); }
        } 
private  System.Decimal CASING_SHOE_DEPTHValue; 
 public System.Decimal CASING_SHOE_DEPTH
        {  
            get  
            {  
                return this.CASING_SHOE_DEPTHValue;  
            }  

          set { SetProperty(ref  CASING_SHOE_DEPTHValue, value); }
        } 
private  System.String CASING_SHOE_DEPTH_OUOMValue; 
 public System.String CASING_SHOE_DEPTH_OUOM
        {  
            get  
            {  
                return this.CASING_SHOE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  CASING_SHOE_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal DRILLING_MDValue; 
 public System.Decimal DRILLING_MD
        {  
            get  
            {  
                return this.DRILLING_MDValue;  
            }  

          set { SetProperty(ref  DRILLING_MDValue, value); }
        } 
private  System.String DRILLING_MD_OUOMValue; 
 public System.String DRILLING_MD_OUOM
        {  
            get  
            {  
                return this.DRILLING_MD_OUOMValue;  
            }  

          set { SetProperty(ref  DRILLING_MD_OUOMValue, value); }
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
private  System.DateTime? END_DATEValue; 
 public System.DateTime? END_DATE
        {  
            get  
            {  
                return this.END_DATEValue;  
            }  

          set { SetProperty(ref  END_DATEValue, value); }
        } 
private  System.String ENGINEERValue; 
 public System.String ENGINEER
        {  
            get  
            {  
                return this.ENGINEERValue;  
            }  

          set { SetProperty(ref  ENGINEERValue, value); }
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
private  System.String LOGGING_COMPANYValue; 
 public System.String LOGGING_COMPANY
        {  
            get  
            {  
                return this.LOGGING_COMPANYValue;  
            }  

          set { SetProperty(ref  LOGGING_COMPANYValue, value); }
        } 
private  System.String LOGGING_UNITValue; 
 public System.String LOGGING_UNIT
        {  
            get  
            {  
                return this.LOGGING_UNITValue;  
            }  

          set { SetProperty(ref  LOGGING_UNITValue, value); }
        } 
private  System.String OBSERVERValue; 
 public System.String OBSERVER
        {  
            get  
            {  
                return this.OBSERVERValue;  
            }  

          set { SetProperty(ref  OBSERVERValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
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


    public WELL_LOG_JOB () { }

  }
}


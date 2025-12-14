using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_LOG: Entity

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
private  System.String WELL_LOG_IDValue; 
 public System.String WELL_LOG_ID
        {  
            get  
            {  
                return this.WELL_LOG_IDValue;  
            }  

          set { SetProperty(ref  WELL_LOG_IDValue, value); }
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
private  System.String ACQUIRED_FOR_BA_IDValue; 
 public System.String ACQUIRED_FOR_BA_ID
        {  
            get  
            {  
                return this.ACQUIRED_FOR_BA_IDValue;  
            }  

          set { SetProperty(ref  ACQUIRED_FOR_BA_IDValue, value); }
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
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String BYPASS_INDValue; 
 public System.String BYPASS_IND
        {  
            get  
            {  
                return this.BYPASS_INDValue;  
            }  

          set { SetProperty(ref  BYPASS_INDValue, value); }
        } 
private  System.String CASED_HOLE_INDValue; 
 public System.String CASED_HOLE_IND
        {  
            get  
            {  
                return this.CASED_HOLE_INDValue;  
            }  

          set { SetProperty(ref  CASED_HOLE_INDValue, value); }
        } 
private  System.String COMPOSITE_INDValue; 
 public System.String COMPOSITE_IND
        {  
            get  
            {  
                return this.COMPOSITE_INDValue;  
            }  

          set { SetProperty(ref  COMPOSITE_INDValue, value); }
        } 
private  System.String DEPTH_TYPEValue; 
 public System.String DEPTH_TYPE
        {  
            get  
            {  
                return this.DEPTH_TYPEValue;  
            }  

          set { SetProperty(ref  DEPTH_TYPEValue, value); }
        } 
private  System.String DICTIONARY_IDValue; 
 public System.String DICTIONARY_ID
        {  
            get  
            {  
                return this.DICTIONARY_IDValue;  
            }  

          set { SetProperty(ref  DICTIONARY_IDValue, value); }
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
private  System.String LOG_JOB_IDValue; 
 public System.String LOG_JOB_ID
        {  
            get  
            {  
                return this.LOG_JOB_IDValue;  
            }  

          set { SetProperty(ref  LOG_JOB_IDValue, value); }
        } 
private  System.String LOG_JOB_SOURCEValue; 
 public System.String LOG_JOB_SOURCE
        {  
            get  
            {  
                return this.LOG_JOB_SOURCEValue;  
            }  

          set { SetProperty(ref  LOG_JOB_SOURCEValue, value); }
        } 
private  System.String LOG_REF_NUMValue; 
 public System.String LOG_REF_NUM
        {  
            get  
            {  
                return this.LOG_REF_NUMValue;  
            }  

          set { SetProperty(ref  LOG_REF_NUMValue, value); }
        } 
private  System.String LOG_TITLEValue; 
 public System.String LOG_TITLE
        {  
            get  
            {  
                return this.LOG_TITLEValue;  
            }  

          set { SetProperty(ref  LOG_TITLEValue, value); }
        } 
private  System.Decimal LOG_TOOL_PASS_NOValue; 
 public System.Decimal LOG_TOOL_PASS_NO
        {  
            get  
            {  
                return this.LOG_TOOL_PASS_NOValue;  
            }  

          set { SetProperty(ref  LOG_TOOL_PASS_NOValue, value); }
        } 
private  System.String MWD_INDValue; 
 public System.String MWD_IND
        {  
            get  
            {  
                return this.MWD_INDValue;  
            }  

          set { SetProperty(ref  MWD_INDValue, value); }
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
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal TRIP_OBS_NOValue; 
 public System.Decimal TRIP_OBS_NO
        {  
            get  
            {  
                return this.TRIP_OBS_NOValue;  
            }  

          set { SetProperty(ref  TRIP_OBS_NOValue, value); }
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


    public WELL_LOG () { }

  }
}


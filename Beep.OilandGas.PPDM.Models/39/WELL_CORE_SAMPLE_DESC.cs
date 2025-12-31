using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_CORE_SAMPLE_DESC: Entity,IPPDMEntity

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
private  System.String CORE_IDValue; 
 public System.String CORE_ID
        {  
            get  
            {  
                return this.CORE_IDValue;  
            }  

          set { SetProperty(ref  CORE_IDValue, value); }
        } 
private  System.Decimal ANALYSIS_OBS_NOValue; 
 public System.Decimal ANALYSIS_OBS_NO
        {  
            get  
            {  
                return this.ANALYSIS_OBS_NOValue;  
            }  

          set { SetProperty(ref  ANALYSIS_OBS_NOValue, value); }
        } 
private  System.String SAMPLE_NUMValue; 
 public System.String SAMPLE_NUM
        {  
            get  
            {  
                return this.SAMPLE_NUMValue;  
            }  

          set { SetProperty(ref  SAMPLE_NUMValue, value); }
        } 
private  System.Decimal SAMPLE_ANALYSIS_OBS_NOValue; 
 public System.Decimal SAMPLE_ANALYSIS_OBS_NO
        {  
            get  
            {  
                return this.SAMPLE_ANALYSIS_OBS_NOValue;  
            }  

          set { SetProperty(ref  SAMPLE_ANALYSIS_OBS_NOValue, value); }
        } 
private  System.Decimal SAMPLE_DESC_OBS_NOValue; 
 public System.Decimal SAMPLE_DESC_OBS_NO
        {  
            get  
            {  
                return this.SAMPLE_DESC_OBS_NOValue;  
            }  

          set { SetProperty(ref  SAMPLE_DESC_OBS_NOValue, value); }
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
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
        } 
private  System.String DESC_SOURCEValue; 
 public System.String DESC_SOURCE
        {  
            get  
            {  
                return this.DESC_SOURCEValue;  
            }  

          set { SetProperty(ref  DESC_SOURCEValue, value); }
        } 
private  System.Decimal DIP_ANGLEValue; 
 public System.Decimal DIP_ANGLE
        {  
            get  
            {  
                return this.DIP_ANGLEValue;  
            }  

          set { SetProperty(ref  DIP_ANGLEValue, value); }
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
private  System.String LITHOLOGY_DESCValue; 
 public System.String LITHOLOGY_DESC
        {  
            get  
            {  
                return this.LITHOLOGY_DESCValue;  
            }  

          set { SetProperty(ref  LITHOLOGY_DESCValue, value); }
        } 
private  System.Decimal POROSITY_LENGTHValue; 
 public System.Decimal POROSITY_LENGTH
        {  
            get  
            {  
                return this.POROSITY_LENGTHValue;  
            }  

          set { SetProperty(ref  POROSITY_LENGTHValue, value); }
        } 
private  System.String POROSITY_LENGTH_OUOMValue; 
 public System.String POROSITY_LENGTH_OUOM
        {  
            get  
            {  
                return this.POROSITY_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  POROSITY_LENGTH_OUOMValue, value); }
        } 
private  System.String POROSITY_QUALITYValue; 
 public System.String POROSITY_QUALITY
        {  
            get  
            {  
                return this.POROSITY_QUALITYValue;  
            }  

          set { SetProperty(ref  POROSITY_QUALITYValue, value); }
        } 
private  System.String POROSITY_TYPEValue; 
 public System.String POROSITY_TYPE
        {  
            get  
            {  
                return this.POROSITY_TYPEValue;  
            }  

          set { SetProperty(ref  POROSITY_TYPEValue, value); }
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
private  System.Decimal RECOVERED_AMOUNTValue; 
 public System.Decimal RECOVERED_AMOUNT
        {  
            get  
            {  
                return this.RECOVERED_AMOUNTValue;  
            }  

          set { SetProperty(ref  RECOVERED_AMOUNTValue, value); }
        } 
private  System.String RECOVERED_AMOUNT_OUOMValue; 
 public System.String RECOVERED_AMOUNT_OUOM
        {  
            get  
            {  
                return this.RECOVERED_AMOUNT_OUOMValue;  
            }  

          set { SetProperty(ref  RECOVERED_AMOUNT_OUOMValue, value); }
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
private  System.String SAMPLE_TYPEValue; 
 public System.String SAMPLE_TYPE
        {  
            get  
            {  
                return this.SAMPLE_TYPEValue;  
            }  

          set { SetProperty(ref  SAMPLE_TYPEValue, value); }
        } 
private  System.Decimal SHOW_LENGTHValue; 
 public System.Decimal SHOW_LENGTH
        {  
            get  
            {  
                return this.SHOW_LENGTHValue;  
            }  

          set { SetProperty(ref  SHOW_LENGTHValue, value); }
        } 
private  System.String SHOW_LENGTH_OUOMValue; 
 public System.String SHOW_LENGTH_OUOM
        {  
            get  
            {  
                return this.SHOW_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  SHOW_LENGTH_OUOMValue, value); }
        } 
private  System.String SHOW_QUALITYValue; 
 public System.String SHOW_QUALITY
        {  
            get  
            {  
                return this.SHOW_QUALITYValue;  
            }  

          set { SetProperty(ref  SHOW_QUALITYValue, value); }
        } 
private  System.String SHOW_TYPEValue; 
 public System.String SHOW_TYPE
        {  
            get  
            {  
                return this.SHOW_TYPEValue;  
            }  

          set { SetProperty(ref  SHOW_TYPEValue, value); }
        } 
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String STRAT_UNIT_IDValue; 
 public System.String STRAT_UNIT_ID
        {  
            get  
            {  
                return this.STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  STRAT_UNIT_IDValue, value); }
        } 
private  System.String SWC_RECOVERY_TYPEValue; 
 public System.String SWC_RECOVERY_TYPE
        {  
            get  
            {  
                return this.SWC_RECOVERY_TYPEValue;  
            }  

          set { SetProperty(ref  SWC_RECOVERY_TYPEValue, value); }
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


    public WELL_CORE_SAMPLE_DESC () { }

  }
}


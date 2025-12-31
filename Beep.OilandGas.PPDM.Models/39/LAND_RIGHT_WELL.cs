using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class LAND_RIGHT_WELL: Entity,IPPDMEntity

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
private  System.String LAND_RIGHT_SUBTYPEValue; 
 public System.String LAND_RIGHT_SUBTYPE
        {  
            get  
            {  
                return this.LAND_RIGHT_SUBTYPEValue;  
            }  

          set { SetProperty(ref  LAND_RIGHT_SUBTYPEValue, value); }
        } 
private  System.String LAND_RIGHT_IDValue; 
 public System.String LAND_RIGHT_ID
        {  
            get  
            {  
                return this.LAND_RIGHT_IDValue;  
            }  

          set { SetProperty(ref  LAND_RIGHT_IDValue, value); }
        } 
private  System.Decimal LR_WELL_SEQ_NOValue; 
 public System.Decimal LR_WELL_SEQ_NO
        {  
            get  
            {  
                return this.LR_WELL_SEQ_NOValue;  
            }  

          set { SetProperty(ref  LR_WELL_SEQ_NOValue, value); }
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
private  System.Decimal COMPLETION_OBS_NOValue; 
 public System.Decimal COMPLETION_OBS_NO
        {  
            get  
            {  
                return this.COMPLETION_OBS_NOValue;  
            }  

          set { SetProperty(ref  COMPLETION_OBS_NOValue, value); }
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
private  System.Decimal GAS_PERCENT_PSUValue; 
 public System.Decimal GAS_PERCENT_PSU
        {  
            get  
            {  
                return this.GAS_PERCENT_PSUValue;  
            }  

          set { SetProperty(ref  GAS_PERCENT_PSUValue, value); }
        } 
private  System.String KEY_WELL_INDValue; 
 public System.String KEY_WELL_IND
        {  
            get  
            {  
                return this.KEY_WELL_INDValue;  
            }  

          set { SetProperty(ref  KEY_WELL_INDValue, value); }
        } 
private  System.Decimal OIL_PERCENT_PSUValue; 
 public System.Decimal OIL_PERCENT_PSU
        {  
            get  
            {  
                return this.OIL_PERCENT_PSUValue;  
            }  

          set { SetProperty(ref  OIL_PERCENT_PSUValue, value); }
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
private  System.Decimal PR_STR_FORM_OBS_NOValue; 
 public System.Decimal PR_STR_FORM_OBS_NO
        {  
            get  
            {  
                return this.PR_STR_FORM_OBS_NOValue;  
            }  

          set { SetProperty(ref  PR_STR_FORM_OBS_NOValue, value); }
        } 
private  System.String PR_STR_SOURCEValue; 
 public System.String PR_STR_SOURCE
        {  
            get  
            {  
                return this.PR_STR_SOURCEValue;  
            }  

          set { SetProperty(ref  PR_STR_SOURCEValue, value); }
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
private  System.String SPACING_COMPLETE_INDValue; 
 public System.String SPACING_COMPLETE_IND
        {  
            get  
            {  
                return this.SPACING_COMPLETE_INDValue;  
            }  

          set { SetProperty(ref  SPACING_COMPLETE_INDValue, value); }
        } 
private  System.String SPACING_UNIT_IDValue; 
 public System.String SPACING_UNIT_ID
        {  
            get  
            {  
                return this.SPACING_UNIT_IDValue;  
            }  

          set { SetProperty(ref  SPACING_UNIT_IDValue, value); }
        } 
private  System.String STRING_IDValue; 
 public System.String STRING_ID
        {  
            get  
            {  
                return this.STRING_IDValue;  
            }  

          set { SetProperty(ref  STRING_IDValue, value); }
        } 
private  System.String WELL_IN_TRACT_INDValue; 
 public System.String WELL_IN_TRACT_IND
        {  
            get  
            {  
                return this.WELL_IN_TRACT_INDValue;  
            }  

          set { SetProperty(ref  WELL_IN_TRACT_INDValue, value); }
        } 
private  System.String WELL_RELATIONSHIP_TYPEValue; 
 public System.String WELL_RELATIONSHIP_TYPE
        {  
            get  
            {  
                return this.WELL_RELATIONSHIP_TYPEValue;  
            }  

          set { SetProperty(ref  WELL_RELATIONSHIP_TYPEValue, value); }
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


    public LAND_RIGHT_WELL () { }

  }
}


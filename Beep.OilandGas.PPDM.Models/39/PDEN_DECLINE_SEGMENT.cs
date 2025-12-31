using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PDEN_DECLINE_SEGMENT: Entity,IPPDMEntity

{

private  System.String PDEN_SUBTYPEValue; 
 public System.String PDEN_SUBTYPE
        {  
            get  
            {  
                return this.PDEN_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PDEN_SUBTYPEValue, value); }
        } 
private  System.String PDEN_IDValue; 
 public System.String PDEN_ID
        {  
            get  
            {  
                return this.PDEN_IDValue;  
            }  

          set { SetProperty(ref  PDEN_IDValue, value); }
        } 
private  System.String PDEN_SOURCEValue; 
 public System.String PDEN_SOURCE
        {  
            get  
            {  
                return this.PDEN_SOURCEValue;  
            }  

          set { SetProperty(ref  PDEN_SOURCEValue, value); }
        } 
private  System.String CASE_IDValue; 
 public System.String CASE_ID
        {  
            get  
            {  
                return this.CASE_IDValue;  
            }  

          set { SetProperty(ref  CASE_IDValue, value); }
        } 
private  System.String SEGMENT_IDValue; 
 public System.String SEGMENT_ID
        {  
            get  
            {  
                return this.SEGMENT_IDValue;  
            }  

          set { SetProperty(ref  SEGMENT_IDValue, value); }
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
private  System.String DECLINE_CURVE_TYPEValue; 
 public System.String DECLINE_CURVE_TYPE
        {  
            get  
            {  
                return this.DECLINE_CURVE_TYPEValue;  
            }  

          set { SetProperty(ref  DECLINE_CURVE_TYPEValue, value); }
        } 
private  System.String DECLINE_TYPEValue; 
 public System.String DECLINE_TYPE
        {  
            get  
            {  
                return this.DECLINE_TYPEValue;  
            }  

          set { SetProperty(ref  DECLINE_TYPEValue, value); }
        } 
private  System.Decimal DURATIONValue; 
 public System.Decimal DURATION
        {  
            get  
            {  
                return this.DURATIONValue;  
            }  

          set { SetProperty(ref  DURATIONValue, value); }
        } 
private  System.String DURATION_OUOMValue; 
 public System.String DURATION_OUOM
        {  
            get  
            {  
                return this.DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  DURATION_OUOMValue, value); }
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
private  System.DateTime? EXPIRY_DATEValue; 
 public System.DateTime? EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.Decimal FINAL_DECLINEValue; 
 public System.Decimal FINAL_DECLINE
        {  
            get  
            {  
                return this.FINAL_DECLINEValue;  
            }  

          set { SetProperty(ref  FINAL_DECLINEValue, value); }
        } 
private  System.Decimal FINAL_RATEValue; 
 public System.Decimal FINAL_RATE
        {  
            get  
            {  
                return this.FINAL_RATEValue;  
            }  

          set { SetProperty(ref  FINAL_RATEValue, value); }
        } 
private  System.Decimal INITIAL_DECLINEValue; 
 public System.Decimal INITIAL_DECLINE
        {  
            get  
            {  
                return this.INITIAL_DECLINEValue;  
            }  

          set { SetProperty(ref  INITIAL_DECLINEValue, value); }
        } 
private  System.Decimal INITIAL_RATEValue; 
 public System.Decimal INITIAL_RATE
        {  
            get  
            {  
                return this.INITIAL_RATEValue;  
            }  

          set { SetProperty(ref  INITIAL_RATEValue, value); }
        } 
private  System.Decimal MINIMUM_DECLINEValue; 
 public System.Decimal MINIMUM_DECLINE
        {  
            get  
            {  
                return this.MINIMUM_DECLINEValue;  
            }  

          set { SetProperty(ref  MINIMUM_DECLINEValue, value); }
        } 
private  System.Decimal N_FACTORValue; 
 public System.Decimal N_FACTOR
        {  
            get  
            {  
                return this.N_FACTORValue;  
            }  

          set { SetProperty(ref  N_FACTORValue, value); }
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
private  System.String PRODUCT_TYPEValue; 
 public System.String PRODUCT_TYPE
        {  
            get  
            {  
                return this.PRODUCT_TYPEValue;  
            }  

          set { SetProperty(ref  PRODUCT_TYPEValue, value); }
        } 
private  System.String RATE_OUOMValue; 
 public System.String RATE_OUOM
        {  
            get  
            {  
                return this.RATE_OUOMValue;  
            }  

          set { SetProperty(ref  RATE_OUOMValue, value); }
        } 
private  System.String RATIO_CURVE_TYPEValue; 
 public System.String RATIO_CURVE_TYPE
        {  
            get  
            {  
                return this.RATIO_CURVE_TYPEValue;  
            }  

          set { SetProperty(ref  RATIO_CURVE_TYPEValue, value); }
        } 
private  System.Decimal RATIO_FINAL_RATEValue; 
 public System.Decimal RATIO_FINAL_RATE
        {  
            get  
            {  
                return this.RATIO_FINAL_RATEValue;  
            }  

          set { SetProperty(ref  RATIO_FINAL_RATEValue, value); }
        } 
private  System.String RATIO_FLUID_TYPEValue; 
 public System.String RATIO_FLUID_TYPE
        {  
            get  
            {  
                return this.RATIO_FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  RATIO_FLUID_TYPEValue, value); }
        } 
private  System.Decimal RATIO_INITIAL_RATEValue; 
 public System.Decimal RATIO_INITIAL_RATE
        {  
            get  
            {  
                return this.RATIO_INITIAL_RATEValue;  
            }  

          set { SetProperty(ref  RATIO_INITIAL_RATEValue, value); }
        } 
private  System.String RATIO_RATE_OUOMValue; 
 public System.String RATIO_RATE_OUOM
        {  
            get  
            {  
                return this.RATIO_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  RATIO_RATE_OUOMValue, value); }
        } 
private  System.Decimal RATIO_VOLUMEValue; 
 public System.Decimal RATIO_VOLUME
        {  
            get  
            {  
                return this.RATIO_VOLUMEValue;  
            }  

          set { SetProperty(ref  RATIO_VOLUMEValue, value); }
        } 
private  System.String RATIO_VOLUME_OUOMValue; 
 public System.String RATIO_VOLUME_OUOM
        {  
            get  
            {  
                return this.RATIO_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  RATIO_VOLUME_OUOMValue, value); }
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
private  System.DateTime? START_DATEValue; 
 public System.DateTime? START_DATE
        {  
            get  
            {  
                return this.START_DATEValue;  
            }  

          set { SetProperty(ref  START_DATEValue, value); }
        } 
private  System.Decimal VOLUMEValue; 
 public System.Decimal VOLUME
        {  
            get  
            {  
                return this.VOLUMEValue;  
            }  

          set { SetProperty(ref  VOLUMEValue, value); }
        } 
private  System.String VOLUME_OUOMValue; 
 public System.String VOLUME_OUOM
        {  
            get  
            {  
                return this.VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  VOLUME_OUOMValue, value); }
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


    public PDEN_DECLINE_SEGMENT () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PDEN_MATERIAL_BAL: Entity,IPPDMEntity

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
private  System.String PRODUCT_TYPEValue; 
 public System.String PRODUCT_TYPE
        {  
            get  
            {  
                return this.PRODUCT_TYPEValue;  
            }  

          set { SetProperty(ref  PRODUCT_TYPEValue, value); }
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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
        } 
private  System.String CASE_NAMEValue; 
 public System.String CASE_NAME
        {  
            get  
            {  
                return this.CASE_NAMEValue;  
            }  

          set { SetProperty(ref  CASE_NAMEValue, value); }
        } 
private  System.String CLOSE_MONTHValue; 
 public System.String CLOSE_MONTH
        {  
            get  
            {  
                return this.CLOSE_MONTHValue;  
            }  

          set { SetProperty(ref  CLOSE_MONTHValue, value); }
        } 
private  System.Decimal CLOSE_YEARValue; 
 public System.Decimal CLOSE_YEAR
        {  
            get  
            {  
                return this.CLOSE_YEARValue;  
            }  

          set { SetProperty(ref  CLOSE_YEARValue, value); }
        } 
private  System.Decimal CO2_PERCENTValue; 
 public System.Decimal CO2_PERCENT
        {  
            get  
            {  
                return this.CO2_PERCENTValue;  
            }  

          set { SetProperty(ref  CO2_PERCENTValue, value); }
        } 
private  System.Decimal CRITICAL_PRESSValue; 
 public System.Decimal CRITICAL_PRESS
        {  
            get  
            {  
                return this.CRITICAL_PRESSValue;  
            }  

          set { SetProperty(ref  CRITICAL_PRESSValue, value); }
        } 
private  System.String CRITICAL_PRESS_OUOMValue; 
 public System.String CRITICAL_PRESS_OUOM
        {  
            get  
            {  
                return this.CRITICAL_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  CRITICAL_PRESS_OUOMValue, value); }
        } 
private  System.Decimal CRITICAL_TEMPValue; 
 public System.Decimal CRITICAL_TEMP
        {  
            get  
            {  
                return this.CRITICAL_TEMPValue;  
            }  

          set { SetProperty(ref  CRITICAL_TEMPValue, value); }
        } 
private  System.String CRITICAL_TEMP_OUOMValue; 
 public System.String CRITICAL_TEMP_OUOM
        {  
            get  
            {  
                return this.CRITICAL_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  CRITICAL_TEMP_OUOMValue, value); }
        } 
private  System.Decimal CUM_VOLUMEValue; 
 public System.Decimal CUM_VOLUME
        {  
            get  
            {  
                return this.CUM_VOLUMEValue;  
            }  

          set { SetProperty(ref  CUM_VOLUMEValue, value); }
        } 
private  System.DateTime? CUM_VOLUME_DATEValue; 
 public System.DateTime? CUM_VOLUME_DATE
        {  
            get  
            {  
                return this.CUM_VOLUME_DATEValue;  
            }  

          set { SetProperty(ref  CUM_VOLUME_DATEValue, value); }
        } 
private  System.String CUM_VOLUME_DATE_DESCValue; 
 public System.String CUM_VOLUME_DATE_DESC
        {  
            get  
            {  
                return this.CUM_VOLUME_DATE_DESCValue;  
            }  

          set { SetProperty(ref  CUM_VOLUME_DATE_DESCValue, value); }
        } 
private  System.String CUM_VOLUME_OUOMValue; 
 public System.String CUM_VOLUME_OUOM
        {  
            get  
            {  
                return this.CUM_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  CUM_VOLUME_OUOMValue, value); }
        } 
private  System.String CUM_VOLUME_UOMValue; 
 public System.String CUM_VOLUME_UOM
        {  
            get  
            {  
                return this.CUM_VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  CUM_VOLUME_UOMValue, value); }
        } 
private  System.Decimal CURVE_FIT_ERRORValue; 
 public System.Decimal CURVE_FIT_ERROR
        {  
            get  
            {  
                return this.CURVE_FIT_ERRORValue;  
            }  

          set { SetProperty(ref  CURVE_FIT_ERRORValue, value); }
        } 
private  System.String CURVE_FIT_TYPEValue; 
 public System.String CURVE_FIT_TYPE
        {  
            get  
            {  
                return this.CURVE_FIT_TYPEValue;  
            }  

          set { SetProperty(ref  CURVE_FIT_TYPEValue, value); }
        } 
private  System.Decimal CURVE_INTERCEPTValue; 
 public System.Decimal CURVE_INTERCEPT
        {  
            get  
            {  
                return this.CURVE_INTERCEPTValue;  
            }  

          set { SetProperty(ref  CURVE_INTERCEPTValue, value); }
        } 
private  System.String CURVE_NAMEValue; 
 public System.String CURVE_NAME
        {  
            get  
            {  
                return this.CURVE_NAMEValue;  
            }  

          set { SetProperty(ref  CURVE_NAMEValue, value); }
        } 
private  System.Decimal CURVE_SLOPEValue; 
 public System.Decimal CURVE_SLOPE
        {  
            get  
            {  
                return this.CURVE_SLOPEValue;  
            }  

          set { SetProperty(ref  CURVE_SLOPEValue, value); }
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
private  System.Decimal FINAL_CUM_VOLUMEValue; 
 public System.Decimal FINAL_CUM_VOLUME
        {  
            get  
            {  
                return this.FINAL_CUM_VOLUMEValue;  
            }  

          set { SetProperty(ref  FINAL_CUM_VOLUMEValue, value); }
        } 
private  System.String FINAL_CUM_VOLUME_OUOMValue; 
 public System.String FINAL_CUM_VOLUME_OUOM
        {  
            get  
            {  
                return this.FINAL_CUM_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_CUM_VOLUME_OUOMValue, value); }
        } 
private  System.String FINAL_CUM_VOLUME_UOMValue; 
 public System.String FINAL_CUM_VOLUME_UOM
        {  
            get  
            {  
                return this.FINAL_CUM_VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  FINAL_CUM_VOLUME_UOMValue, value); }
        } 
private  System.Decimal FINAL_PRESSValue; 
 public System.Decimal FINAL_PRESS
        {  
            get  
            {  
                return this.FINAL_PRESSValue;  
            }  

          set { SetProperty(ref  FINAL_PRESSValue, value); }
        } 
private  System.String FINAL_PRESS_OUOMValue; 
 public System.String FINAL_PRESS_OUOM
        {  
            get  
            {  
                return this.FINAL_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  FINAL_PRESS_OUOMValue, value); }
        } 
private  System.Decimal GAS_ABANDON_PRESSValue; 
 public System.Decimal GAS_ABANDON_PRESS
        {  
            get  
            {  
                return this.GAS_ABANDON_PRESSValue;  
            }  

          set { SetProperty(ref  GAS_ABANDON_PRESSValue, value); }
        } 
private  System.String GAS_ABANDON_PRESS_OUOMValue; 
 public System.String GAS_ABANDON_PRESS_OUOM
        {  
            get  
            {  
                return this.GAS_ABANDON_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_ABANDON_PRESS_OUOMValue, value); }
        } 
private  System.Decimal GAS_ABANDON_RECOVERValue; 
 public System.Decimal GAS_ABANDON_RECOVER
        {  
            get  
            {  
                return this.GAS_ABANDON_RECOVERValue;  
            }  

          set { SetProperty(ref  GAS_ABANDON_RECOVERValue, value); }
        } 
private  System.Decimal H2S_PERCENTValue; 
 public System.Decimal H2S_PERCENT
        {  
            get  
            {  
                return this.H2S_PERCENTValue;  
            }  

          set { SetProperty(ref  H2S_PERCENTValue, value); }
        } 
private  System.Decimal INITIAL_CUM_VOLUMEValue; 
 public System.Decimal INITIAL_CUM_VOLUME
        {  
            get  
            {  
                return this.INITIAL_CUM_VOLUMEValue;  
            }  

          set { SetProperty(ref  INITIAL_CUM_VOLUMEValue, value); }
        } 
private  System.String INITIAL_CUM_VOLUME_OUOMValue; 
 public System.String INITIAL_CUM_VOLUME_OUOM
        {  
            get  
            {  
                return this.INITIAL_CUM_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  INITIAL_CUM_VOLUME_OUOMValue, value); }
        } 
private  System.String INITIAL_CUM_VOLUME_UOMValue; 
 public System.String INITIAL_CUM_VOLUME_UOM
        {  
            get  
            {  
                return this.INITIAL_CUM_VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  INITIAL_CUM_VOLUME_UOMValue, value); }
        } 
private  System.Decimal INITIAL_PRESSValue; 
 public System.Decimal INITIAL_PRESS
        {  
            get  
            {  
                return this.INITIAL_PRESSValue;  
            }  

          set { SetProperty(ref  INITIAL_PRESSValue, value); }
        } 
private  System.String INITIAL_PRESS_OUOMValue; 
 public System.String INITIAL_PRESS_OUOM
        {  
            get  
            {  
                return this.INITIAL_PRESS_OUOMValue;  
            }  

          set { SetProperty(ref  INITIAL_PRESS_OUOMValue, value); }
        } 
private  System.Decimal INITIAL_TEMPValue; 
 public System.Decimal INITIAL_TEMP
        {  
            get  
            {  
                return this.INITIAL_TEMPValue;  
            }  

          set { SetProperty(ref  INITIAL_TEMPValue, value); }
        } 
private  System.String INITIAL_TEMP_OUOMValue; 
 public System.String INITIAL_TEMP_OUOM
        {  
            get  
            {  
                return this.INITIAL_TEMP_OUOMValue;  
            }  

          set { SetProperty(ref  INITIAL_TEMP_OUOMValue, value); }
        } 
private  System.String MEASURED_PRESS_INDValue; 
 public System.String MEASURED_PRESS_IND
        {  
            get  
            {  
                return this.MEASURED_PRESS_INDValue;  
            }  

          set { SetProperty(ref  MEASURED_PRESS_INDValue, value); }
        } 
private  System.Decimal N2_PERCENTValue; 
 public System.Decimal N2_PERCENT
        {  
            get  
            {  
                return this.N2_PERCENTValue;  
            }  

          set { SetProperty(ref  N2_PERCENTValue, value); }
        } 
private  System.Decimal ORIG_GAS_IN_PLACEValue; 
 public System.Decimal ORIG_GAS_IN_PLACE
        {  
            get  
            {  
                return this.ORIG_GAS_IN_PLACEValue;  
            }  

          set { SetProperty(ref  ORIG_GAS_IN_PLACEValue, value); }
        } 
private  System.Decimal POOL_DATUM_DEPTHValue; 
 public System.Decimal POOL_DATUM_DEPTH
        {  
            get  
            {  
                return this.POOL_DATUM_DEPTHValue;  
            }  

          set { SetProperty(ref  POOL_DATUM_DEPTHValue, value); }
        } 
private  System.String POOL_DATUM_DEPTH_OUOMValue; 
 public System.String POOL_DATUM_DEPTH_OUOM
        {  
            get  
            {  
                return this.POOL_DATUM_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  POOL_DATUM_DEPTH_OUOMValue, value); }
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
private  System.String PROJECT_IDValue; 
 public System.String PROJECT_ID
        {  
            get  
            {  
                return this.PROJECT_IDValue;  
            }  

          set { SetProperty(ref  PROJECT_IDValue, value); }
        } 
private  System.Decimal RECOV_GAS_IN_PLACEValue; 
 public System.Decimal RECOV_GAS_IN_PLACE
        {  
            get  
            {  
                return this.RECOV_GAS_IN_PLACEValue;  
            }  

          set { SetProperty(ref  RECOV_GAS_IN_PLACEValue, value); }
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
private  System.Decimal SPECIFIC_GRAVITYValue; 
 public System.Decimal SPECIFIC_GRAVITY
        {  
            get  
            {  
                return this.SPECIFIC_GRAVITYValue;  
            }  

          set { SetProperty(ref  SPECIFIC_GRAVITYValue, value); }
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
private  System.Decimal SURFACE_LOSS_PERCENTValue; 
 public System.Decimal SURFACE_LOSS_PERCENT
        {  
            get  
            {  
                return this.SURFACE_LOSS_PERCENTValue;  
            }  

          set { SetProperty(ref  SURFACE_LOSS_PERCENTValue, value); }
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
private  System.String VOLUME_UOMValue; 
 public System.String VOLUME_UOM
        {  
            get  
            {  
                return this.VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  VOLUME_UOMValue, value); }
        } 
private  System.String ZERO_PRESS_INDValue; 
 public System.String ZERO_PRESS_IND
        {  
            get  
            {  
                return this.ZERO_PRESS_INDValue;  
            }  

          set { SetProperty(ref  ZERO_PRESS_INDValue, value); }
        } 
private  System.String Z_FACTOR_METHODValue; 
 public System.String Z_FACTOR_METHOD
        {  
            get  
            {  
                return this.Z_FACTOR_METHODValue;  
            }  

          set { SetProperty(ref  Z_FACTOR_METHODValue, value); }
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


    public PDEN_MATERIAL_BAL () { }

  }
}


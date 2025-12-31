using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PDEN_FLOW_MEASUREMENT: Entity,IPPDMEntity

{

private  System.String PDEN_IDValue; 
 public System.String PDEN_ID
        {  
            get  
            {  
                return this.PDEN_IDValue;  
            }  

          set { SetProperty(ref  PDEN_IDValue, value); }
        } 
private  System.String PDEN_SUBTYPEValue; 
 public System.String PDEN_SUBTYPE
        {  
            get  
            {  
                return this.PDEN_SUBTYPEValue;  
            }  

          set { SetProperty(ref  PDEN_SUBTYPEValue, value); }
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
private  System.Decimal AMENDMENT_SEQ_NOValue; 
 public System.Decimal AMENDMENT_SEQ_NO
        {  
            get  
            {  
                return this.AMENDMENT_SEQ_NOValue;  
            }  

          set { SetProperty(ref  AMENDMENT_SEQ_NOValue, value); }
        } 
private  System.String PERIOD_TYPEValue; 
 public System.String PERIOD_TYPE
        {  
            get  
            {  
                return this.PERIOD_TYPEValue;  
            }  

          set { SetProperty(ref  PERIOD_TYPEValue, value); }
        } 
private  System.Decimal MEASUREMENT_OBS_NOValue; 
 public System.Decimal MEASUREMENT_OBS_NO
        {  
            get  
            {  
                return this.MEASUREMENT_OBS_NOValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_OBS_NOValue, value); }
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
private  System.String AMEND_REASONValue; 
 public System.String AMEND_REASON
        {  
            get  
            {  
                return this.AMEND_REASONValue;  
            }  

          set { SetProperty(ref  AMEND_REASONValue, value); }
        } 
private  System.Decimal CASING_PRESSUREValue; 
 public System.Decimal CASING_PRESSURE
        {  
            get  
            {  
                return this.CASING_PRESSUREValue;  
            }  

          set { SetProperty(ref  CASING_PRESSUREValue, value); }
        } 
private  System.String CASING_PRESSURE_OUOMValue; 
 public System.String CASING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.CASING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  CASING_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal CHOKE_POSITIONValue; 
 public System.Decimal CHOKE_POSITION
        {  
            get  
            {  
                return this.CHOKE_POSITIONValue;  
            }  

          set { SetProperty(ref  CHOKE_POSITIONValue, value); }
        } 
private  System.Decimal CHOKE_SIZEValue; 
 public System.Decimal CHOKE_SIZE
        {  
            get  
            {  
                return this.CHOKE_SIZEValue;  
            }  

          set { SetProperty(ref  CHOKE_SIZEValue, value); }
        } 
private  System.String CHOKE_SIZE_OUOMValue; 
 public System.String CHOKE_SIZE_OUOM
        {  
            get  
            {  
                return this.CHOKE_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  CHOKE_SIZE_OUOMValue, value); }
        } 
private  System.Decimal DIFFERENTIAL_PRESSUREValue; 
 public System.Decimal DIFFERENTIAL_PRESSURE
        {  
            get  
            {  
                return this.DIFFERENTIAL_PRESSUREValue;  
            }  

          set { SetProperty(ref  DIFFERENTIAL_PRESSUREValue, value); }
        } 
private  System.String DIFF_PRESSURE_OUOMValue; 
 public System.String DIFF_PRESSURE_OUOM
        {  
            get  
            {  
                return this.DIFF_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  DIFF_PRESSURE_OUOMValue, value); }
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
private  System.Decimal FLOW_RATEValue; 
 public System.Decimal FLOW_RATE
        {  
            get  
            {  
                return this.FLOW_RATEValue;  
            }  

          set { SetProperty(ref  FLOW_RATEValue, value); }
        } 
private  System.String FLOW_RATE_OUOMValue; 
 public System.String FLOW_RATE_OUOM
        {  
            get  
            {  
                return this.FLOW_RATE_OUOMValue;  
            }  

          set { SetProperty(ref  FLOW_RATE_OUOMValue, value); }
        } 
private  System.DateTime? MEASUREMENT_DATEValue; 
 public System.DateTime? MEASUREMENT_DATE
        {  
            get  
            {  
                return this.MEASUREMENT_DATEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_DATEValue, value); }
        } 
private  System.String MEASUREMENT_DATE_DESCValue; 
 public System.String MEASUREMENT_DATE_DESC
        {  
            get  
            {  
                return this.MEASUREMENT_DATE_DESCValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_DATE_DESCValue, value); }
        } 
private  System.Decimal MEASUREMENT_TIMEValue; 
 public System.Decimal MEASUREMENT_TIME
        {  
            get  
            {  
                return this.MEASUREMENT_TIMEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TIMEValue, value); }
        } 
private  System.String MEASUREMENT_TIMEZONEValue; 
 public System.String MEASUREMENT_TIMEZONE
        {  
            get  
            {  
                return this.MEASUREMENT_TIMEZONEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TIMEZONEValue, value); }
        } 
private  System.String MEASUREMENT_TYPEValue; 
 public System.String MEASUREMENT_TYPE
        {  
            get  
            {  
                return this.MEASUREMENT_TYPEValue;  
            }  

          set { SetProperty(ref  MEASUREMENT_TYPEValue, value); }
        } 
private  System.Decimal MEAS_TEMPERATUREValue; 
 public System.Decimal MEAS_TEMPERATURE
        {  
            get  
            {  
                return this.MEAS_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  MEAS_TEMPERATUREValue, value); }
        } 
private  System.String MEAS_TEMPERATURE_OUOMValue; 
 public System.String MEAS_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.MEAS_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  MEAS_TEMPERATURE_OUOMValue, value); }
        } 
private  System.DateTime? POSTED_DATEValue; 
 public System.DateTime? POSTED_DATE
        {  
            get  
            {  
                return this.POSTED_DATEValue;  
            }  

          set { SetProperty(ref  POSTED_DATEValue, value); }
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
private  System.Decimal PRODUCTION_VOLUMEValue; 
 public System.Decimal PRODUCTION_VOLUME
        {  
            get  
            {  
                return this.PRODUCTION_VOLUMEValue;  
            }  

          set { SetProperty(ref  PRODUCTION_VOLUMEValue, value); }
        } 
private  System.String PRODUCTION_VOLUME_OUOMValue; 
 public System.String PRODUCTION_VOLUME_OUOM
        {  
            get  
            {  
                return this.PRODUCTION_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  PRODUCTION_VOLUME_OUOMValue, value); }
        } 
private  System.String PRODUCTION_VOLUME_UOMValue; 
 public System.String PRODUCTION_VOLUME_UOM
        {  
            get  
            {  
                return this.PRODUCTION_VOLUME_UOMValue;  
            }  

          set { SetProperty(ref  PRODUCTION_VOLUME_UOMValue, value); }
        } 
private  System.Decimal PROD_TIME_ELAPSEDValue; 
 public System.Decimal PROD_TIME_ELAPSED
        {  
            get  
            {  
                return this.PROD_TIME_ELAPSEDValue;  
            }  

          set { SetProperty(ref  PROD_TIME_ELAPSEDValue, value); }
        } 
private  System.String PROD_TIME_ELAPSED_OUOMValue; 
 public System.String PROD_TIME_ELAPSED_OUOM
        {  
            get  
            {  
                return this.PROD_TIME_ELAPSED_OUOMValue;  
            }  

          set { SetProperty(ref  PROD_TIME_ELAPSED_OUOMValue, value); }
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
private  System.Decimal STATIC_PRESSUREValue; 
 public System.Decimal STATIC_PRESSURE
        {  
            get  
            {  
                return this.STATIC_PRESSUREValue;  
            }  

          set { SetProperty(ref  STATIC_PRESSUREValue, value); }
        } 
private  System.String STATIC_PRESSURE_OUOMValue; 
 public System.String STATIC_PRESSURE_OUOM
        {  
            get  
            {  
                return this.STATIC_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  STATIC_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal TUBING_PRESSUREValue; 
 public System.Decimal TUBING_PRESSURE
        {  
            get  
            {  
                return this.TUBING_PRESSUREValue;  
            }  

          set { SetProperty(ref  TUBING_PRESSUREValue, value); }
        } 
private  System.String TUBING_PRESSURE_OUOMValue; 
 public System.String TUBING_PRESSURE_OUOM
        {  
            get  
            {  
                return this.TUBING_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  TUBING_PRESSURE_OUOMValue, value); }
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


    public PDEN_FLOW_MEASUREMENT () { }

  }
}


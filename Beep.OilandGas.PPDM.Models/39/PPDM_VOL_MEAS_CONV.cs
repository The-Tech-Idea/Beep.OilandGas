using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PPDM_VOL_MEAS_CONV: Entity,IPPDMEntity

{

private  System.String VOLUME_REGIME_IDValue; 
 public System.String VOLUME_REGIME_ID
        {  
            get  
            {  
                return this.VOLUME_REGIME_IDValue;  
            }  

          set { SetProperty(ref  VOLUME_REGIME_IDValue, value); }
        } 
private  System.String CONVERSION_QUANTITYValue; 
 public System.String CONVERSION_QUANTITY
        {  
            get  
            {  
                return this.CONVERSION_QUANTITYValue;  
            }  

          set { SetProperty(ref  CONVERSION_QUANTITYValue, value); }
        } 
private  System.Decimal CONVERSION_OBS_NOValue; 
 public System.Decimal CONVERSION_OBS_NO
        {  
            get  
            {  
                return this.CONVERSION_OBS_NOValue;  
            }  

          set { SetProperty(ref  CONVERSION_OBS_NOValue, value); }
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
private  System.Decimal CONVERSION_FACTORValue; 
 public System.Decimal CONVERSION_FACTOR
        {  
            get  
            {  
                return this.CONVERSION_FACTORValue;  
            }  

          set { SetProperty(ref  CONVERSION_FACTORValue, value); }
        } 
private  System.String CONVERSION_FORMULAValue; 
 public System.String CONVERSION_FORMULA
        {  
            get  
            {  
                return this.CONVERSION_FORMULAValue;  
            }  

          set { SetProperty(ref  CONVERSION_FORMULAValue, value); }
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
private  System.String FROM_UOMValue; 
 public System.String FROM_UOM
        {  
            get  
            {  
                return this.FROM_UOMValue;  
            }  

          set { SetProperty(ref  FROM_UOMValue, value); }
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
private  System.String PREFERRRED_INDValue; 
 public System.String PREFERRRED_IND
        {  
            get  
            {  
                return this.PREFERRRED_INDValue;  
            }  

          set { SetProperty(ref  PREFERRRED_INDValue, value); }
        } 
private  System.Decimal PRESSUREValue; 
 public System.Decimal PRESSURE
        {  
            get  
            {  
                return this.PRESSUREValue;  
            }  

          set { SetProperty(ref  PRESSUREValue, value); }
        } 
private  System.String PRESSURE_UOMValue; 
 public System.String PRESSURE_UOM
        {  
            get  
            {  
                return this.PRESSURE_UOMValue;  
            }  

          set { SetProperty(ref  PRESSURE_UOMValue, value); }
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
private  System.Decimal TEMPERATUREValue; 
 public System.Decimal TEMPERATURE
        {  
            get  
            {  
                return this.TEMPERATUREValue;  
            }  

          set { SetProperty(ref  TEMPERATUREValue, value); }
        } 
private  System.String TEMPERATURE_UOMValue; 
 public System.String TEMPERATURE_UOM
        {  
            get  
            {  
                return this.TEMPERATURE_UOMValue;  
            }  

          set { SetProperty(ref  TEMPERATURE_UOMValue, value); }
        } 
private  System.String TO_UOMValue; 
 public System.String TO_UOM
        {  
            get  
            {  
                return this.TO_UOMValue;  
            }  

          set { SetProperty(ref  TO_UOMValue, value); }
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


    public PPDM_VOL_MEAS_CONV () { }

  }
}


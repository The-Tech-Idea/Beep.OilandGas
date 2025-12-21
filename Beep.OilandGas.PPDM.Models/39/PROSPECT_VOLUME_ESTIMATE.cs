using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PROSPECT_VOLUME_ESTIMATE: Entity

{

private  System.String PROSPECT_IDValue; 
 public System.String PROSPECT_ID
        {  
            get  
            {  
                return this.PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROSPECT_IDValue, value); }
        } 
private  System.String ESTIMATE_IDValue; 
 public System.String ESTIMATE_ID
        {  
            get  
            {  
                return this.ESTIMATE_IDValue;  
            }  

          set { SetProperty(ref  ESTIMATE_IDValue, value); }
        } 
private  System.Decimal? OIL_VOLUME_P10Value; 
 public System.Decimal? OIL_VOLUME_P10
        {  
            get  
            {  
                return this.OIL_VOLUME_P10Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P10Value, value); }
        } 
private  System.Decimal? OIL_VOLUME_P50Value; 
 public System.Decimal? OIL_VOLUME_P50
        {  
            get  
            {  
                return this.OIL_VOLUME_P50Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P50Value, value); }
        } 
private  System.Decimal? OIL_VOLUME_P90Value; 
 public System.Decimal? OIL_VOLUME_P90
        {  
            get  
            {  
                return this.OIL_VOLUME_P90Value;  
            }  

          set { SetProperty(ref  OIL_VOLUME_P90Value, value); }
        } 
private  System.Decimal? GAS_VOLUME_P10Value; 
 public System.Decimal? GAS_VOLUME_P10
        {  
            get  
            {  
                return this.GAS_VOLUME_P10Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P10Value, value); }
        } 
private  System.Decimal? GAS_VOLUME_P50Value; 
 public System.Decimal? GAS_VOLUME_P50
        {  
            get  
            {  
                return this.GAS_VOLUME_P50Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P50Value, value); }
        } 
private  System.Decimal? GAS_VOLUME_P90Value; 
 public System.Decimal? GAS_VOLUME_P90
        {  
            get  
            {  
                return this.GAS_VOLUME_P90Value;  
            }  

          set { SetProperty(ref  GAS_VOLUME_P90Value, value); }
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
private  System.Decimal? UNRISKED_OIL_VOLUMEValue; 
 public System.Decimal? UNRISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? UNRISKED_GAS_VOLUMEValue; 
 public System.Decimal? UNRISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal? RISKED_OIL_VOLUMEValue; 
 public System.Decimal? RISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.RISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? RISKED_GAS_VOLUMEValue; 
 public System.Decimal? RISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.RISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal? RECOVERY_FACTORValue; 
 public System.Decimal? RECOVERY_FACTOR
        {  
            get  
            {  
                return this.RECOVERY_FACTORValue;  
            }  

          set { SetProperty(ref  RECOVERY_FACTORValue, value); }
        } 
private  System.Decimal? FORMATION_VOLUME_FACTORValue; 
 public System.Decimal? FORMATION_VOLUME_FACTOR
        {  
            get  
            {  
                return this.FORMATION_VOLUME_FACTORValue;  
            }  

          set { SetProperty(ref  FORMATION_VOLUME_FACTORValue, value); }
        } 
private  System.String ESTIMATE_METHODValue; 
 public System.String ESTIMATE_METHOD
        {  
            get  
            {  
                return this.ESTIMATE_METHODValue;  
            }  

          set { SetProperty(ref  ESTIMATE_METHODValue, value); }
        } 
private  System.DateTime? ESTIMATE_DATEValue; 
 public System.DateTime? ESTIMATE_DATE
        {  
            get  
            {  
                return this.ESTIMATE_DATEValue;  
            }  

          set { SetProperty(ref  ESTIMATE_DATEValue, value); }
        } 
private  System.String ESTIMATORValue; 
 public System.String ESTIMATOR
        {  
            get  
            {  
                return this.ESTIMATORValue;  
            }  

          set { SetProperty(ref  ESTIMATORValue, value); }
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
private  System.String ACTIVE_INDValue; 
 public System.String ACTIVE_IND
        {  
            get  
            {  
                return this.ACTIVE_INDValue;  
            }  

          set { SetProperty(ref  ACTIVE_INDValue, value); }
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


    public PROSPECT_VOLUME_ESTIMATE () { }

  }
}

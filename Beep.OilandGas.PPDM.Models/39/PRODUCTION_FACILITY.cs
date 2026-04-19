using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PRODUCTION_FACILITY: Entity,IPPDMEntity

{

private  System.String FACILITY_IDValue; 
 public System.String FACILITY_ID
        {  
            get  
            {  
                return this.FACILITY_IDValue;  
            }  

          set { SetProperty(ref  FACILITY_IDValue, value); }
        } 
private  System.String FACILITY_TYPEValue; 
 public System.String FACILITY_TYPE
        {  
            get  
            {  
                return this.FACILITY_TYPEValue;  
            }  

          set { SetProperty(ref  FACILITY_TYPEValue, value); }
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
private  System.String PDEN_IDValue; 
 public System.String PDEN_ID
        {  
            get  
            {  
                return this.PDEN_IDValue;  
            }  

          set { SetProperty(ref  PDEN_IDValue, value); }
        } 
private  System.String FIELD_IDValue; 
 public System.String FIELD_ID
        {  
            get  
            {  
                return this.FIELD_IDValue;  
            }  

          set { SetProperty(ref  FIELD_IDValue, value); }
        } 
private  System.DateTime? PRODUCTION_DATEValue; 
 public System.DateTime? PRODUCTION_DATE
        {  
            get  
            {  
                return this.PRODUCTION_DATEValue;  
            }  

          set { SetProperty(ref  PRODUCTION_DATEValue, value); }
        } 
private  System.Decimal? OIL_VOLUMEValue; 
 public System.Decimal? OIL_VOLUME
        {  
            get  
            {  
                return this.OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  OIL_VOLUMEValue, value); }
        } 
private  System.String OIL_VOLUME_OUOMValue; 
 public System.String OIL_VOLUME_OUOM
        {  
            get  
            {  
                return this.OIL_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal? GAS_VOLUMEValue; 
 public System.Decimal? GAS_VOLUME
        {  
            get  
            {  
                return this.GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  GAS_VOLUMEValue, value); }
        } 
private  System.String GAS_VOLUME_OUOMValue; 
 public System.String GAS_VOLUME_OUOM
        {  
            get  
            {  
                return this.GAS_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_VOLUME_OUOMValue, value); }
        } 
private  System.Decimal? WATER_VOLUMEValue; 
 public System.Decimal? WATER_VOLUME
        {  
            get  
            {  
                return this.WATER_VOLUMEValue;  
            }  

          set { SetProperty(ref  WATER_VOLUMEValue, value); }
        } 
private  System.String WATER_VOLUME_OUOMValue; 
 public System.String WATER_VOLUME_OUOM
        {  
            get  
            {  
                return this.WATER_VOLUME_OUOMValue;  
            }  

          set { SetProperty(ref  WATER_VOLUME_OUOMValue, value); }
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
private  System.DateTime? ROW_EFFECTIVE_DATEValue; 
 public System.DateTime? ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }        } 
private  System.DateTime? ROW_EXPIRY_DATEValue; 
 public System.DateTime? ROW_EXPIRY_DATE
        {  
            get  
            {  
                return this.ROW_EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EXPIRY_DATEValue, value); }        } 
private  System.String ROW_QUALITYValue; 
 public System.String ROW_QUALITY
        {  
            get  
            {  
                return this.ROW_QUALITYValue;  
            }  

          set { SetProperty(ref  ROW_QUALITYValue, value); }
        } 

    public PRODUCTION_FACILITY () { }
}
}

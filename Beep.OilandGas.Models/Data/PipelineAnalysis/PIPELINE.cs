using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
public partial class PIPELINE: Entity,IPPDMEntity

{

private  System.String PIPELINE_IDValue; 
 public System.String PIPELINE_ID
        {  
            get  
            {  
                return this.PIPELINE_IDValue;  
            }  

          set { SetProperty(ref  PIPELINE_IDValue, value); }
        } 
private  System.String PIPELINE_NAMEValue; 
 public System.String PIPELINE_NAME
        {  
            get  
            {  
                return this.PIPELINE_NAMEValue;  
            }  

          set { SetProperty(ref  PIPELINE_NAMEValue, value); }
        } 
private  System.String PIPELINE_SHORT_NAMEValue; 
 public System.String PIPELINE_SHORT_NAME
        {  
            get  
            {  
                return this.PIPELINE_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  PIPELINE_SHORT_NAMEValue, value); }
        } 
private  System.String PIPELINE_TYPEValue; 
 public System.String PIPELINE_TYPE
        {  
            get  
            {  
                return this.PIPELINE_TYPEValue;  
            }  

          set { SetProperty(ref  PIPELINE_TYPEValue, value); }
        } 
private  System.String PIPELINE_STATUSValue; 
 public System.String PIPELINE_STATUS
        {  
            get  
            {  
                return this.PIPELINE_STATUSValue;  
            }  

          set { SetProperty(ref  PIPELINE_STATUSValue, value); }
        } 
private  System.String REGULATORY_IDValue; 
 public System.String REGULATORY_ID
        {  
            get  
            {  
                return this.REGULATORY_IDValue;  
            }  

          set { SetProperty(ref  REGULATORY_IDValue, value); }
        } 
private  System.String PRIMARY_FIELD_IDValue; 
 public System.String PRIMARY_FIELD_ID
        {  
            get  
            {  
                return this.PRIMARY_FIELD_IDValue;  
            }  

          set { SetProperty(ref  PRIMARY_FIELD_IDValue, value); }
        } 
private  System.String CURRENT_OPERATORValue; 
 public System.String CURRENT_OPERATOR
        {  
            get  
            {  
                return this.CURRENT_OPERATORValue;  
            }  

          set { SetProperty(ref  CURRENT_OPERATORValue, value); }
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
private  System.DateTime? INSTALLATION_DATEValue; 
 public System.DateTime? INSTALLATION_DATE
        {  
            get  
            {  
                return this.INSTALLATION_DATEValue;  
            }  

          set { SetProperty(ref  INSTALLATION_DATEValue, value); }
        } 
private  System.DateTime? COMMISSIONING_DATEValue; 
 public System.DateTime? COMMISSIONING_DATE
        {  
            get  
            {  
                return this.COMMISSIONING_DATEValue;  
            }  

          set { SetProperty(ref  COMMISSIONING_DATEValue, value); }
        } 
private  System.DateTime? DECOMMISSIONING_DATEValue; 
 public System.DateTime? DECOMMISSIONING_DATE
        {  
            get  
            {  
                return this.DECOMMISSIONING_DATEValue;  
            }  

          set { SetProperty(ref  DECOMMISSIONING_DATEValue, value); }
        } 
private  System.DateTime? ABANDONED_DATEValue; 
 public System.DateTime? ABANDONED_DATE
        {  
            get  
            {  
                return this.ABANDONED_DATEValue;  
            }  

          set { SetProperty(ref  ABANDONED_DATEValue, value); }
        } 
private  System.DateTime? ACTIVE_DATEValue; 
 public System.DateTime? ACTIVE_DATE
        {  
            get  
            {  
                return this.ACTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ACTIVE_DATEValue, value); }
        } 
private  System.DateTime? INACTIVE_DATEValue; 
 public System.DateTime? INACTIVE_DATE
        {  
            get  
            {  
                return this.INACTIVE_DATEValue;  
            }  

          set { SetProperty(ref  INACTIVE_DATEValue, value); }
        } 
private  System.Decimal DESIGN_PRESSUREValue; 
 public System.Decimal DESIGN_PRESSURE
        {  
            get  
            {  
                return this.DESIGN_PRESSUREValue;  
            }  

          set { SetProperty(ref  DESIGN_PRESSUREValue, value); }
        } 
private  System.String DESIGN_PRESSURE_OUOMValue; 
 public System.String DESIGN_PRESSURE_OUOM
        {  
            get  
            {  
                return this.DESIGN_PRESSURE_OUOMValue;  
            }  

          set { SetProperty(ref  DESIGN_PRESSURE_OUOMValue, value); }
        } 
private  System.Decimal DESIGN_TEMPERATUREValue; 
 public System.Decimal DESIGN_TEMPERATURE
        {  
            get  
            {  
                return this.DESIGN_TEMPERATUREValue;  
            }  

          set { SetProperty(ref  DESIGN_TEMPERATUREValue, value); }
        } 
private  System.String DESIGN_TEMPERATURE_OUOMValue; 
 public System.String DESIGN_TEMPERATURE_OUOM
        {  
            get  
            {  
                return this.DESIGN_TEMPERATURE_OUOMValue;  
            }  

          set { SetProperty(ref  DESIGN_TEMPERATURE_OUOMValue, value); }
        } 
private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
        } 
private  System.String LOCAL_COORD_SYSTEM_IDValue; 
 public System.String LOCAL_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.LOCAL_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  LOCAL_COORD_SYSTEM_IDValue, value); }
        } 
private  System.String COORD_ACQUISITION_IDValue; 
 public System.String COORD_ACQUISITION_ID
        {  
            get  
            {  
                return this.COORD_ACQUISITION_IDValue;  
            }  

          set { SetProperty(ref  COORD_ACQUISITION_IDValue, value); }
        } 
private  System.Decimal LATITUDEValue; 
 public System.Decimal LATITUDE
        {  
            get  
            {  
                return this.LATITUDEValue;  
            }  

          set { SetProperty(ref  LATITUDEValue, value); }
        } 
private  System.Decimal LONGITUDEValue; 
 public System.Decimal LONGITUDE
        {  
            get  
            {  
                return this.LONGITUDEValue;  
            }  

          set { SetProperty(ref  LONGITUDEValue, value); }
        } 
private  System.Decimal ELEVATIONValue; 
 public System.Decimal ELEVATION
        {  
            get  
            {  
                return this.ELEVATIONValue;  
            }  

          set { SetProperty(ref  ELEVATIONValue, value); }
        } 
private  System.String ELEVATION_OUOMValue; 
 public System.String ELEVATION_OUOM
        {  
            get  
            {  
                return this.ELEVATION_OUOMValue;  
            }  

          set { SetProperty(ref  ELEVATION_OUOMValue, value); }
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
private  System.Decimal? DIAMETERValue; 
 public System.Decimal? DIAMETER
        {  
            get  
            {  
                return this.DIAMETERValue;  
            }  

          set { SetProperty(ref  DIAMETERValue, value); }
        } 
private  System.String DIAMETER_OUOMValue; 
 public System.String DIAMETER_OUOM
        {  
            get  
            {  
                return this.DIAMETER_OUOMValue;  
            }  

          set { SetProperty(ref  DIAMETER_OUOMValue, value); }
        } 
private  System.Decimal? LENGTHValue; 
 public System.Decimal? LENGTH
        {  
            get  
            {  
                return this.LENGTHValue;  
            }  

          set { SetProperty(ref  LENGTHValue, value); }
        } 
private  System.String LENGTH_OUOMValue; 
 public System.String LENGTH_OUOM
        {  
            get  
            {  
                return this.LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  LENGTH_OUOMValue, value); }
        } 


    public PIPELINE () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class CS_COORDINATE_SYSTEM: Entity,IPPDMEntity

{

private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
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
private  System.String COORDINATE_SYSTEM_TYPEValue; 
 public System.String COORDINATE_SYSTEM_TYPE
        {  
            get  
            {  
                return this.COORDINATE_SYSTEM_TYPEValue;  
            }  

          set { SetProperty(ref  COORDINATE_SYSTEM_TYPEValue, value); }
        } 
private  System.String COORD_SYSTEM_ABBREVIATIONValue; 
 public System.String COORD_SYSTEM_ABBREVIATION
        {  
            get  
            {  
                return this.COORD_SYSTEM_ABBREVIATIONValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_ABBREVIATIONValue, value); }
        } 
private  System.String COORD_SYSTEM_AREAValue; 
 public System.String COORD_SYSTEM_AREA
        {  
            get  
            {  
                return this.COORD_SYSTEM_AREAValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_AREAValue, value); }
        } 
private  System.String COORD_SYSTEM_LONG_NAMEValue; 
 public System.String COORD_SYSTEM_LONG_NAME
        {  
            get  
            {  
                return this.COORD_SYSTEM_LONG_NAMEValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_LONG_NAMEValue, value); }
        } 
private  System.String COORD_SYSTEM_SHORT_NAMEValue; 
 public System.String COORD_SYSTEM_SHORT_NAME
        {  
            get  
            {  
                return this.COORD_SYSTEM_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_SHORT_NAMEValue, value); }
        } 
private  System.String COORD_SYSTEM_UOMValue; 
 public System.String COORD_SYSTEM_UOM
        {  
            get  
            {  
                return this.COORD_SYSTEM_UOMValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_UOMValue, value); }
        } 
private  System.String DATUM_OUOMValue; 
 public System.String DATUM_OUOM
        {  
            get  
            {  
                return this.DATUM_OUOMValue;  
            }  

          set { SetProperty(ref  DATUM_OUOMValue, value); }
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
private  System.String GEODETIC_DATUMValue; 
 public System.String GEODETIC_DATUM
        {  
            get  
            {  
                return this.GEODETIC_DATUMValue;  
            }  

          set { SetProperty(ref  GEODETIC_DATUMValue, value); }
        } 
private  System.Decimal N_VALUEValue; 
 public System.Decimal N_VALUE
        {  
            get  
            {  
                return this.N_VALUEValue;  
            }  

          set { SetProperty(ref  N_VALUEValue, value); }
        } 
private  System.String N_VALUE_OUOMValue; 
 public System.String N_VALUE_OUOM
        {  
            get  
            {  
                return this.N_VALUE_OUOMValue;  
            }  

          set { SetProperty(ref  N_VALUE_OUOMValue, value); }
        } 
private  System.String OWNER_BA_IDValue; 
 public System.String OWNER_BA_ID
        {  
            get  
            {  
                return this.OWNER_BA_IDValue;  
            }  

          set { SetProperty(ref  OWNER_BA_IDValue, value); }
        } 
private  System.String PARENT_COORD_SYSTEM_IDValue; 
 public System.String PARENT_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.PARENT_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  PARENT_COORD_SYSTEM_IDValue, value); }
        } 
private  System.Decimal PERSPECTIVE_HEIGHTValue; 
 public System.Decimal PERSPECTIVE_HEIGHT
        {  
            get  
            {  
                return this.PERSPECTIVE_HEIGHTValue;  
            }  

          set { SetProperty(ref  PERSPECTIVE_HEIGHTValue, value); }
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
private  System.String PRIME_MERIDIANValue; 
 public System.String PRIME_MERIDIAN
        {  
            get  
            {  
                return this.PRIME_MERIDIANValue;  
            }  

          set { SetProperty(ref  PRIME_MERIDIANValue, value); }
        } 
private  System.String PRINCIPAL_MERIDIANValue; 
 public System.String PRINCIPAL_MERIDIAN
        {  
            get  
            {  
                return this.PRINCIPAL_MERIDIANValue;  
            }  

          set { SetProperty(ref  PRINCIPAL_MERIDIANValue, value); }
        } 
private  System.String PROJECTION_TYPEValue; 
 public System.String PROJECTION_TYPE
        {  
            get  
            {  
                return this.PROJECTION_TYPEValue;  
            }  

          set { SetProperty(ref  PROJECTION_TYPEValue, value); }
        } 
private  System.Decimal REFERENCE_ELEVValue; 
 public System.Decimal REFERENCE_ELEV
        {  
            get  
            {  
                return this.REFERENCE_ELEVValue;  
            }  

          set { SetProperty(ref  REFERENCE_ELEVValue, value); }
        } 
private  System.String REFERENCE_ELEV_OUOMValue; 
 public System.String REFERENCE_ELEV_OUOM
        {  
            get  
            {  
                return this.REFERENCE_ELEV_OUOMValue;  
            }  

          set { SetProperty(ref  REFERENCE_ELEV_OUOMValue, value); }
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
private  System.String ROTATION_INDValue; 
 public System.String ROTATION_IND
        {  
            get  
            {  
                return this.ROTATION_INDValue;  
            }  

          set { SetProperty(ref  ROTATION_INDValue, value); }
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
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.String VERTICAL_DATUM_TYPEValue; 
 public System.String VERTICAL_DATUM_TYPE
        {  
            get  
            {  
                return this.VERTICAL_DATUM_TYPEValue;  
            }  

          set { SetProperty(ref  VERTICAL_DATUM_TYPEValue, value); }
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


    public CS_COORDINATE_SYSTEM () { }

  }
}


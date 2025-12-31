using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class CS_GEODETIC_DATUM: Entity,IPPDMEntity

{

private  System.String GEODETIC_DATUMValue; 
 public System.String GEODETIC_DATUM
        {  
            get  
            {  
                return this.GEODETIC_DATUMValue;  
            }  

          set { SetProperty(ref  GEODETIC_DATUMValue, value); }
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
private  System.String DATUM_ORIGINValue; 
 public System.String DATUM_ORIGIN
        {  
            get  
            {  
                return this.DATUM_ORIGINValue;  
            }  

          set { SetProperty(ref  DATUM_ORIGINValue, value); }
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
private  System.String ELLIPSOID_IDValue; 
 public System.String ELLIPSOID_ID
        {  
            get  
            {  
                return this.ELLIPSOID_IDValue;  
            }  

          set { SetProperty(ref  ELLIPSOID_IDValue, value); }
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
private  System.String GEODETIC_DATUM_AREA_IDValue; 
 public System.String GEODETIC_DATUM_AREA_ID
        {  
            get  
            {  
                return this.GEODETIC_DATUM_AREA_IDValue;  
            }  

          set { SetProperty(ref  GEODETIC_DATUM_AREA_IDValue, value); }
        } 
private  System.String GEODETIC_DATUM_AREA_TYPEValue; 
 public System.String GEODETIC_DATUM_AREA_TYPE
        {  
            get  
            {  
                return this.GEODETIC_DATUM_AREA_TYPEValue;  
            }  

          set { SetProperty(ref  GEODETIC_DATUM_AREA_TYPEValue, value); }
        } 
private  System.String GEODETIC_DATUM_NAMEValue; 
 public System.String GEODETIC_DATUM_NAME
        {  
            get  
            {  
                return this.GEODETIC_DATUM_NAMEValue;  
            }  

          set { SetProperty(ref  GEODETIC_DATUM_NAMEValue, value); }
        } 
private  System.String ORIGIN_ANGULAR_OUOMValue; 
 public System.String ORIGIN_ANGULAR_OUOM
        {  
            get  
            {  
                return this.ORIGIN_ANGULAR_OUOMValue;  
            }  

          set { SetProperty(ref  ORIGIN_ANGULAR_OUOMValue, value); }
        } 
private  System.Decimal ORIGIN_LATITUDEValue; 
 public System.Decimal ORIGIN_LATITUDE
        {  
            get  
            {  
                return this.ORIGIN_LATITUDEValue;  
            }  

          set { SetProperty(ref  ORIGIN_LATITUDEValue, value); }
        } 
private  System.Decimal ORIGIN_LONGITUDEValue; 
 public System.Decimal ORIGIN_LONGITUDE
        {  
            get  
            {  
                return this.ORIGIN_LONGITUDEValue;  
            }  

          set { SetProperty(ref  ORIGIN_LONGITUDEValue, value); }
        } 
private  System.String ORIGIN_NAMEValue; 
 public System.String ORIGIN_NAME
        {  
            get  
            {  
                return this.ORIGIN_NAMEValue;  
            }  

          set { SetProperty(ref  ORIGIN_NAMEValue, value); }
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
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
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


    public CS_GEODETIC_DATUM () { }

  }
}


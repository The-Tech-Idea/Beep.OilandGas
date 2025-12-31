using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_SET: Entity,IPPDMEntity

{

private  System.String SEIS_SET_SUBTYPEValue; 
 public System.String SEIS_SET_SUBTYPE
        {  
            get  
            {  
                return this.SEIS_SET_SUBTYPEValue;  
            }  

          set { SetProperty(ref  SEIS_SET_SUBTYPEValue, value); }
        } 
private  System.String SEIS_SET_IDValue; 
 public System.String SEIS_SET_ID
        {  
            get  
            {  
                return this.SEIS_SET_IDValue;  
            }  

          set { SetProperty(ref  SEIS_SET_IDValue, value); }
        } 
private  System.String ACQTN_DESIGN_IDValue; 
 public System.String ACQTN_DESIGN_ID
        {  
            get  
            {  
                return this.ACQTN_DESIGN_IDValue;  
            }  

          set { SetProperty(ref  ACQTN_DESIGN_IDValue, value); }
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
private  System.String AREA_IDValue; 
 public System.String AREA_ID
        {  
            get  
            {  
                return this.AREA_IDValue;  
            }  

          set { SetProperty(ref  AREA_IDValue, value); }
        } 
private  System.Decimal AREA_SIZEValue; 
 public System.Decimal AREA_SIZE
        {  
            get  
            {  
                return this.AREA_SIZEValue;  
            }  

          set { SetProperty(ref  AREA_SIZEValue, value); }
        } 
private  System.String AREA_SIZE_OUOMValue; 
 public System.String AREA_SIZE_OUOM
        {  
            get  
            {  
                return this.AREA_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  AREA_SIZE_OUOMValue, value); }
        } 
private  System.String AREA_TYPEValue; 
 public System.String AREA_TYPE
        {  
            get  
            {  
                return this.AREA_TYPEValue;  
            }  

          set { SetProperty(ref  AREA_TYPEValue, value); }
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
private  System.String CURRENT_SEIS_STATUSValue; 
 public System.String CURRENT_SEIS_STATUS
        {  
            get  
            {  
                return this.CURRENT_SEIS_STATUSValue;  
            }  

          set { SetProperty(ref  CURRENT_SEIS_STATUSValue, value); }
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
private  System.String FINANCE_IDValue; 
 public System.String FINANCE_ID
        {  
            get  
            {  
                return this.FINANCE_IDValue;  
            }  

          set { SetProperty(ref  FINANCE_IDValue, value); }
        } 
private  System.String GEOGRAPHIC_COORD_SYSTEM_IDValue; 
 public System.String GEOGRAPHIC_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.GEOGRAPHIC_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  GEOGRAPHIC_COORD_SYSTEM_IDValue, value); }
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
private  System.Decimal MAX_LATITUDEValue; 
 public System.Decimal MAX_LATITUDE
        {  
            get  
            {  
                return this.MAX_LATITUDEValue;  
            }  

          set { SetProperty(ref  MAX_LATITUDEValue, value); }
        } 
private  System.Decimal MAX_LONGITUDEValue; 
 public System.Decimal MAX_LONGITUDE
        {  
            get  
            {  
                return this.MAX_LONGITUDEValue;  
            }  

          set { SetProperty(ref  MAX_LONGITUDEValue, value); }
        } 
private  System.Decimal MIN_LATITUDEValue; 
 public System.Decimal MIN_LATITUDE
        {  
            get  
            {  
                return this.MIN_LATITUDEValue;  
            }  

          set { SetProperty(ref  MIN_LATITUDEValue, value); }
        } 
private  System.Decimal MIN_LONGITUDEValue; 
 public System.Decimal MIN_LONGITUDE
        {  
            get  
            {  
                return this.MIN_LONGITUDEValue;  
            }  

          set { SetProperty(ref  MIN_LONGITUDEValue, value); }
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
private  System.String PREFERRED_NAMEValue; 
 public System.String PREFERRED_NAME
        {  
            get  
            {  
                return this.PREFERRED_NAMEValue;  
            }  

          set { SetProperty(ref  PREFERRED_NAMEValue, value); }
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
private  System.String XY_COORD_SYSTEM_IDValue; 
 public System.String XY_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.XY_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  XY_COORD_SYSTEM_IDValue, value); }
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


    public SEIS_SET () { }

  }
}


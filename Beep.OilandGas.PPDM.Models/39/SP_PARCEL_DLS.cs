using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SP_PARCEL_DLS: Entity,IPPDMEntity

{

private  System.String PARCEL_DLS_IDValue; 
 public System.String PARCEL_DLS_ID
        {  
            get  
            {  
                return this.PARCEL_DLS_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_DLS_IDValue, value); }
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
private  System.String AREA_TYPEValue; 
 public System.String AREA_TYPE
        {  
            get  
            {  
                return this.AREA_TYPEValue;  
            }  

          set { SetProperty(ref  AREA_TYPEValue, value); }
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
private  System.String DESCRIPTIONValue; 
 public System.String DESCRIPTION
        {  
            get  
            {  
                return this.DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  DESCRIPTIONValue, value); }
        } 
private  System.Decimal DLS_LEGAL_SUBDIVISIONValue; 
 public System.Decimal DLS_LEGAL_SUBDIVISION
        {  
            get  
            {  
                return this.DLS_LEGAL_SUBDIVISIONValue;  
            }  

          set { SetProperty(ref  DLS_LEGAL_SUBDIVISIONValue, value); }
        } 
private  System.Decimal DLS_MERIDIANValue; 
 public System.Decimal DLS_MERIDIAN
        {  
            get  
            {  
                return this.DLS_MERIDIANValue;  
            }  

          set { SetProperty(ref  DLS_MERIDIANValue, value); }
        } 
private  System.String DLS_MERIDIAN_DIRECTIONValue; 
 public System.String DLS_MERIDIAN_DIRECTION
        {  
            get  
            {  
                return this.DLS_MERIDIAN_DIRECTIONValue;  
            }  

          set { SetProperty(ref  DLS_MERIDIAN_DIRECTIONValue, value); }
        } 
private  System.String DLS_QUARTER_SECTIONValue; 
 public System.String DLS_QUARTER_SECTION
        {  
            get  
            {  
                return this.DLS_QUARTER_SECTIONValue;  
            }  

          set { SetProperty(ref  DLS_QUARTER_SECTIONValue, value); }
        } 
private  System.String DLS_QUARTER_SECTION_QUARTERValue; 
 public System.String DLS_QUARTER_SECTION_QUARTER
        {  
            get  
            {  
                return this.DLS_QUARTER_SECTION_QUARTERValue;  
            }  

          set { SetProperty(ref  DLS_QUARTER_SECTION_QUARTERValue, value); }
        } 
private  System.Decimal DLS_RANGEValue; 
 public System.Decimal DLS_RANGE
        {  
            get  
            {  
                return this.DLS_RANGEValue;  
            }  

          set { SetProperty(ref  DLS_RANGEValue, value); }
        } 
private  System.String DLS_RANGE_MODIFIERValue; 
 public System.String DLS_RANGE_MODIFIER
        {  
            get  
            {  
                return this.DLS_RANGE_MODIFIERValue;  
            }  

          set { SetProperty(ref  DLS_RANGE_MODIFIERValue, value); }
        } 
private  System.Decimal DLS_SECTIONValue; 
 public System.Decimal DLS_SECTION
        {  
            get  
            {  
                return this.DLS_SECTIONValue;  
            }  

          set { SetProperty(ref  DLS_SECTIONValue, value); }
        } 
private  System.Decimal DLS_TOWNSHIPValue; 
 public System.Decimal DLS_TOWNSHIP
        {  
            get  
            {  
                return this.DLS_TOWNSHIPValue;  
            }  

          set { SetProperty(ref  DLS_TOWNSHIPValue, value); }
        } 
private  System.String DLS_TOWNSHIP_MODIFIERValue; 
 public System.String DLS_TOWNSHIP_MODIFIER
        {  
            get  
            {  
                return this.DLS_TOWNSHIP_MODIFIERValue;  
            }  

          set { SetProperty(ref  DLS_TOWNSHIP_MODIFIERValue, value); }
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
private  System.String LOCAL_COORD_SYSTEM_IDValue; 
 public System.String LOCAL_COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.LOCAL_COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  LOCAL_COORD_SYSTEM_IDValue, value); }
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
private  System.String PRINCIPAL_MERIDIANValue; 
 public System.String PRINCIPAL_MERIDIAN
        {  
            get  
            {  
                return this.PRINCIPAL_MERIDIANValue;  
            }  

          set { SetProperty(ref  PRINCIPAL_MERIDIANValue, value); }
        } 
private  System.String REFERENCE_PLAN_NUMValue; 
 public System.String REFERENCE_PLAN_NUM
        {  
            get  
            {  
                return this.REFERENCE_PLAN_NUMValue;  
            }  

          set { SetProperty(ref  REFERENCE_PLAN_NUMValue, value); }
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
private  System.String SPATIAL_DESCRIPTION_IDValue; 
 public System.String SPATIAL_DESCRIPTION_ID
        {  
            get  
            {  
                return this.SPATIAL_DESCRIPTION_IDValue;  
            }  

          set { SetProperty(ref  SPATIAL_DESCRIPTION_IDValue, value); }
        } 
private  System.Decimal SPATIAL_OBS_NOValue; 
 public System.Decimal SPATIAL_OBS_NO
        {  
            get  
            {  
                return this.SPATIAL_OBS_NOValue;  
            }  

          set { SetProperty(ref  SPATIAL_OBS_NOValue, value); }
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


    public SP_PARCEL_DLS () { }

  }
}


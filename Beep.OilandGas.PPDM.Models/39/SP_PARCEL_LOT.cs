using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SP_PARCEL_LOT: Entity,IPPDMEntity

{

private  System.String PARCEL_LOT_IDValue; 
 public System.String PARCEL_LOT_ID
        {  
            get  
            {  
                return this.PARCEL_LOT_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_LOT_IDValue, value); }
        } 
private  System.String PARCEL_LOT_TYPEValue; 
 public System.String PARCEL_LOT_TYPE
        {  
            get  
            {  
                return this.PARCEL_LOT_TYPEValue;  
            }  

          set { SetProperty(ref  PARCEL_LOT_TYPEValue, value); }
        } 
private  System.String PARCEL_LOT_NUMValue; 
 public System.String PARCEL_LOT_NUM
        {  
            get  
            {  
                return this.PARCEL_LOT_NUMValue;  
            }  

          set { SetProperty(ref  PARCEL_LOT_NUMValue, value); }
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
private  System.Decimal GROSS_SIZEValue; 
 public System.Decimal GROSS_SIZE
        {  
            get  
            {  
                return this.GROSS_SIZEValue;  
            }  

          set { SetProperty(ref  GROSS_SIZEValue, value); }
        } 
private  System.String GROSS_SIZE_OUOMValue; 
 public System.String GROSS_SIZE_OUOM
        {  
            get  
            {  
                return this.GROSS_SIZE_OUOMValue;  
            }  

          set { SetProperty(ref  GROSS_SIZE_OUOMValue, value); }
        } 
private  System.String LOT_DESCRIPTIONValue; 
 public System.String LOT_DESCRIPTION
        {  
            get  
            {  
                return this.LOT_DESCRIPTIONValue;  
            }  

          set { SetProperty(ref  LOT_DESCRIPTIONValue, value); }
        } 
private  System.String LOT_NAMEValue; 
 public System.String LOT_NAME
        {  
            get  
            {  
                return this.LOT_NAMEValue;  
            }  

          set { SetProperty(ref  LOT_NAMEValue, value); }
        } 
private  System.String PARCEL_CONGRESS_IDValue; 
 public System.String PARCEL_CONGRESS_ID
        {  
            get  
            {  
                return this.PARCEL_CONGRESS_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_CONGRESS_IDValue, value); }
        } 
private  System.String PARCEL_OHIO_IDValue; 
 public System.String PARCEL_OHIO_ID
        {  
            get  
            {  
                return this.PARCEL_OHIO_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_OHIO_IDValue, value); }
        } 
private  System.String PARCEL_PBL_IDValue; 
 public System.String PARCEL_PBL_ID
        {  
            get  
            {  
                return this.PARCEL_PBL_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_PBL_IDValue, value); }
        } 
private  System.String PARCEL_TEXAS_IDValue; 
 public System.String PARCEL_TEXAS_ID
        {  
            get  
            {  
                return this.PARCEL_TEXAS_IDValue;  
            }  

          set { SetProperty(ref  PARCEL_TEXAS_IDValue, value); }
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
private  System.String REMARK_TYPEValue; 
 public System.String REMARK_TYPE
        {  
            get  
            {  
                return this.REMARK_TYPEValue;  
            }  

          set { SetProperty(ref  REMARK_TYPEValue, value); }
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


    public SP_PARCEL_LOT () { }

  }
}


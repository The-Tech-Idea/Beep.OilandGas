using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SEIS_BIN_OUTLINE: Entity,IPPDMEntity

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
private  System.String BIN_GRID_IDValue; 
 public System.String BIN_GRID_ID
        {  
            get  
            {  
                return this.BIN_GRID_IDValue;  
            }  

          set { SetProperty(ref  BIN_GRID_IDValue, value); }
        } 
private  System.String BIN_GRID_SOURCEValue; 
 public System.String BIN_GRID_SOURCE
        {  
            get  
            {  
                return this.BIN_GRID_SOURCEValue;  
            }  

          set { SetProperty(ref  BIN_GRID_SOURCEValue, value); }
        } 
private  System.Decimal OUTLINE_SEQ_NOValue; 
 public System.Decimal OUTLINE_SEQ_NO
        {  
            get  
            {  
                return this.OUTLINE_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OUTLINE_SEQ_NOValue, value); }
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
private  System.String BIN_OUTLINE_TYPEValue; 
 public System.String BIN_OUTLINE_TYPE
        {  
            get  
            {  
                return this.BIN_OUTLINE_TYPEValue;  
            }  

          set { SetProperty(ref  BIN_OUTLINE_TYPEValue, value); }
        } 
private  System.String BIN_POINT_IDValue; 
 public System.String BIN_POINT_ID
        {  
            get  
            {  
                return this.BIN_POINT_IDValue;  
            }  

          set { SetProperty(ref  BIN_POINT_IDValue, value); }
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
private  System.String COORD_SYSTEM_IDValue; 
 public System.String COORD_SYSTEM_ID
        {  
            get  
            {  
                return this.COORD_SYSTEM_IDValue;  
            }  

          set { SetProperty(ref  COORD_SYSTEM_IDValue, value); }
        } 
private  System.Decimal EASTINGValue; 
 public System.Decimal EASTING
        {  
            get  
            {  
                return this.EASTINGValue;  
            }  

          set { SetProperty(ref  EASTINGValue, value); }
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
private  System.Decimal NLINE_NOValue; 
 public System.Decimal NLINE_NO
        {  
            get  
            {  
                return this.NLINE_NOValue;  
            }  

          set { SetProperty(ref  NLINE_NOValue, value); }
        } 
private  System.Decimal NORTHINGValue; 
 public System.Decimal NORTHING
        {  
            get  
            {  
                return this.NORTHINGValue;  
            }  

          set { SetProperty(ref  NORTHINGValue, value); }
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
private  System.Decimal XLINE_NOValue; 
 public System.Decimal XLINE_NO
        {  
            get  
            {  
                return this.XLINE_NOValue;  
            }  

          set { SetProperty(ref  XLINE_NOValue, value); }
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


    public SEIS_BIN_OUTLINE () { }

  }
}


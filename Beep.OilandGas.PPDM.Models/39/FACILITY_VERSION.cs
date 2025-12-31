using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class FACILITY_VERSION: Entity,IPPDMEntity

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
private  System.DateTime? ACTIVE_DATEValue; 
 public System.DateTime? ACTIVE_DATE
        {  
            get  
            {  
                return this.ACTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ACTIVE_DATEValue, value); }
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
private  System.DateTime? CONSTRUCTED_DATEValue; 
 public System.DateTime? CONSTRUCTED_DATE
        {  
            get  
            {  
                return this.CONSTRUCTED_DATEValue;  
            }  

          set { SetProperty(ref  CONSTRUCTED_DATEValue, value); }
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
private  System.DateTime? CURRENT_STATUS_DATEValue; 
 public System.DateTime? CURRENT_STATUS_DATE
        {  
            get  
            {  
                return this.CURRENT_STATUS_DATEValue;  
            }  

          set { SetProperty(ref  CURRENT_STATUS_DATEValue, value); }
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
private  System.String FACILITY_LONG_NAMEValue; 
 public System.String FACILITY_LONG_NAME
        {  
            get  
            {  
                return this.FACILITY_LONG_NAMEValue;  
            }  

          set { SetProperty(ref  FACILITY_LONG_NAMEValue, value); }
        } 
private  System.String FACILITY_SHORT_NAMEValue; 
 public System.String FACILITY_SHORT_NAME
        {  
            get  
            {  
                return this.FACILITY_SHORT_NAMEValue;  
            }  

          set { SetProperty(ref  FACILITY_SHORT_NAMEValue, value); }
        } 
private  System.String FACILITY_STATUS_IDValue; 
 public System.String FACILITY_STATUS_ID
        {  
            get  
            {  
                return this.FACILITY_STATUS_IDValue;  
            }  

          set { SetProperty(ref  FACILITY_STATUS_IDValue, value); }
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
private  System.DateTime? LAST_INJECTION_DATEValue; 
 public System.DateTime? LAST_INJECTION_DATE
        {  
            get  
            {  
                return this.LAST_INJECTION_DATEValue;  
            }  

          set { SetProperty(ref  LAST_INJECTION_DATEValue, value); }
        } 
private  System.DateTime? LAST_PRODUCTION_DATEValue; 
 public System.DateTime? LAST_PRODUCTION_DATE
        {  
            get  
            {  
                return this.LAST_PRODUCTION_DATEValue;  
            }  

          set { SetProperty(ref  LAST_PRODUCTION_DATEValue, value); }
        } 
private  System.DateTime? LAST_REPORTED_DATEValue; 
 public System.DateTime? LAST_REPORTED_DATE
        {  
            get  
            {  
                return this.LAST_REPORTED_DATEValue;  
            }  

          set { SetProperty(ref  LAST_REPORTED_DATEValue, value); }
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
private  System.DateTime? ON_INJECTION_DATEValue; 
 public System.DateTime? ON_INJECTION_DATE
        {  
            get  
            {  
                return this.ON_INJECTION_DATEValue;  
            }  

          set { SetProperty(ref  ON_INJECTION_DATEValue, value); }
        } 
private  System.DateTime? ON_PRODUCTION_DATEValue; 
 public System.DateTime? ON_PRODUCTION_DATE
        {  
            get  
            {  
                return this.ON_PRODUCTION_DATEValue;  
            }  

          set { SetProperty(ref  ON_PRODUCTION_DATEValue, value); }
        } 
private  System.String PLOT_NAMEValue; 
 public System.String PLOT_NAME
        {  
            get  
            {  
                return this.PLOT_NAMEValue;  
            }  

          set { SetProperty(ref  PLOT_NAMEValue, value); }
        } 
private  System.String PLOT_SYMBOLValue; 
 public System.String PLOT_SYMBOL
        {  
            get  
            {  
                return this.PLOT_SYMBOLValue;  
            }  

          set { SetProperty(ref  PLOT_SYMBOLValue, value); }
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
private  System.String STATUS_TYPEValue; 
 public System.String STATUS_TYPE
        {  
            get  
            {  
                return this.STATUS_TYPEValue;  
            }  

          set { SetProperty(ref  STATUS_TYPEValue, value); }
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


    public FACILITY_VERSION () { }

  }
}


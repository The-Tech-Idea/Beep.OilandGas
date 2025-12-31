using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class PROD_STRING: Entity,IPPDMEntity

{

private  System.String UWIValue; 
 public System.String UWI
        {  
            get  
            {  
                return this.UWIValue;  
            }  

          set { SetProperty(ref  UWIValue, value); }
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
private  System.String STRING_IDValue; 
 public System.String STRING_ID
        {  
            get  
            {  
                return this.STRING_IDValue;  
            }  

          set { SetProperty(ref  STRING_IDValue, value); }
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
private  System.Decimal BASE_DEPTHValue; 
 public System.Decimal BASE_DEPTH
        {  
            get  
            {  
                return this.BASE_DEPTHValue;  
            }  

          set { SetProperty(ref  BASE_DEPTHValue, value); }
        } 
private  System.String BASE_DEPTH_OUOMValue; 
 public System.String BASE_DEPTH_OUOM
        {  
            get  
            {  
                return this.BASE_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  BASE_DEPTH_OUOMValue, value); }
        } 
private  System.String BUSINESS_ASSOCIATE_IDValue; 
 public System.String BUSINESS_ASSOCIATE_ID
        {  
            get  
            {  
                return this.BUSINESS_ASSOCIATE_IDValue;  
            }  

          set { SetProperty(ref  BUSINESS_ASSOCIATE_IDValue, value); }
        } 
private  System.String COMMINGLED_INDValue; 
 public System.String COMMINGLED_IND
        {  
            get  
            {  
                return this.COMMINGLED_INDValue;  
            }  

          set { SetProperty(ref  COMMINGLED_INDValue, value); }
        } 
private  System.String CURRENT_STATUSValue; 
 public System.String CURRENT_STATUS
        {  
            get  
            {  
                return this.CURRENT_STATUSValue;  
            }  

          set { SetProperty(ref  CURRENT_STATUSValue, value); }
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
private  System.String FIELD_IDValue; 
 public System.String FIELD_ID
        {  
            get  
            {  
                return this.FIELD_IDValue;  
            }  

          set { SetProperty(ref  FIELD_IDValue, value); }
        } 
private  System.String GOVERNMENT_STRING_IDValue; 
 public System.String GOVERNMENT_STRING_ID
        {  
            get  
            {  
                return this.GOVERNMENT_STRING_IDValue;  
            }  

          set { SetProperty(ref  GOVERNMENT_STRING_IDValue, value); }
        } 
private  System.String LEASE_UNIT_IDValue; 
 public System.String LEASE_UNIT_ID
        {  
            get  
            {  
                return this.LEASE_UNIT_IDValue;  
            }  

          set { SetProperty(ref  LEASE_UNIT_IDValue, value); }
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
private  System.String PLOT_SYMBOLValue; 
 public System.String PLOT_SYMBOL
        {  
            get  
            {  
                return this.PLOT_SYMBOLValue;  
            }  

          set { SetProperty(ref  PLOT_SYMBOLValue, value); }
        } 
private  System.String POOL_IDValue; 
 public System.String POOL_ID
        {  
            get  
            {  
                return this.POOL_IDValue;  
            }  

          set { SetProperty(ref  POOL_IDValue, value); }
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
private  System.Decimal PROD_STRING_TVDValue; 
 public System.Decimal PROD_STRING_TVD
        {  
            get  
            {  
                return this.PROD_STRING_TVDValue;  
            }  

          set { SetProperty(ref  PROD_STRING_TVDValue, value); }
        } 
private  System.String PROD_STRING_TVD_OUOMValue; 
 public System.String PROD_STRING_TVD_OUOM
        {  
            get  
            {  
                return this.PROD_STRING_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  PROD_STRING_TVD_OUOMValue, value); }
        } 
private  System.String PROD_STRING_TYPEValue; 
 public System.String PROD_STRING_TYPE
        {  
            get  
            {  
                return this.PROD_STRING_TYPEValue;  
            }  

          set { SetProperty(ref  PROD_STRING_TYPEValue, value); }
        } 
private  System.String PROFILE_TYPEValue; 
 public System.String PROFILE_TYPE
        {  
            get  
            {  
                return this.PROFILE_TYPEValue;  
            }  

          set { SetProperty(ref  PROFILE_TYPEValue, value); }
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
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
        } 
private  System.String STRAT_UNIT_IDValue; 
 public System.String STRAT_UNIT_ID
        {  
            get  
            {  
                return this.STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  STRAT_UNIT_IDValue, value); }
        } 
private  System.String TAX_CREDIT_CODEValue; 
 public System.String TAX_CREDIT_CODE
        {  
            get  
            {  
                return this.TAX_CREDIT_CODEValue;  
            }  

          set { SetProperty(ref  TAX_CREDIT_CODEValue, value); }
        } 
private  System.Decimal TOP_DEPTHValue; 
 public System.Decimal TOP_DEPTH
        {  
            get  
            {  
                return this.TOP_DEPTHValue;  
            }  

          set { SetProperty(ref  TOP_DEPTHValue, value); }
        } 
private  System.String TOP_DEPTH_OUOMValue; 
 public System.String TOP_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOP_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOP_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal TOTAL_DEPTHValue; 
 public System.Decimal TOTAL_DEPTH
        {  
            get  
            {  
                return this.TOTAL_DEPTHValue;  
            }  

          set { SetProperty(ref  TOTAL_DEPTHValue, value); }
        } 
private  System.String TOTAL_DEPTH_OUOMValue; 
 public System.String TOTAL_DEPTH_OUOM
        {  
            get  
            {  
                return this.TOTAL_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  TOTAL_DEPTH_OUOMValue, value); }
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


    public PROD_STRING () { }

  }
}


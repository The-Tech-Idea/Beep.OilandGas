using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_PAYZONE: Entity

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
private  System.String ZONE_IDValue; 
 public System.String ZONE_ID
        {  
            get  
            {  
                return this.ZONE_IDValue;  
            }  

          set { SetProperty(ref  ZONE_IDValue, value); }
        } 
private  System.String ZONE_SOURCEValue; 
 public System.String ZONE_SOURCE
        {  
            get  
            {  
                return this.ZONE_SOURCEValue;  
            }  

          set { SetProperty(ref  ZONE_SOURCEValue, value); }
        } 
private  System.String PAYZONE_TYPEValue; 
 public System.String PAYZONE_TYPE
        {  
            get  
            {  
                return this.PAYZONE_TYPEValue;  
            }  

          set { SetProperty(ref  PAYZONE_TYPEValue, value); }
        } 
private  System.String FLUID_TYPEValue; 
 public System.String FLUID_TYPE
        {  
            get  
            {  
                return this.FLUID_TYPEValue;  
            }  

          set { SetProperty(ref  FLUID_TYPEValue, value); }
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
private  System.String BASE_STRAT_UNIT_IDValue; 
 public System.String BASE_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.BASE_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  BASE_STRAT_UNIT_IDValue, value); }
        } 
private  System.DateTime EFFECTIVE_DATEValue; 
 public System.DateTime EFFECTIVE_DATE
        {  
            get  
            {  
                return this.EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime EXPIRY_DATEValue; 
 public System.DateTime EXPIRY_DATE
        {  
            get  
            {  
                return this.EXPIRY_DATEValue;  
            }  

          set { SetProperty(ref  EXPIRY_DATEValue, value); }
        } 
private  System.Decimal GAS_OIL_CONTACT_DEPTHValue; 
 public System.Decimal GAS_OIL_CONTACT_DEPTH
        {  
            get  
            {  
                return this.GAS_OIL_CONTACT_DEPTHValue;  
            }  

          set { SetProperty(ref  GAS_OIL_CONTACT_DEPTHValue, value); }
        } 
private  System.String GAS_OIL_CONTACT_DEPTH_OUOMValue; 
 public System.String GAS_OIL_CONTACT_DEPTH_OUOM
        {  
            get  
            {  
                return this.GAS_OIL_CONTACT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_OIL_CONTACT_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal GAS_WTR_CONTACT_DEPTHValue; 
 public System.Decimal GAS_WTR_CONTACT_DEPTH
        {  
            get  
            {  
                return this.GAS_WTR_CONTACT_DEPTHValue;  
            }  

          set { SetProperty(ref  GAS_WTR_CONTACT_DEPTHValue, value); }
        } 
private  System.String GAS_WTR_CONTACT_DEPTH_OUOMValue; 
 public System.String GAS_WTR_CONTACT_DEPTH_OUOM
        {  
            get  
            {  
                return this.GAS_WTR_CONTACT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  GAS_WTR_CONTACT_DEPTH_OUOMValue, value); }
        } 
private  System.Decimal GROSS_PAYValue; 
 public System.Decimal GROSS_PAY
        {  
            get  
            {  
                return this.GROSS_PAYValue;  
            }  

          set { SetProperty(ref  GROSS_PAYValue, value); }
        } 
private  System.String GROSS_PAY_OUOMValue; 
 public System.String GROSS_PAY_OUOM
        {  
            get  
            {  
                return this.GROSS_PAY_OUOMValue;  
            }  

          set { SetProperty(ref  GROSS_PAY_OUOMValue, value); }
        } 
private  System.Decimal NET_PAYValue; 
 public System.Decimal NET_PAY
        {  
            get  
            {  
                return this.NET_PAYValue;  
            }  

          set { SetProperty(ref  NET_PAYValue, value); }
        } 
private  System.String NET_PAY_OUOMValue; 
 public System.String NET_PAY_OUOM
        {  
            get  
            {  
                return this.NET_PAY_OUOMValue;  
            }  

          set { SetProperty(ref  NET_PAY_OUOMValue, value); }
        } 
private  System.Decimal OIL_WTR_CONTACT_DEPTHValue; 
 public System.Decimal OIL_WTR_CONTACT_DEPTH
        {  
            get  
            {  
                return this.OIL_WTR_CONTACT_DEPTHValue;  
            }  

          set { SetProperty(ref  OIL_WTR_CONTACT_DEPTHValue, value); }
        } 
private  System.String OIL_WTR_CONTACT_DEPTH_OUOMValue; 
 public System.String OIL_WTR_CONTACT_DEPTH_OUOM
        {  
            get  
            {  
                return this.OIL_WTR_CONTACT_DEPTH_OUOMValue;  
            }  

          set { SetProperty(ref  OIL_WTR_CONTACT_DEPTH_OUOMValue, value); }
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
private  System.String STRAT_NAME_SET_IDValue; 
 public System.String STRAT_NAME_SET_ID
        {  
            get  
            {  
                return this.STRAT_NAME_SET_IDValue;  
            }  

          set { SetProperty(ref  STRAT_NAME_SET_IDValue, value); }
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
private  System.String TOP_STRAT_UNIT_IDValue; 
 public System.String TOP_STRAT_UNIT_ID
        {  
            get  
            {  
                return this.TOP_STRAT_UNIT_IDValue;  
            }  

          set { SetProperty(ref  TOP_STRAT_UNIT_IDValue, value); }
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
private  System.DateTime ROW_CHANGED_DATEValue; 
 public System.DateTime ROW_CHANGED_DATE
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
private  System.DateTime ROW_CREATED_DATEValue; 
 public System.DateTime ROW_CREATED_DATE
        {  
            get  
            {  
                return this.ROW_CREATED_DATEValue;  
            }  

          set { SetProperty(ref  ROW_CREATED_DATEValue, value); }
        } 
private  System.DateTime ROW_EFFECTIVE_DATEValue; 
 public System.DateTime ROW_EFFECTIVE_DATE
        {  
            get  
            {  
                return this.ROW_EFFECTIVE_DATEValue;  
            }  

          set { SetProperty(ref  ROW_EFFECTIVE_DATEValue, value); }
        } 
private  System.DateTime ROW_EXPIRY_DATEValue; 
 public System.DateTime ROW_EXPIRY_DATE
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


    public WELL_PAYZONE () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class CONTRACT: Entity,IPPDMEntity

{

private  System.String CONTRACT_IDValue; 
 public System.String CONTRACT_ID
        {  
            get  
            {  
                return this.CONTRACT_IDValue;  
            }  

          set { SetProperty(ref  CONTRACT_IDValue, value); }
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
private  System.String AMI_AOE_INDValue; 
 public System.String AMI_AOE_IND
        {  
            get  
            {  
                return this.AMI_AOE_INDValue;  
            }  

          set { SetProperty(ref  AMI_AOE_INDValue, value); }
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
private  System.String ASSIGNMENT_PROC_INDValue; 
 public System.String ASSIGNMENT_PROC_IND
        {  
            get  
            {  
                return this.ASSIGNMENT_PROC_INDValue;  
            }  

          set { SetProperty(ref  ASSIGNMENT_PROC_INDValue, value); }
        } 
private  System.String CASING_POINT_ELECTIONValue; 
 public System.String CASING_POINT_ELECTION
        {  
            get  
            {  
                return this.CASING_POINT_ELECTIONValue;  
            }  

          set { SetProperty(ref  CASING_POINT_ELECTIONValue, value); }
        } 
private  System.DateTime? CLOSING_DATEValue; 
 public System.DateTime? CLOSING_DATE
        {  
            get  
            {  
                return this.CLOSING_DATEValue;  
            }  

          set { SetProperty(ref  CLOSING_DATEValue, value); }
        } 
private  System.String CONFIDENTIAL_INDValue; 
 public System.String CONFIDENTIAL_IND
        {  
            get  
            {  
                return this.CONFIDENTIAL_INDValue;  
            }  

          set { SetProperty(ref  CONFIDENTIAL_INDValue, value); }
        } 
private  System.String CONTRACT_NAMEValue; 
 public System.String CONTRACT_NAME
        {  
            get  
            {  
                return this.CONTRACT_NAMEValue;  
            }  

          set { SetProperty(ref  CONTRACT_NAMEValue, value); }
        } 
private  System.String CONTRACT_NUMValue; 
 public System.String CONTRACT_NUM
        {  
            get  
            {  
                return this.CONTRACT_NUMValue;  
            }  

          set { SetProperty(ref  CONTRACT_NUMValue, value); }
        } 
private  System.Decimal CURRENCY_CONVERSIONValue; 
 public System.Decimal CURRENCY_CONVERSION
        {  
            get  
            {  
                return this.CURRENCY_CONVERSIONValue;  
            }  

          set { SetProperty(ref  CURRENCY_CONVERSIONValue, value); }
        } 
private  System.String CURRENCY_OUOMValue; 
 public System.String CURRENCY_OUOM
        {  
            get  
            {  
                return this.CURRENCY_OUOMValue;  
            }  

          set { SetProperty(ref  CURRENCY_OUOMValue, value); }
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
private  System.String EXTENSION_CONDITIONValue; 
 public System.String EXTENSION_CONDITION
        {  
            get  
            {  
                return this.EXTENSION_CONDITIONValue;  
            }  

          set { SetProperty(ref  EXTENSION_CONDITIONValue, value); }
        } 
private  System.String GOVERNING_CONTRACT_INDValue; 
 public System.String GOVERNING_CONTRACT_IND
        {  
            get  
            {  
                return this.GOVERNING_CONTRACT_INDValue;  
            }  

          set { SetProperty(ref  GOVERNING_CONTRACT_INDValue, value); }
        } 
private  System.Decimal LIABILITY_PERIODValue; 
 public System.Decimal LIABILITY_PERIOD
        {  
            get  
            {  
                return this.LIABILITY_PERIODValue;  
            }  

          set { SetProperty(ref  LIABILITY_PERIODValue, value); }
        } 
private  System.String LIABILITY_PERIOD_OUOMValue; 
 public System.String LIABILITY_PERIOD_OUOM
        {  
            get  
            {  
                return this.LIABILITY_PERIOD_OUOMValue;  
            }  

          set { SetProperty(ref  LIABILITY_PERIOD_OUOMValue, value); }
        } 
private  System.DateTime? LIABILITY_RELEASE_DATEValue; 
 public System.DateTime? LIABILITY_RELEASE_DATE
        {  
            get  
            {  
                return this.LIABILITY_RELEASE_DATEValue;  
            }  

          set { SetProperty(ref  LIABILITY_RELEASE_DATEValue, value); }
        } 
private  System.Decimal NAT_CURRENCY_CONVERSIONValue; 
 public System.Decimal NAT_CURRENCY_CONVERSION
        {  
            get  
            {  
                return this.NAT_CURRENCY_CONVERSIONValue;  
            }  

          set { SetProperty(ref  NAT_CURRENCY_CONVERSIONValue, value); }
        } 
private  System.String NAT_CURRENCY_UOMValue; 
 public System.String NAT_CURRENCY_UOM
        {  
            get  
            {  
                return this.NAT_CURRENCY_UOMValue;  
            }  

          set { SetProperty(ref  NAT_CURRENCY_UOMValue, value); }
        } 
private  System.String OPERATING_PROC_INDValue; 
 public System.String OPERATING_PROC_IND
        {  
            get  
            {  
                return this.OPERATING_PROC_INDValue;  
            }  

          set { SetProperty(ref  OPERATING_PROC_INDValue, value); }
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
private  System.Decimal PRIMARY_TERMValue; 
 public System.Decimal PRIMARY_TERM
        {  
            get  
            {  
                return this.PRIMARY_TERMValue;  
            }  

          set { SetProperty(ref  PRIMARY_TERMValue, value); }
        } 
private  System.String PRIMARY_TERM_OUOMValue; 
 public System.String PRIMARY_TERM_OUOM
        {  
            get  
            {  
                return this.PRIMARY_TERM_OUOMValue;  
            }  

          set { SetProperty(ref  PRIMARY_TERM_OUOMValue, value); }
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
private  System.String ROFR_INDValue; 
 public System.String ROFR_IND
        {  
            get  
            {  
                return this.ROFR_INDValue;  
            }  

          set { SetProperty(ref  ROFR_INDValue, value); }
        } 
private  System.Decimal SECONDARY_TERMValue; 
 public System.Decimal SECONDARY_TERM
        {  
            get  
            {  
                return this.SECONDARY_TERMValue;  
            }  

          set { SetProperty(ref  SECONDARY_TERMValue, value); }
        } 
private  System.String SECONDARY_TERM_OUOMValue; 
 public System.String SECONDARY_TERM_OUOM
        {  
            get  
            {  
                return this.SECONDARY_TERM_OUOMValue;  
            }  

          set { SetProperty(ref  SECONDARY_TERM_OUOMValue, value); }
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
private  System.String SOURCE_DOCUMENTValue; 
 public System.String SOURCE_DOCUMENT
        {  
            get  
            {  
                return this.SOURCE_DOCUMENTValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENTValue, value); }
        } 
private  System.Decimal SURRENDER_NOTICE_TERMValue; 
 public System.Decimal SURRENDER_NOTICE_TERM
        {  
            get  
            {  
                return this.SURRENDER_NOTICE_TERMValue;  
            }  

          set { SetProperty(ref  SURRENDER_NOTICE_TERMValue, value); }
        } 
private  System.String SURRENDER_NOTICE_TERM_OUOMValue; 
 public System.String SURRENDER_NOTICE_TERM_OUOM
        {  
            get  
            {  
                return this.SURRENDER_NOTICE_TERM_OUOMValue;  
            }  

          set { SetProperty(ref  SURRENDER_NOTICE_TERM_OUOMValue, value); }
        } 
private  System.DateTime? TERMINATION_DATEValue; 
 public System.DateTime? TERMINATION_DATE
        {  
            get  
            {  
                return this.TERMINATION_DATEValue;  
            }  

          set { SetProperty(ref  TERMINATION_DATEValue, value); }
        } 
private  System.String VOTING_PROC_INDValue; 
 public System.String VOTING_PROC_IND
        {  
            get  
            {  
                return this.VOTING_PROC_INDValue;  
            }  

          set { SetProperty(ref  VOTING_PROC_INDValue, value); }
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


    public CONTRACT () { }

  }
}


using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class OBLIGATION: Entity,IPPDMEntity

{

private  System.String OBLIGATION_IDValue; 
 public System.String OBLIGATION_ID
        {  
            get  
            {  
                return this.OBLIGATION_IDValue;  
            }  

          set { SetProperty(ref  OBLIGATION_IDValue, value); }
        } 
private  System.Decimal OBLIGATION_SEQ_NOValue; 
 public System.Decimal OBLIGATION_SEQ_NO
        {  
            get  
            {  
                return this.OBLIGATION_SEQ_NOValue;  
            }  

          set { SetProperty(ref  OBLIGATION_SEQ_NOValue, value); }
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
private  System.String CALCULATION_METHODValue; 
 public System.String CALCULATION_METHOD
        {  
            get  
            {  
                return this.CALCULATION_METHODValue;  
            }  

          set { SetProperty(ref  CALCULATION_METHODValue, value); }
        } 
private  System.String CONVERTIBLE_INDValue; 
 public System.String CONVERTIBLE_IND
        {  
            get  
            {  
                return this.CONVERTIBLE_INDValue;  
            }  

          set { SetProperty(ref  CONVERTIBLE_INDValue, value); }
        } 
private  System.DateTime? CRITICAL_DATEValue; 
 public System.DateTime? CRITICAL_DATE
        {  
            get  
            {  
                return this.CRITICAL_DATEValue;  
            }  

          set { SetProperty(ref  CRITICAL_DATEValue, value); }
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
private  System.DateTime? FULFILLED_DATEValue; 
 public System.DateTime? FULFILLED_DATE
        {  
            get  
            {  
                return this.FULFILLED_DATEValue;  
            }  

          set { SetProperty(ref  FULFILLED_DATEValue, value); }
        } 
private  System.String FULFILLED_INDValue; 
 public System.String FULFILLED_IND
        {  
            get  
            {  
                return this.FULFILLED_INDValue;  
            }  

          set { SetProperty(ref  FULFILLED_INDValue, value); }
        } 
private  System.Decimal GROSS_OBLIGATION_COSTValue; 
 public System.Decimal GROSS_OBLIGATION_COST
        {  
            get  
            {  
                return this.GROSS_OBLIGATION_COSTValue;  
            }  

          set { SetProperty(ref  GROSS_OBLIGATION_COSTValue, value); }
        } 
private  System.String INSTRUMENT_IDValue; 
 public System.String INSTRUMENT_ID
        {  
            get  
            {  
                return this.INSTRUMENT_IDValue;  
            }  

          set { SetProperty(ref  INSTRUMENT_IDValue, value); }
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
private  System.Decimal NET_OBLIGATION_COSTValue; 
 public System.Decimal NET_OBLIGATION_COST
        {  
            get  
            {  
                return this.NET_OBLIGATION_COSTValue;  
            }  

          set { SetProperty(ref  NET_OBLIGATION_COSTValue, value); }
        } 
private  System.Decimal NOTICE_PERIOD_LENGTHValue; 
 public System.Decimal NOTICE_PERIOD_LENGTH
        {  
            get  
            {  
                return this.NOTICE_PERIOD_LENGTHValue;  
            }  

          set { SetProperty(ref  NOTICE_PERIOD_LENGTHValue, value); }
        } 
private  System.String NOTICE_PERIOD_OUOMValue; 
 public System.String NOTICE_PERIOD_OUOM
        {  
            get  
            {  
                return this.NOTICE_PERIOD_OUOMValue;  
            }  

          set { SetProperty(ref  NOTICE_PERIOD_OUOMValue, value); }
        } 
private  System.String OBLIGATION_CATEGORYValue; 
 public System.String OBLIGATION_CATEGORY
        {  
            get  
            {  
                return this.OBLIGATION_CATEGORYValue;  
            }  

          set { SetProperty(ref  OBLIGATION_CATEGORYValue, value); }
        } 
private  System.Decimal OBLIGATION_DURATIONValue; 
 public System.Decimal OBLIGATION_DURATION
        {  
            get  
            {  
                return this.OBLIGATION_DURATIONValue;  
            }  

          set { SetProperty(ref  OBLIGATION_DURATIONValue, value); }
        } 
private  System.String OBLIGATION_DURATION_OUOMValue; 
 public System.String OBLIGATION_DURATION_OUOM
        {  
            get  
            {  
                return this.OBLIGATION_DURATION_OUOMValue;  
            }  

          set { SetProperty(ref  OBLIGATION_DURATION_OUOMValue, value); }
        } 
private  System.String OBLIGATION_FREQUENCYValue; 
 public System.String OBLIGATION_FREQUENCY
        {  
            get  
            {  
                return this.OBLIGATION_FREQUENCYValue;  
            }  

          set { SetProperty(ref  OBLIGATION_FREQUENCYValue, value); }
        } 
private  System.String OBLIGATION_TYPEValue; 
 public System.String OBLIGATION_TYPE
        {  
            get  
            {  
                return this.OBLIGATION_TYPEValue;  
            }  

          set { SetProperty(ref  OBLIGATION_TYPEValue, value); }
        } 
private  System.String PAYMENT_INDValue; 
 public System.String PAYMENT_IND
        {  
            get  
            {  
                return this.PAYMENT_INDValue;  
            }  

          set { SetProperty(ref  PAYMENT_INDValue, value); }
        } 
private  System.String PAYMENT_RESPONSIBILITYValue; 
 public System.String PAYMENT_RESPONSIBILITY
        {  
            get  
            {  
                return this.PAYMENT_RESPONSIBILITYValue;  
            }  

          set { SetProperty(ref  PAYMENT_RESPONSIBILITYValue, value); }
        } 
private  System.Decimal PERCENTAGEValue; 
 public System.Decimal PERCENTAGE
        {  
            get  
            {  
                return this.PERCENTAGEValue;  
            }  

          set { SetProperty(ref  PERCENTAGEValue, value); }
        } 
private  System.String POTENTIAL_OBLIGATION_DESCValue; 
 public System.String POTENTIAL_OBLIGATION_DESC
        {  
            get  
            {  
                return this.POTENTIAL_OBLIGATION_DESCValue;  
            }  

          set { SetProperty(ref  POTENTIAL_OBLIGATION_DESCValue, value); }
        } 
private  System.String POTENTIAL_OBLIGATION_INDValue; 
 public System.String POTENTIAL_OBLIGATION_IND
        {  
            get  
            {  
                return this.POTENTIAL_OBLIGATION_INDValue;  
            }  

          set { SetProperty(ref  POTENTIAL_OBLIGATION_INDValue, value); }
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
private  System.String PREPAID_INDValue; 
 public System.String PREPAID_IND
        {  
            get  
            {  
                return this.PREPAID_INDValue;  
            }  

          set { SetProperty(ref  PREPAID_INDValue, value); }
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
private  System.String RESP_PARTY_BA_IDValue; 
 public System.String RESP_PARTY_BA_ID
        {  
            get  
            {  
                return this.RESP_PARTY_BA_IDValue;  
            }  

          set { SetProperty(ref  RESP_PARTY_BA_IDValue, value); }
        } 
private  System.String REVIEW_FREQUENCYValue; 
 public System.String REVIEW_FREQUENCY
        {  
            get  
            {  
                return this.REVIEW_FREQUENCYValue;  
            }  

          set { SetProperty(ref  REVIEW_FREQUENCYValue, value); }
        } 
private  System.String ROYALTY_OWNER_BA_IDValue; 
 public System.String ROYALTY_OWNER_BA_ID
        {  
            get  
            {  
                return this.ROYALTY_OWNER_BA_IDValue;  
            }  

          set { SetProperty(ref  ROYALTY_OWNER_BA_IDValue, value); }
        } 
private  System.String ROYALTY_PAYOR_BA_IDValue; 
 public System.String ROYALTY_PAYOR_BA_ID
        {  
            get  
            {  
                return this.ROYALTY_PAYOR_BA_IDValue;  
            }  

          set { SetProperty(ref  ROYALTY_PAYOR_BA_IDValue, value); }
        } 
private  System.String ROYALTY_TYPEValue; 
 public System.String ROYALTY_TYPE
        {  
            get  
            {  
                return this.ROYALTY_TYPEValue;  
            }  

          set { SetProperty(ref  ROYALTY_TYPEValue, value); }
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
private  System.String SUBSTANCE_IDValue; 
 public System.String SUBSTANCE_ID
        {  
            get  
            {  
                return this.SUBSTANCE_IDValue;  
            }  

          set { SetProperty(ref  SUBSTANCE_IDValue, value); }
        } 
private  System.String TRIGGER_METHODValue; 
 public System.String TRIGGER_METHOD
        {  
            get  
            {  
                return this.TRIGGER_METHODValue;  
            }  

          set { SetProperty(ref  TRIGGER_METHODValue, value); }
        } 
private  System.String WORK_OBLIGATION_DESCValue; 
 public System.String WORK_OBLIGATION_DESC
        {  
            get  
            {  
                return this.WORK_OBLIGATION_DESCValue;  
            }  

          set { SetProperty(ref  WORK_OBLIGATION_DESCValue, value); }
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


    public OBLIGATION () { }

  }
}


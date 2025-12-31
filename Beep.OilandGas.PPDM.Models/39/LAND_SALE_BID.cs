using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class LAND_SALE_BID: Entity,IPPDMEntity

{

private  System.String JURISDICTIONValue; 
 public System.String JURISDICTION
        {  
            get  
            {  
                return this.JURISDICTIONValue;  
            }  

          set { SetProperty(ref  JURISDICTIONValue, value); }
        } 
private  System.String LAND_SALE_NUMBERValue; 
 public System.String LAND_SALE_NUMBER
        {  
            get  
            {  
                return this.LAND_SALE_NUMBERValue;  
            }  

          set { SetProperty(ref  LAND_SALE_NUMBERValue, value); }
        } 
private  System.String LAND_SALE_OFFERING_IDValue; 
 public System.String LAND_SALE_OFFERING_ID
        {  
            get  
            {  
                return this.LAND_SALE_OFFERING_IDValue;  
            }  

          set { SetProperty(ref  LAND_SALE_OFFERING_IDValue, value); }
        } 
private  System.String LAND_OFFERING_BID_IDValue; 
 public System.String LAND_OFFERING_BID_ID
        {  
            get  
            {  
                return this.LAND_OFFERING_BID_IDValue;  
            }  

          set { SetProperty(ref  LAND_OFFERING_BID_IDValue, value); }
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
private  System.String ADDRESS_FOR_SERVICEValue; 
 public System.String ADDRESS_FOR_SERVICE
        {  
            get  
            {  
                return this.ADDRESS_FOR_SERVICEValue;  
            }  

          set { SetProperty(ref  ADDRESS_FOR_SERVICEValue, value); }
        } 
private  System.Decimal ADDRESS_OBS_NOValue; 
 public System.Decimal ADDRESS_OBS_NO
        {  
            get  
            {  
                return this.ADDRESS_OBS_NOValue;  
            }  

          set { SetProperty(ref  ADDRESS_OBS_NOValue, value); }
        } 
private  System.String ADDRESS_SOURCEValue; 
 public System.String ADDRESS_SOURCE
        {  
            get  
            {  
                return this.ADDRESS_SOURCEValue;  
            }  

          set { SetProperty(ref  ADDRESS_SOURCEValue, value); }
        } 
private  System.String BIDDERValue; 
 public System.String BIDDER
        {  
            get  
            {  
                return this.BIDDERValue;  
            }  

          set { SetProperty(ref  BIDDERValue, value); }
        } 
private  System.String BIDDER_TYPEValue; 
 public System.String BIDDER_TYPE
        {  
            get  
            {  
                return this.BIDDER_TYPEValue;  
            }  

          set { SetProperty(ref  BIDDER_TYPEValue, value); }
        } 
private  System.String BID_STATUSValue; 
 public System.String BID_STATUS
        {  
            get  
            {  
                return this.BID_STATUSValue;  
            }  

          set { SetProperty(ref  BID_STATUSValue, value); }
        } 
private  System.DateTime? BID_STATUS_DATEValue; 
 public System.DateTime? BID_STATUS_DATE
        {  
            get  
            {  
                return this.BID_STATUS_DATEValue;  
            }  

          set { SetProperty(ref  BID_STATUS_DATEValue, value); }
        } 
private  System.DateTime? BID_SUBMIT_DATEValue; 
 public System.DateTime? BID_SUBMIT_DATE
        {  
            get  
            {  
                return this.BID_SUBMIT_DATEValue;  
            }  

          set { SetProperty(ref  BID_SUBMIT_DATEValue, value); }
        } 
private  System.String BID_TYPEValue; 
 public System.String BID_TYPE
        {  
            get  
            {  
                return this.BID_TYPEValue;  
            }  

          set { SetProperty(ref  BID_TYPEValue, value); }
        } 
private  System.String CASH_BID_TYPEValue; 
 public System.String CASH_BID_TYPE
        {  
            get  
            {  
                return this.CASH_BID_TYPEValue;  
            }  

          set { SetProperty(ref  CASH_BID_TYPEValue, value); }
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
private  System.String CONTINGENCY_DESCValue; 
 public System.String CONTINGENCY_DESC
        {  
            get  
            {  
                return this.CONTINGENCY_DESCValue;  
            }  

          set { SetProperty(ref  CONTINGENCY_DESCValue, value); }
        } 
private  System.String CONTINGENCY_INDValue; 
 public System.String CONTINGENCY_IND
        {  
            get  
            {  
                return this.CONTINGENCY_INDValue;  
            }  

          set { SetProperty(ref  CONTINGENCY_INDValue, value); }
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
private  System.String PAYMENT_INSTRUCTION_IDValue; 
 public System.String PAYMENT_INSTRUCTION_ID
        {  
            get  
            {  
                return this.PAYMENT_INSTRUCTION_IDValue;  
            }  

          set { SetProperty(ref  PAYMENT_INSTRUCTION_IDValue, value); }
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
private  System.Decimal PRIORITY_ORDERValue; 
 public System.Decimal PRIORITY_ORDER
        {  
            get  
            {  
                return this.PRIORITY_ORDERValue;  
            }  

          set { SetProperty(ref  PRIORITY_ORDERValue, value); }
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
private  System.String RESPONSEValue; 
 public System.String RESPONSE
        {  
            get  
            {  
                return this.RESPONSEValue;  
            }  

          set { SetProperty(ref  RESPONSEValue, value); }
        } 
private  System.DateTime? RESPONSE_DATEValue; 
 public System.DateTime? RESPONSE_DATE
        {  
            get  
            {  
                return this.RESPONSE_DATEValue;  
            }  

          set { SetProperty(ref  RESPONSE_DATEValue, value); }
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
private  System.String SUCCESSFUL_INDValue; 
 public System.String SUCCESSFUL_IND
        {  
            get  
            {  
                return this.SUCCESSFUL_INDValue;  
            }  

          set { SetProperty(ref  SUCCESSFUL_INDValue, value); }
        } 
private  System.String WORK_BID_INDValue; 
 public System.String WORK_BID_IND
        {  
            get  
            {  
                return this.WORK_BID_INDValue;  
            }  

          set { SetProperty(ref  WORK_BID_INDValue, value); }
        } 
private  System.String WORK_BID_REMARKValue; 
 public System.String WORK_BID_REMARK
        {  
            get  
            {  
                return this.WORK_BID_REMARKValue;  
            }  

          set { SetProperty(ref  WORK_BID_REMARKValue, value); }
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


    public LAND_SALE_BID () { }

  }
}


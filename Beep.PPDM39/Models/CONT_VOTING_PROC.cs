using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.PPDM39.Models 
{
public partial class CONT_VOTING_PROC: Entity

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
private  System.String VOTING_PROCEDURE_IDValue; 
 public System.String VOTING_PROCEDURE_ID
        {  
            get  
            {  
                return this.VOTING_PROCEDURE_IDValue;  
            }  

          set { SetProperty(ref  VOTING_PROCEDURE_IDValue, value); }
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
private  System.Decimal DEFEAT_COUNTValue; 
 public System.Decimal DEFEAT_COUNT
        {  
            get  
            {  
                return this.DEFEAT_COUNTValue;  
            }  

          set { SetProperty(ref  DEFEAT_COUNTValue, value); }
        } 
private  System.Decimal DEFEAT_PERCENTValue; 
 public System.Decimal DEFEAT_PERCENT
        {  
            get  
            {  
                return this.DEFEAT_PERCENTValue;  
            }  

          set { SetProperty(ref  DEFEAT_PERCENTValue, value); }
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
private  System.Decimal INTEREST_PERCENTValue; 
 public System.Decimal INTEREST_PERCENT
        {  
            get  
            {  
                return this.INTEREST_PERCENTValue;  
            }  

          set { SetProperty(ref  INTEREST_PERCENTValue, value); }
        } 
private  System.String NO_VOTE_RESPONSEValue; 
 public System.String NO_VOTE_RESPONSE
        {  
            get  
            {  
                return this.NO_VOTE_RESPONSEValue;  
            }  

          set { SetProperty(ref  NO_VOTE_RESPONSEValue, value); }
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
private  System.Decimal QUORUM_COUNTValue; 
 public System.Decimal QUORUM_COUNT
        {  
            get  
            {  
                return this.QUORUM_COUNTValue;  
            }  

          set { SetProperty(ref  QUORUM_COUNTValue, value); }
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
private  System.Decimal RESPONSE_PERIODValue; 
 public System.Decimal RESPONSE_PERIOD
        {  
            get  
            {  
                return this.RESPONSE_PERIODValue;  
            }  

          set { SetProperty(ref  RESPONSE_PERIODValue, value); }
        } 
private  System.String RESPONSE_PERIOD_UOMValue; 
 public System.String RESPONSE_PERIOD_UOM
        {  
            get  
            {  
                return this.RESPONSE_PERIOD_UOMValue;  
            }  

          set { SetProperty(ref  RESPONSE_PERIOD_UOMValue, value); }
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
private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.String VOTE_PROCEDURE_TYPEValue; 
 public System.String VOTE_PROCEDURE_TYPE
        {  
            get  
            {  
                return this.VOTE_PROCEDURE_TYPEValue;  
            }  

          set { SetProperty(ref  VOTE_PROCEDURE_TYPEValue, value); }
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


    public CONT_VOTING_PROC () { }

  }
}


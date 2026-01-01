using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_RISK_ASSESSMENT: Entity,IPPDMEntity

{

private  System.String PROSPECT_IDValue; 
 public System.String PROSPECT_ID
        {  
            get  
            {  
                return this.PROSPECT_IDValue;  
            }  

          set { SetProperty(ref  PROSPECT_IDValue, value); }
        } 
private  System.String ASSESSMENT_IDValue; 
 public System.String ASSESSMENT_ID
        {  
            get  
            {  
                return this.ASSESSMENT_IDValue;  
            }  

          set { SetProperty(ref  ASSESSMENT_IDValue, value); }
        } 
private  System.String RISK_MODEL_TYPEValue; 
 public System.String RISK_MODEL_TYPE
        {  
            get  
            {  
                return this.RISK_MODEL_TYPEValue;  
            }  

          set { SetProperty(ref  RISK_MODEL_TYPEValue, value); }
        } 
private  System.Decimal? TRAP_RISKValue; 
 public System.Decimal? TRAP_RISK
        {  
            get  
            {  
                return this.TRAP_RISKValue;  
            }  

          set { SetProperty(ref  TRAP_RISKValue, value); }
        } 
private  System.Decimal? RESERVOIR_RISKValue; 
 public System.Decimal? RESERVOIR_RISK
        {  
            get  
            {  
                return this.RESERVOIR_RISKValue;  
            }  

          set { SetProperty(ref  RESERVOIR_RISKValue, value); }
        } 
private  System.Decimal? SEAL_RISKValue; 
 public System.Decimal? SEAL_RISK
        {  
            get  
            {  
                return this.SEAL_RISKValue;  
            }  

          set { SetProperty(ref  SEAL_RISKValue, value); }
        } 
private  System.Decimal? SOURCE_RISKValue; 
 public System.Decimal? SOURCE_RISK
        {  
            get  
            {  
                return this.SOURCE_RISKValue;  
            }  

          set { SetProperty(ref  SOURCE_RISKValue, value); }
        } 
private  System.Decimal? TIMING_RISKValue; 
 public System.Decimal? TIMING_RISK
        {  
            get  
            {  
                return this.TIMING_RISKValue;  
            }  

          set { SetProperty(ref  TIMING_RISKValue, value); }
        } 
private  System.Decimal? OVERALL_GEOLOGICAL_RISKValue; 
 public System.Decimal? OVERALL_GEOLOGICAL_RISK
        {  
            get  
            {  
                return this.OVERALL_GEOLOGICAL_RISKValue;  
            }  

          set { SetProperty(ref  OVERALL_GEOLOGICAL_RISKValue, value); }
        } 
private  System.Decimal? LOW_ESTIMATE_OILValue; 
 public System.Decimal? LOW_ESTIMATE_OIL
        {  
            get  
            {  
                return this.LOW_ESTIMATE_OILValue;  
            }  

          set { SetProperty(ref  LOW_ESTIMATE_OILValue, value); }
        } 
private  System.Decimal? BEST_ESTIMATE_OILValue; 
 public System.Decimal? BEST_ESTIMATE_OIL
        {  
            get  
            {  
                return this.BEST_ESTIMATE_OILValue;  
            }  

          set { SetProperty(ref  BEST_ESTIMATE_OILValue, value); }
        } 
private  System.Decimal? HIGH_ESTIMATE_OILValue; 
 public System.Decimal? HIGH_ESTIMATE_OIL
        {  
            get  
            {  
                return this.HIGH_ESTIMATE_OILValue;  
            }  

          set { SetProperty(ref  HIGH_ESTIMATE_OILValue, value); }
        } 
private  System.Decimal? LOW_ESTIMATE_GASValue; 
 public System.Decimal? LOW_ESTIMATE_GAS
        {  
            get  
            {  
                return this.LOW_ESTIMATE_GASValue;  
            }  

          set { SetProperty(ref  LOW_ESTIMATE_GASValue, value); }
        } 
private  System.Decimal? BEST_ESTIMATE_GASValue; 
 public System.Decimal? BEST_ESTIMATE_GAS
        {  
            get  
            {  
                return this.BEST_ESTIMATE_GASValue;  
            }  

          set { SetProperty(ref  BEST_ESTIMATE_GASValue, value); }
        } 
private  System.Decimal? HIGH_ESTIMATE_GASValue; 
 public System.Decimal? HIGH_ESTIMATE_GAS
        {  
            get  
            {  
                return this.HIGH_ESTIMATE_GASValue;  
            }  

          set { SetProperty(ref  HIGH_ESTIMATE_GASValue, value); }
        } 
private  System.Decimal? OIL_PRICEValue; 
 public System.Decimal? OIL_PRICE
        {  
            get  
            {  
                return this.OIL_PRICEValue;  
            }  

          set { SetProperty(ref  OIL_PRICEValue, value); }
        } 
private  System.Decimal? GAS_PRICEValue; 
 public System.Decimal? GAS_PRICE
        {  
            get  
            {  
                return this.GAS_PRICEValue;  
            }  

          set { SetProperty(ref  GAS_PRICEValue, value); }
        } 
private  System.Decimal? DEVELOPMENT_COSTValue; 
 public System.Decimal? DEVELOPMENT_COST
        {  
            get  
            {  
                return this.DEVELOPMENT_COSTValue;  
            }  

          set { SetProperty(ref  DEVELOPMENT_COSTValue, value); }
        } 
private  System.Decimal? RISKED_OIL_VOLUMEValue; 
 public System.Decimal? RISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.RISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? RISKED_GAS_VOLUMEValue; 
 public System.Decimal? RISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.RISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  RISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal? UNRISKED_OIL_VOLUMEValue; 
 public System.Decimal? UNRISKED_OIL_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_OIL_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_OIL_VOLUMEValue, value); }
        } 
private  System.Decimal? UNRISKED_GAS_VOLUMEValue; 
 public System.Decimal? UNRISKED_GAS_VOLUME
        {  
            get  
            {  
                return this.UNRISKED_GAS_VOLUMEValue;  
            }  

          set { SetProperty(ref  UNRISKED_GAS_VOLUMEValue, value); }
        } 
private  System.Decimal? EMVValue; 
 public System.Decimal? EMV
        {  
            get  
            {  
                return this.EMVValue;  
            }  

          set { SetProperty(ref  EMVValue, value); }
        } 
private  System.Decimal? NPVValue; 
 public System.Decimal? NPV
        {  
            get  
            {  
                return this.NPVValue;  
            }  

          set { SetProperty(ref  NPVValue, value); }
        } 
private  System.String RISK_CATEGORYValue; 
 public System.String RISK_CATEGORY
        {  
            get  
            {  
                return this.RISK_CATEGORYValue;  
            }  

          set { SetProperty(ref  RISK_CATEGORYValue, value); }
        } 
private  System.String RISK_FACTORSValue; 
 public System.String RISK_FACTORS
        {  
            get  
            {  
                return this.RISK_FACTORSValue;  
            }  

          set { SetProperty(ref  RISK_FACTORSValue, value); }
        } 
private  System.String RECOMMENDATIONSValue; 
 public System.String RECOMMENDATIONS
        {  
            get  
            {  
                return this.RECOMMENDATIONSValue;  
            }  

          set { SetProperty(ref  RECOMMENDATIONSValue, value); }
        } 
private  System.DateTime? ASSESSMENT_DATEValue; 
 public System.DateTime? ASSESSMENT_DATE
        {  
            get  
            {  
                return this.ASSESSMENT_DATEValue;  
            }  

          set { SetProperty(ref  ASSESSMENT_DATEValue, value); }
        } 
private  System.String ASSESSORValue; 
 public System.String ASSESSOR
        {  
            get  
            {  
                return this.ASSESSORValue;  
            }  

          set { SetProperty(ref  ASSESSORValue, value); }
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
private  System.String SOURCEValue; 
 public System.String SOURCE
        {  
            get  
            {  
                return this.SOURCEValue;  
            }  

          set { SetProperty(ref  SOURCEValue, value); }
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


    public PROSPECT_RISK_ASSESSMENT () { }

  }
}

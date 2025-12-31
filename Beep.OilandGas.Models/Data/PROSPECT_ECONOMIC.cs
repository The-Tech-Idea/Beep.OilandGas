using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.Models.Data 
{
public partial class PROSPECT_ECONOMIC: Entity,IPPDMEntity

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
private  System.String ECONOMIC_IDValue; 
 public System.String ECONOMIC_ID
        {  
            get  
            {  
                return this.ECONOMIC_IDValue;  
            }  

          set { SetProperty(ref  ECONOMIC_IDValue, value); }
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
private  System.Decimal? EMVValue; 
 public System.Decimal? EMV
        {  
            get  
            {  
                return this.EMVValue;  
            }  

          set { SetProperty(ref  EMVValue, value); }
        } 
private  System.Decimal? IRRValue; 
 public System.Decimal? IRR
        {  
            get  
            {  
                return this.IRRValue;  
            }  

          set { SetProperty(ref  IRRValue, value); }
        } 
private  System.Decimal? PAYBACK_PERIODValue; 
 public System.Decimal? PAYBACK_PERIOD
        {  
            get  
            {  
                return this.PAYBACK_PERIODValue;  
            }  

          set { SetProperty(ref  PAYBACK_PERIODValue, value); }
        } 
private  System.String PAYBACK_PERIOD_OUOMValue; 
 public System.String PAYBACK_PERIOD_OUOM
        {  
            get  
            {  
                return this.PAYBACK_PERIOD_OUOMValue;  
            }  

          set { SetProperty(ref  PAYBACK_PERIOD_OUOMValue, value); }
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
private  System.Decimal? OPERATING_COSTValue; 
 public System.Decimal? OPERATING_COST
        {  
            get  
            {  
                return this.OPERATING_COSTValue;  
            }  

          set { SetProperty(ref  OPERATING_COSTValue, value); }
        } 
private  System.Decimal? ABANDONMENT_COSTValue; 
 public System.Decimal? ABANDONMENT_COST
        {  
            get  
            {  
                return this.ABANDONMENT_COSTValue;  
            }  

          set { SetProperty(ref  ABANDONMENT_COSTValue, value); }
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
private  System.Decimal? DISCOUNT_RATEValue; 
 public System.Decimal? DISCOUNT_RATE
        {  
            get  
            {  
                return this.DISCOUNT_RATEValue;  
            }  

          set { SetProperty(ref  DISCOUNT_RATEValue, value); }
        } 
private  System.Decimal? INFLATION_RATEValue; 
 public System.Decimal? INFLATION_RATE
        {  
            get  
            {  
                return this.INFLATION_RATEValue;  
            }  

          set { SetProperty(ref  INFLATION_RATEValue, value); }
        } 
private  System.String ECONOMIC_SCENARIOValue; 
 public System.String ECONOMIC_SCENARIO
        {  
            get  
            {  
                return this.ECONOMIC_SCENARIOValue;  
            }  

          set { SetProperty(ref  ECONOMIC_SCENARIOValue, value); }
        } 
private  System.DateTime? EVALUATION_DATEValue; 
 public System.DateTime? EVALUATION_DATE
        {  
            get  
            {  
                return this.EVALUATION_DATEValue;  
            }  

          set { SetProperty(ref  EVALUATION_DATEValue, value); }
        } 
private  System.String EVALUATORValue; 
 public System.String EVALUATOR
        {  
            get  
            {  
                return this.EVALUATORValue;  
            }  

          set { SetProperty(ref  EVALUATORValue, value); }
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


    public PROSPECT_ECONOMIC () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
 namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_ECONOMIC : ModelEntityBase

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
private  System.DateTime? EFFECTIVE_DATEValue; 

private  System.DateTime? EXPIRY_DATEValue; 

private  System.String REMARKValue; 

private  System.String SOURCEValue; 

    public PROSPECT_ECONOMIC () { }

  }
}



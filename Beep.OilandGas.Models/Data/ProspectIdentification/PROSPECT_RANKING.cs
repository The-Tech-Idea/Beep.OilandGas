using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
public partial class PROSPECT_RANKING : ModelEntityBase

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
private  System.String RANKING_IDValue; 
 public System.String RANKING_ID
        {  
            get  
            {  
                return this.RANKING_IDValue;  
            }  

          set { SetProperty(ref  RANKING_IDValue, value); }
        } 
private  System.Decimal? RANKING_SCOREValue; 
 public System.Decimal? RANKING_SCORE
        {  
            get  
            {  
                return this.RANKING_SCOREValue;  
            }  

          set { SetProperty(ref  RANKING_SCOREValue, value); }
        } 
private  System.Decimal? PRIORITY_ORDERValue; 
 public System.Decimal? PRIORITY_ORDER
        {  
            get  
            {  
                return this.PRIORITY_ORDERValue;  
            }  

          set { SetProperty(ref  PRIORITY_ORDERValue, value); }
        } 
private  System.String RANKING_CRITERIAValue; 
 public System.String RANKING_CRITERIA
        {  
            get  
            {  
                return this.RANKING_CRITERIAValue;  
            }  

          set { SetProperty(ref  RANKING_CRITERIAValue, value); }
        } 
private  System.DateTime? RANKING_DATEValue; 
 public System.DateTime? RANKING_DATE
        {  
            get  
            {  
                return this.RANKING_DATEValue;  
            }  

          set { SetProperty(ref  RANKING_DATEValue, value); }
        } 
private  System.String RANKERValue; 
 public System.String RANKER
        {  
            get  
            {  
                return this.RANKERValue;  
            }  

          set { SetProperty(ref  RANKERValue, value); }
        } 
private  System.String METHODOLOGYValue; 
 public System.String METHODOLOGY
        {  
            get  
            {  
                return this.METHODOLOGYValue;  
            }  

          set { SetProperty(ref  METHODOLOGYValue, value); }
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

    public PROSPECT_RANKING () { }

  }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class WELL_HORIZ_DRILL_SPOKE: Entity,IPPDMEntity

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
private  System.Decimal KICKOFF_POINT_OBS_NOValue; 
 public System.Decimal KICKOFF_POINT_OBS_NO
        {  
            get  
            {  
                return this.KICKOFF_POINT_OBS_NOValue;  
            }  

          set { SetProperty(ref  KICKOFF_POINT_OBS_NOValue, value); }
        } 
private  System.Decimal SPOKE_OBS_NOValue; 
 public System.Decimal SPOKE_OBS_NO
        {  
            get  
            {  
                return this.SPOKE_OBS_NOValue;  
            }  

          set { SetProperty(ref  SPOKE_OBS_NOValue, value); }
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
private  System.String LATERAL_HOLE_IDValue; 
 public System.String LATERAL_HOLE_ID
        {  
            get  
            {  
                return this.LATERAL_HOLE_IDValue;  
            }  

          set { SetProperty(ref  LATERAL_HOLE_IDValue, value); }
        } 
private  System.String NODE_IDValue; 
 public System.String NODE_ID
        {  
            get  
            {  
                return this.NODE_IDValue;  
            }  

          set { SetProperty(ref  NODE_IDValue, value); }
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
private  System.Decimal SPOKE_LENGTHValue; 
 public System.Decimal SPOKE_LENGTH
        {  
            get  
            {  
                return this.SPOKE_LENGTHValue;  
            }  

          set { SetProperty(ref  SPOKE_LENGTHValue, value); }
        } 
private  System.String SPOKE_LENGTH_OUOMValue; 
 public System.String SPOKE_LENGTH_OUOM
        {  
            get  
            {  
                return this.SPOKE_LENGTH_OUOMValue;  
            }  

          set { SetProperty(ref  SPOKE_LENGTH_OUOMValue, value); }
        } 
private  System.Decimal SPOKE_MDValue; 
 public System.Decimal SPOKE_MD
        {  
            get  
            {  
                return this.SPOKE_MDValue;  
            }  

          set { SetProperty(ref  SPOKE_MDValue, value); }
        } 
private  System.String SPOKE_MD_OUOMValue; 
 public System.String SPOKE_MD_OUOM
        {  
            get  
            {  
                return this.SPOKE_MD_OUOMValue;  
            }  

          set { SetProperty(ref  SPOKE_MD_OUOMValue, value); }
        } 
private  System.Decimal SPOKE_TVDValue; 
 public System.Decimal SPOKE_TVD
        {  
            get  
            {  
                return this.SPOKE_TVDValue;  
            }  

          set { SetProperty(ref  SPOKE_TVDValue, value); }
        } 
private  System.String SPOKE_TVD_OUOMValue; 
 public System.String SPOKE_TVD_OUOM
        {  
            get  
            {  
                return this.SPOKE_TVD_OUOMValue;  
            }  

          set { SetProperty(ref  SPOKE_TVD_OUOMValue, value); }
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


    public WELL_HORIZ_DRILL_SPOKE () { }

  }
}


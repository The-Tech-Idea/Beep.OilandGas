using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

 namespace Beep.OilandGas.PPDM39.Models 
{
public partial class SOURCE_DOCUMENT: Entity,IPPDMEntity

{

private  System.String SOURCE_DOCUMENT_IDValue; 
 public System.String SOURCE_DOCUMENT_ID
        {  
            get  
            {  
                return this.SOURCE_DOCUMENT_IDValue;  
            }  

          set { SetProperty(ref  SOURCE_DOCUMENT_IDValue, value); }
        } 
private  System.String ABBREVIATIONValue; 
 public System.String ABBREVIATION
        {  
            get  
            {  
                return this.ABBREVIATIONValue;  
            }  

          set { SetProperty(ref  ABBREVIATIONValue, value); }
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
private  System.String DOCUMENT_TITLEValue; 
 public System.String DOCUMENT_TITLE
        {  
            get  
            {  
                return this.DOCUMENT_TITLEValue;  
            }  

          set { SetProperty(ref  DOCUMENT_TITLEValue, value); }
        } 
private  System.String DOCUMENT_TYPEValue; 
 public System.String DOCUMENT_TYPE
        {  
            get  
            {  
                return this.DOCUMENT_TYPEValue;  
            }  

          set { SetProperty(ref  DOCUMENT_TYPEValue, value); }
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
private  System.String FIGURE_REFERENCEValue; 
 public System.String FIGURE_REFERENCE
        {  
            get  
            {  
                return this.FIGURE_REFERENCEValue;  
            }  

          set { SetProperty(ref  FIGURE_REFERENCEValue, value); }
        } 
private  System.String ISSUEValue; 
 public System.String ISSUE
        {  
            get  
            {  
                return this.ISSUEValue;  
            }  

          set { SetProperty(ref  ISSUEValue, value); }
        } 
private  System.String LANGUAGEValue; 
 public System.String LANGUAGE
        {  
            get  
            {  
                return this.LANGUAGEValue;  
            }  

          set { SetProperty(ref  LANGUAGEValue, value); }
        } 
private  System.String PAGE_REFERENCEValue; 
 public System.String PAGE_REFERENCE
        {  
            get  
            {  
                return this.PAGE_REFERENCEValue;  
            }  

          set { SetProperty(ref  PAGE_REFERENCEValue, value); }
        } 
private  System.String PLATE_REFERENCEValue; 
 public System.String PLATE_REFERENCE
        {  
            get  
            {  
                return this.PLATE_REFERENCEValue;  
            }  

          set { SetProperty(ref  PLATE_REFERENCEValue, value); }
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
private  System.String PUBLICATIONValue; 
 public System.String PUBLICATION
        {  
            get  
            {  
                return this.PUBLICATIONValue;  
            }  

          set { SetProperty(ref  PUBLICATIONValue, value); }
        } 
private  System.DateTime? PUBLICATION_DATEValue; 
 public System.DateTime? PUBLICATION_DATE
        {  
            get  
            {  
                return this.PUBLICATION_DATEValue;  
            }  

          set { SetProperty(ref  PUBLICATION_DATEValue, value); }
        } 
private  System.Decimal PUBLICATION_YEARValue; 
 public System.Decimal PUBLICATION_YEAR
        {  
            get  
            {  
                return this.PUBLICATION_YEARValue;  
            }  

          set { SetProperty(ref  PUBLICATION_YEARValue, value); }
        } 
private  System.String PUBLISHERValue; 
 public System.String PUBLISHER
        {  
            get  
            {  
                return this.PUBLISHERValue;  
            }  

          set { SetProperty(ref  PUBLISHERValue, value); }
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


    public SOURCE_DOCUMENT () { }

  }
}


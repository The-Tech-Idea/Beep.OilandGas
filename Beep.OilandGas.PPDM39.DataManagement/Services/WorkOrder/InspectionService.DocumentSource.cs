using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class InspectionService : ICompanyWorkOrderPermitDocumentSourceOwner
{
    private async Task<WORK_ORDER_COMPONENT[]> ReadDocumentLinksAsync(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,Func<Task> authority,bool owned=false,bool completeGuard=false)
    {
        permit.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(context.FieldId!=permit.Task.FieldId)throw new UnauthorizedAccessException("Original document FIELD differs.");
        async Task Guard(){await authority();if(!completeGuard){await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,permit.Task.FieldId,permit.Task.WorkOrderId,"WRITE");}}
        await Guard();var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original document core source is unavailable.");RequireDocumentSourceOwnership(source,owned);
        var repo=await DocumentRepository(typeof(WORK_ORDER_COMPONENT),["WORK_ORDER_ID","COMPONENT_ID"],source,context);
        var raw=(await repo.GetAsync([new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=permit.Task.WorkOrderId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=permit.Task.FieldId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
        if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original document datasource changed.");RequireDocumentSourceOwnership(source,owned);
        if(raw.Length>4096 || raw.Any(value=>value is not WORK_ORDER_COMPONENT row || row.WORK_ORDER_ID!=permit.Task.WorkOrderId || row.FIELD_ID!=permit.Task.FieldId || row.ACTIVE_IND!="Y" || !LeaseSummaryValidator.Identifier(row.COMPONENT_ID)))throw new UnauthorizedAccessException("Complete original order/FIELD component membership is unavailable or foreign.");
        foreach(var row in raw.Cast<WORK_ORDER_COMPONENT>().Where(row=>!string.IsNullOrWhiteSpace(row.INFORMATION_ITEM_ID)))RequireDocumentDateKinds(row.EFFECTIVE_DATE,row.EXPIRY_DATE);
        var rows=raw.Cast<WORK_ORDER_COMPONENT>().Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.COMPONENT_ID,StringComparer.Ordinal).ToArray();if(rows.Select(row=>row.COMPONENT_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Recorded component membership is ambiguous.");
        foreach(var row in rows)if(string.IsNullOrWhiteSpace(row.INFORMATION_ITEM_ID)!=string.IsNullOrWhiteSpace(row.INFO_ITEM_SUBTYPE))throw new InvalidOperationException("Recorded document component has an incomplete information-item key.");return rows;
    }
    private async Task<PPDMGenericRepository> DocumentRepository(Type type,string[] keys,TheTechIdea.Beep.IDataSource source,FieldSelectionReview context)
    {
        var table=type.Name;if(!source.CheckEntityExist(table))throw new InvalidOperationException("Recorded document source schema is unavailable.");var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var actual=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(actual is null || actual.Any(field=>field.AllowDBNull) || !actual.Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Complete native document composite keys are unavailable.");
        var definition=await metadata.GetTableMetadataAsync(table);
        if(definition is null || definition.EntityTypeName is not {} name || name!=type.FullName && name!=type.Name || !definition.PrimaryKeyColumn.Split(',').Select(key=>key.Trim()).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Canonical document source metadata is unavailable.");
        var expected=table switch
        {
            "WORK_ORDER_COMPONENT"=>new[]{("WORK_ORDER_ID","WORK_ORDER","WORK_ORDER_ID"),("FIELD_ID","FIELD","FIELD_ID"),("INFORMATION_ITEM_ID","RM_INFORMATION_ITEM","INFORMATION_ITEM_ID"),("INFO_ITEM_SUBTYPE","RM_INFORMATION_ITEM","INFO_ITEM_SUBTYPE")},
            "RM_INFORMATION_ITEM"=>new[]{("SOURCE_DOCUMENT_ID","SOURCE_DOCUMENT","SOURCE_DOCUMENT_ID")},
            "RM_CREATOR"=>new[]{("INFORMATION_ITEM_ID","RM_INFORMATION_ITEM","INFORMATION_ITEM_ID"),("INFO_ITEM_SUBTYPE","RM_INFORMATION_ITEM","INFO_ITEM_SUBTYPE"),("CREATOR_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")},
            "RM_FILE_CONTENT"=>new[]{("INFORMATION_ITEM_ID","RM_INFORMATION_ITEM","INFORMATION_ITEM_ID"),("INFO_ITEM_SUBTYPE","RM_INFORMATION_ITEM","INFO_ITEM_SUBTYPE")},
            _=>Array.Empty<(string,string,string)>()
        };
        foreach(var (column,parent,key) in expected){var relationships=definition.ForeignKeys.Where(value=>value.ForeignKeyColumn==column).ToArray();if(relationships.Length!=1 || relationships[0].ReferencedTable!=parent || relationships[0].ReferencedPrimaryKey!=key)throw new InvalidOperationException("Complete unambiguous canonical document relationship metadata is unavailable.");}
        return new(editor,columns,defaults,metadata,type,context.Database.ConnectionName,table);
    }
    private static void RequireDocumentSourceOwnership(TheTechIdea.Beep.IDataSource source,bool owned)
    {if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
    private static string DocumentDigest(object value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value));
    private static void RequireDocumentDateKinds(params DateTime?[] dates)
    {if(dates.Any(date=>date is {} value && value.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Native document date time zone is unconfirmed; cloning cannot establish UTC provenance.");}
    private static CompanyWorkOrderDocumentCatalog DocumentCatalog(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,WORK_ORDER_COMPONENT[] rows)
    {
        var result=new CompanyWorkOrderDocumentCatalog(context,permit,rows.Where(row=>!string.IsNullOrWhiteSpace(row.INFORMATION_ITEM_ID)).Select(row=>new CompanyWorkOrderDocumentLink(row.COMPONENT_ID,row.INFORMATION_ITEM_ID,row.INFO_ITEM_SUBTYPE,row.EFFECTIVE_DATE,row.EXPIRY_DATE,DocumentDigest(row))).ToArray(),DocumentDigest(rows),DateTime.UtcNow);result.Require(context.Company,context.ActorUserId,permit);return result;
    }
    public async Task<CompanyWorkOrderDocumentCatalog> ReadPermitDocumentCatalogAsync(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,Func<Task> requireAuthority)
    {
        var first=await ReadDocumentLinksAsync(context,permit,requireAuthority);var closing=await ReadDocumentLinksAsync(context,permit,requireAuthority);if(DocumentDigest(first)!=DocumentDigest(closing))throw new InvalidOperationException("Complete original document component population changed.");return DocumentCatalog(context,permit,closing);
    }
    public Task<CompanyWorkOrderPermitDocumentFacts> ReadPermitDocumentSourceAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> requireAuthority)=>ReadDocumentSourceAsync(context,query,expectedReference,requireAuthority,null);
    public async Task<CompanyWorkOrderPermitDocumentContent> ReadPermitDocumentContentAsync(CompanyWorkOrderPermitDocumentReview original,Func<Task> requireAuthority)
    {
        var review=CompanyInventorySourceReview.Copy(original);if(review is null)throw new InvalidOperationException("Original document review is unavailable.");review.Require();var context=review.PermitReview.Context;review.Require(context.Company,context.ActorUserId,review.Source.Query);
        if(review.Blockers.Count!=1 || review.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker)throw new InvalidOperationException("Confirmed unrestricted document evidence is required before content delivery.");
        byte[]? bytes=null;var source=await ReadDocumentSourceAsync(context,review.Source.Query,review.Source.ExpectedReference,requireAuthority,value=>bytes=value);
        var closing=review with{Source=source,Blockers=CompanyWorkOrderPermitDocumentReview.GetBlockers(review.PermitReview.Proposal,source)};
        var result=new CompanyWorkOrderPermitDocumentContent(closing,bytes??[]);result.Require(review);return result;
    }
    private async Task<CompanyWorkOrderPermitDocumentFacts> ReadDocumentSourceAsync(FieldSelectionReview context,CompanyWorkOrderPermitDocumentQuery query,string expectedReference,Func<Task> requireAuthority,Action<byte[]>? deliver,bool owned=false,bool completeGuard=false,bool outerPopulationClosure=false)
    {
        query.Require();if(!LeaseSummaryValidator.Identifier(expectedReference))throw new InvalidOperationException("Exact original permit source reference is unavailable.");var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original document source is unavailable.");
        async Task Guard(){await requireAuthority();if(!completeGuard){await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,query.Permit.Task.FieldId,query.Permit.Task.WorkOrderId,"WRITE");}if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original document target changed.");RequireDocumentSourceOwnership(source,owned);}
        var firstLinks=await ReadDocumentLinksAsync(context,query.Permit,Guard,owned,true);var link=firstLinks.SingleOrDefault(row=>row.COMPONENT_ID==query.ComponentId && !string.IsNullOrWhiteSpace(row.INFORMATION_ITEM_ID))??throw new UnauthorizedAccessException("Selected document is not in the complete authorized order/FIELD component population.");
        if(link.EFFECTIVE_DATE is not {} from || link.EXPIRY_DATE is not {} to || from.Kind!=DateTimeKind.Utc || to.Kind!=DateTimeKind.Utc || from>DateTime.UtcNow || to<=DateTime.UtcNow)throw new UnauthorizedAccessException("Recorded document membership effective UTC is unconfirmed or outside its current window; private reads are withheld.");
        async Task<(RM_INFORMATION_ITEM? Info,SOURCE_DOCUMENT? Document,RM_FILE_CONTENT? File)> Read()
        {
            await Guard();var links=await ReadDocumentLinksAsync(context,query.Permit,Guard,owned,true);if(DocumentDigest(links)!=DocumentDigest(firstLinks))throw new InvalidOperationException("Document membership changed before private reads.");
            var raw=(await (await DocumentRepository(typeof(RM_INFORMATION_ITEM),["INFO_ITEM_SUBTYPE","INFORMATION_ITEM_ID"],source,context)).GetAsync([new(){FieldName="INFORMATION_ITEM_ID",Operator="=",FilterValue=link.INFORMATION_ITEM_ID},new(){FieldName="INFO_ITEM_SUBTYPE",Operator="=",FilterValue=link.INFO_ITEM_SUBTYPE},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(raw.Length>1 || raw.Any(value=>value is not RM_INFORMATION_ITEM info || info.INFORMATION_ITEM_ID!=link.INFORMATION_ITEM_ID || info.INFO_ITEM_SUBTYPE!=link.INFO_ITEM_SUBTYPE || info.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Linked document information item is ambiguous or foreign.");if(raw.Length==0)return(null,null,null);var originalInfo=(RM_INFORMATION_ITEM)raw[0];RequireDocumentDateKinds(originalInfo.EFFECTIVE_DATE,originalInfo.EXPIRY_DATE);var info=CompanyInventorySourceReview.Copy(originalInfo);
            if(info.SOURCE_DOCUMENT_ID!=expectedReference)throw new UnauthorizedAccessException("Linked information item does not identify the exact original permit document reference.");
            // Unmapped access/use/security conditions never grant private document or content access.
            if(!string.IsNullOrWhiteSpace(info.ACCESS_CONDITION) || !string.IsNullOrWhiteSpace(info.USE_CONDITION) || !string.IsNullOrWhiteSpace(info.SECURITY_DESC))return(info,null,null);
            var documents=(await (await DocumentRepository(typeof(SOURCE_DOCUMENT),["SOURCE_DOCUMENT_ID"],source,context)).GetAsync([new(){FieldName="SOURCE_DOCUMENT_ID",Operator="=",FilterValue=expectedReference},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(documents.Length>1 || documents.Any(value=>value is not SOURCE_DOCUMENT doc || doc.SOURCE_DOCUMENT_ID!=expectedReference || doc.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Referenced source document is ambiguous or foreign.");if(documents.Length==0)return(info,null,null);var originalDocument=(SOURCE_DOCUMENT)documents[0];RequireDocumentDateKinds(originalDocument.PUBLICATION_DATE,originalDocument.EFFECTIVE_DATE,originalDocument.EXPIRY_DATE);var document=CompanyInventorySourceReview.Copy(originalDocument);
            var files=(await (await DocumentRepository(typeof(RM_FILE_CONTENT),["FILE_ID"],source,context)).GetAsync([new(){FieldName="INFORMATION_ITEM_ID",Operator="=",FilterValue=link.INFORMATION_ITEM_ID},new(){FieldName="INFO_ITEM_SUBTYPE",Operator="=",FilterValue=link.INFO_ITEM_SUBTYPE},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(files.Length>1 || files.Any(value=>value is not RM_FILE_CONTENT file || file.INFORMATION_ITEM_ID!=link.INFORMATION_ITEM_ID || file.INFO_ITEM_SUBTYPE!=link.INFO_ITEM_SUBTYPE || file.ACTIVE_IND!="Y" || !LeaseSummaryValidator.Identifier(file.FILE_ID)))throw new UnauthorizedAccessException("Linked document file population is ambiguous or foreign.");var originalFile=files.Length==0?null:(RM_FILE_CONTENT)files[0];if(originalFile is not null)RequireDocumentDateKinds(originalFile.EFFECTIVE_DATE,originalFile.EXPIRY_DATE);if(originalFile?.FILE_CONTENT?.Length>CompanyWorkOrderPermitDocumentFacts.MaximumContentBytes)throw new InvalidOperationException("Recorded document content exceeds its bounded review capacity.");var result=originalFile is null?null:CompanyInventorySourceReview.Copy(originalFile);return(info,document,result);
        }
        // The contributor owner compares complete first/closing observations around its own population reads.
        // Standalone source/content calls always retain their independent repeated read.
        var first=await Read();var closing=outerPopulationClosure?first:await Read();var closingLinks=await ReadDocumentLinksAsync(context,query.Permit,Guard,owned,true);await Guard();
        if(DocumentDigest(firstLinks)!=DocumentDigest(closingLinks) || DocumentDigest(new{first.Info,first.Document,first.File})!=DocumentDigest(new{closing.Info,closing.Document,closing.File}))throw new InvalidOperationException("Complete original document membership/version/header/content changed during review.");
        var restricted=closing.Info is {} i && (!string.IsNullOrWhiteSpace(i.ACCESS_CONDITION) || !string.IsNullOrWhiteSpace(i.USE_CONDITION) || !string.IsNullOrWhiteSpace(i.SECURITY_DESC));
        var information=closing.Info is {} item?new CompanyPermitInformationItemFacts(item.INFORMATION_ITEM_ID,item.INFO_ITEM_SUBTYPE,item.SOURCE_DOCUMENT_ID,restricted?null:item.TITLE,restricted?null:item.VERSION_NUM,restricted?"UNMAPPED RESTRICTION":null,null,item.EFFECTIVE_DATE,item.EXPIRY_DATE,DocumentDigest(item)):null;
        var docFacts=closing.Document is {} d?new CompanyPermitSourceDocumentFacts(d.SOURCE_DOCUMENT_ID,d.DOCUMENT_TITLE,d.DOCUMENT_TYPE,d.ISSUE,d.LANGUAGE,d.PUBLISHER,d.PUBLICATION_DATE,d.EFFECTIVE_DATE,d.EXPIRY_DATE,d.ROW_CREATED_BY,DocumentDigest(d)):null;
        var fileFacts=closing.File is {} f?new CompanyPermitFileFacts(f.FILE_ID,f.INFORMATION_ITEM_ID,f.INFO_ITEM_SUBTYPE,f.DIGITAL_FORMAT,f.FILE_CONTENT?.LongLength??0,f.FILE_SIZE,f.FILE_SIZE_UOM,f.FILE_CONTENT is {Length:>0} bytes?Convert.ToHexString(SHA256.HashData(bytes)):null,f.EFFECTIVE_DATE,f.EXPIRY_DATE,DocumentDigest(f)):null;
        var facts=CompanyWorkOrderPermitDocumentFacts.Create(DocumentCatalog(context,query.Permit,closingLinks),query,expectedReference,information,docFacts,fileFacts,DateTime.UtcNow);if(deliver is not null && closing.File?.FILE_CONTENT is {} content)deliver(content.ToArray());return facts;
    }
}
